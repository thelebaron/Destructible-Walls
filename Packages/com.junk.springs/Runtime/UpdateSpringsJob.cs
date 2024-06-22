using Junk.Math;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Springs
{
    public partial struct SpringSystem
    {
        [BurstCompile]
        private struct UpdateSpringsJob : IJobChunk
        {
            [ReadOnly] public float                       DeltaTime;
            [ReadOnly] public float                       ElapsedTime;
            public            ComponentTypeHandle<Spring> SpringType;
            public            BufferTypeHandle<SoftForce> SoftForceType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var springs         = chunk.GetNativeArray(ref SpringType);
                var softForceBuffer = chunk.GetBufferAccessor(ref SoftForceType);
                
                var entityEnumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (entityEnumerator.NextEntityIndex(out int i))
                {
                    var enabled    = chunk.IsComponentEnabled(ref SpringType, i);
                    var spring     = springs[i];
                    var softForces = softForceBuffer[i];

                    spring.PreviousValue = spring.Value;
                    
                    // Springs update too slowly without this, though seems inelegant
                    ElapsedTime *= 3f;
                    
                    Stop(ref softForces, ref spring);
                
                    if (HasPendingForces(spring))
                    {
                        AddForceInternal(ref spring, spring.InternalForce, DeltaTime);
                        ClearForces(ref spring);
                    }
                    // HasPendingSoftForces
                    if (HasPendingSoftForces(spring))
                    {
                        AddSoftForce(ref softForces, ref spring, spring.InternalForce,spring.InternalFrames, DeltaTime);
                        ClearForces(ref spring);
                    }
                    
                    // handle forced velocity fade in
                    if (spring.VelocityFadeInEndTime > ElapsedTime)
                        spring.VelocityFadeInCap = math.clamp(1 - (spring.VelocityFadeInEndTime - ElapsedTime) / spring.VelocityFadeInLength, 0, 1);// Mathf.Clamp01(1 - ((spring.VelocityFadeInEndTime - ElapsedTime) / spring.VelocityFadeInLength));
                    else
                        spring.VelocityFadeInCap = 1.0f;

                    // handle smooth force
                    if (!softForces[0].Force.Equals(float3.zero))
                    {
                        AddForceInternal(ref spring, softForces[0].Force, DeltaTime);
                        for (int v = 0; v < 120; v++)
                        {
                            softForces[v] = (v < 119) ? softForces[v + 1].Force : float3.zero;
                            if (softForces[v].Force.Equals(float3.zero))
                                break;
                        }
                    }

                    /*if (spring.OverrideRestState)
                    {
                        spring.RestState = spring.Value;
                        spring.OverrideRestState = false;
                    }*/
                    CalculateState(ref spring, DeltaTime);
                    CalculateDelta(ref spring);
                    
                    springs[i] = spring;
                }
            }

            private bool HasPendingForces(Spring spring)
            {
                return !spring.InternalForce.Equals(float3.zero);
            }

            private bool HasPendingSoftForces(Spring spring)
            {
                return !spring.InternalSoftForce.Equals(float3.zero);
            }


            /// <summary>
            /// CalculateState method is responsible for calculating the state of the spring 
            /// system at a given time. The method takes a float parameter deltaTime, which
            /// represents the time elapsed since the last update, and updates the state of 
            /// the spring system accordingly.

            /// The CalculateState method first calculates the acceleration of the spring system 
            /// based on its current position, velocity, and the spring force. It then updates 
            /// the velocity and position of the spring system based on the calculated 
            /// acceleration and the elapsed time.
            /// </summary>
            private void CalculateState(ref Spring spring, float deltaTime)
            {
                if (spring.Value.Equals(spring.RestState))
                    return;
            
                ref var springBlogData = ref spring.SpringLimitsReference.Value;
            
                // add rest state distance * stiffness to velocity
                spring.Velocity += maths.scale((spring.RestState - spring.Value), spring.Stiffness);
                // dampen velocity
                spring.Velocity = maths.scale(spring.Velocity, spring.Damping); 

                // clamp velocity to maximum
                spring.Velocity = maths.clampMagnitude(spring.Velocity, springBlogData.MaxVelocity);//spring.MaxVelocity

                // apply velocity, or stop if velocity is below minimum
                if (math.lengthsq(spring.Velocity) > (springBlogData.MinVelocity * springBlogData.MinVelocity)) //spring.MinVelocity
                    Move(ref spring, deltaTime);
                else
                    Reset(ref spring);
            }
            
                
            
            /// <summary>
            /// adds velocity to the state and clamps state between min
            /// and max values
            /// </summary>
            private static void Move(ref Spring spring, float deltaTime)
            {
                ref var springData = ref spring.SpringLimitsReference.Value;
                
                spring.Value   += spring.Velocity * deltaTime * 60.0f;
                spring.Value.x =  math.clamp(spring.Value.x, springData.MinState.x, springData.MaxState.x);//Mathf.Clamp(spring.State.x, spring.MinState.x, spring.MaxState.x);
                spring.Value.y =  math.clamp(spring.Value.y, springData.MinState.y, springData.MaxState.y);
                spring.Value.z =  math.clamp(spring.Value.z, springData.MinState.z, springData.MaxState.z);
            }


            /// <summary>
            /// stops spring velocity and resets state to the static
            /// equilibrium
            /// </summary>
            private static void Reset(ref Spring spring)
            {
                spring.Velocity = float3.zero;
                spring.Value    = spring.RestState;
            }
            
            /// <summary>
            /// adds external velocity to the spring in one frame
            /// </summary>
            private static void AddForceInternal(ref Spring spring, float3 force, float deltaTime)
            {
                ref var springData = ref spring.SpringLimitsReference.Value;
                
                force           *= spring.VelocityFadeInCap;
                spring.Velocity += force;
                spring.Velocity =  maths.clampMagnitude(spring.Velocity, springData.MaxVelocity);
            
                Move(ref spring, deltaTime);
            }
            
            /*
            /// <summary>
            /// adds external velocity to the spring in one frame
            /// </summary>
            public static void AddForce(ref DynamicBuffer<SoftForceElement> softForce, ref Spring spring, float3 force, float timeScale)
            {
                if (timeScale < 1.0f)
                    AddSoftForce(ref softForce, ref spring, force, 1, timeScale);
                else
                    AddForceInternal(ref spring, force, timeScale);
            }*/
        


            /// <summary>
            /// adds a force distributed over up to 120 fixed frames
            /// </summary>
            private static void AddSoftForce(ref DynamicBuffer<SoftForce> softForce, ref Spring spring, float3 force, float frames, float deltaTime)
            {
                force /= deltaTime * 60.0f;

                frames = math.clamp(frames, 1, 120);

                AddForceInternal(ref spring, force / frames, deltaTime);

                for (int v = 0; v < (math.round(frames) - 1); v++)
                {
                    softForce[v] += (force / frames);
                }
            }




            /// <summary>
            /// stops spring velocity
            /// </summary>
            private static void Stop(ref DynamicBuffer<SoftForce> softForce, ref Spring spring)
            {
                if (spring.Stop)
                {
                    spring.Velocity = float3.zero;
                    spring.Stop     = false;
                    
                    if (spring.StopAndIncludeSoftForce)
                    {
                        StopSoftForce(ref softForce);
                        spring.StopAndIncludeSoftForce = false;
                    }
                }
                

            }

            public void StopSpring(bool includeSoftForce = false)
            {
                //Stop                    = true;
                //StopAndIncludeSoftForce = includeSoftForce;
            }

            /// <summary>
            /// stops soft force
            /// </summary>
            private static void StopSoftForce(ref DynamicBuffer<SoftForce> softForce)
            {
                for (int v = 0; v < 120; v++)
                {
                    softForce[v] = float3.zero;
                }
            }


            /// <summary>
            /// instantly kills any forces added to the spring, gradually
            /// easing them back in over 'seconds'.
            /// this is useful when you need a spring to freeze up for a
            /// brief amount of time, then slowly relaxing back to normal.
            /// </summary>
            private void ForceVelocityFadeIn(ref Spring spring, float seconds, float elapsedTime)
            {
                spring.VelocityFadeInLength  = seconds;
                spring.VelocityFadeInEndTime = elapsedTime + seconds;
                spring.VelocityFadeInCap     = 0.0f;
            }
            
            /// <summary>
            /// clears all internal forces for the spring
            /// </summary>
            private static void ClearForces(ref Spring spring)
            {
                spring.InternalFrames    = 0;
                spring.InternalSoftForce = float3.zero;
                spring.InternalForce     = float3.zero;
            }
            
            /// <summary>
            /// per frame difference in value
            /// </summary>
            /// <param name="spring"></param>
            private void CalculateDelta(ref Spring spring)
            {
                spring.Delta = spring.PreviousValue - spring.Value;
            }
        }
    }
}