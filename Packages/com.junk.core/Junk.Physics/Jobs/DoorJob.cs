using Junk.Entities;
using Junk.Math;
using Junk.Transforms;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Junk.Physics
{
    [BurstCompile]
    public partial struct DoorJob : IJobEntity
    {
        public float                           DeltaTime;
        public ComponentLookup<TriggerUse>     DoorTriggerLookup;
        public ComponentLookup<LocalTransform> LocalTransformLookup;
        public ComponentLookup<LocalToWorld>   LocalToWorldLookup;

        public void Execute(Entity entity, ref Door door, ref RotationEulerXYZ rotationEulerXYZ, EnabledRefRW<Active> active)
        {
            // State behaviour setup
            // If door is not moving and received use command
            if(active.ValueRO)
            {
                //Debug.Log($"DoorJob use");
                switch (door.State)
                {
                    case DoorState.Closed:
                        door.State = DoorState.Opening;
                        break;
                    case DoorState.Opened:
                        door.State = DoorState.Closing;
                        break;
                    case DoorState.Opening:
                        break;
                    case DoorState.Closing:
                        break;
                    default:
                        break;
                }
            }

            // Movement behaviour
            if (door.State == DoorState.Opening)
            {                    
                if (door.MovementType == DoorMovementType.Rotational)
                {
                    // Tick the movement
                    if (door.CurrentMoveTime < door.MovementDuration)
                    {
                        door.CurrentMoveTime += DeltaTime;


                            rotationEulerXYZ.Value.x = math.select(rotationEulerXYZ.Value.x, rotationEulerXYZ.Value.x + DeltaTime * door.Speed / 1, door.Axis.x);
                            rotationEulerXYZ.Value.y = math.select(rotationEulerXYZ.Value.y, rotationEulerXYZ.Value.y + DeltaTime * door.Speed / 1, door.Axis.y);
                            rotationEulerXYZ.Value.z = math.select(rotationEulerXYZ.Value.z, rotationEulerXYZ.Value.z + DeltaTime * door.Speed / 1, door.Axis.z);
                        
                        // If finished, set state and reset timers
                        if (door.CurrentMoveTime > door.MovementDuration)
                        {
                            door.CurrentMoveTime = 0;
                            door.State           = DoorState.Opened;
                        }
                    }
                }

                if (door.MovementType == DoorMovementType.Linear)
                {
                    var localTransform = LocalTransformLookup[entity];
                    var increment    = DeltaTime * door.Speed;
                    localTransform.Position = math.lerp(localTransform.Position, door.OpenPosition, increment);
                    
                    if(maths.approximately(localTransform.Position, door.OpenPosition))
                        door.State           = DoorState.Opened;
                    
                    LocalTransformLookup[entity] = localTransform;
                }
                

                var localToWorld = LocalToWorldLookup[entity];

                var colliderLocalTransform = LocalTransformLookup[door.ColliderEntity];
                colliderLocalTransform.Position = localToWorld.Position;
                colliderLocalTransform.Rotation = new quaternion(localToWorld.Value);
                //LocalTransformLookup[door.ColliderEntity] = colliderLocalTransform;
            }
            if (door.State == DoorState.Closing)
            {
                if (door.MovementType == DoorMovementType.Rotational)
                {
                    // Tick the movement
                    if (door.CurrentMoveTime < door.MovementDuration)
                    {
                        door.CurrentMoveTime += DeltaTime;

                        //translation.Value -= maths.up * FixedTime * door.Speed/3;
                        rotationEulerXYZ.Value.y -= DeltaTime * door.Speed / 1;
                    }

                    // If finished, set state and reset timers
                    if (door.CurrentMoveTime > door.MovementDuration)
                    {
                        door.CurrentMoveTime = 0;
                        door.State           = DoorState.Closed;
                    }
                }

                if (door.MovementType == DoorMovementType.Linear)
                {
                    var localTransform = LocalTransformLookup[entity];
                    var increment      = DeltaTime * door.Speed;
                    localTransform.Position = math.lerp(localTransform.Position, door.ClosedPosition, increment);

                    if (maths.approximately(localTransform.Position, door.ClosedPosition))
                    {
                        door.State           = DoorState.Closed;
                        active.ValueRW = false;
                    }
                    
                    LocalTransformLookup[entity] = localTransform;
                }
                
                var localToWorld = LocalToWorldLookup[entity];

                var colliderLocalTransform = LocalTransformLookup[door.ColliderEntity];
                colliderLocalTransform.Position = localToWorld.Position;
                colliderLocalTransform.Rotation = new quaternion(localToWorld.Value);
            }
        }
    }
}