using Unity.Physics;

namespace Junk.Physics
{
    /*
     * 0: Static Environment
     * 1: Dynamic Environment
     * 2: Kinetic Environment
     * 3: Characters
     * 4: Vehicles
     * 5: Projectiles
     * 6: Character Controllers
     * 7: Character Sensors
     * 8: Ragdolls
     * 9: 
     * 10: Character Kinematic Movement Collision
     * 11: Environment Triggers(doors and platforms etc)
     * 12: Environment Collision
     * 13: Pickup Collision
     *
     * missing: Trigger
     */
    
    public static class Layers
    {
        public static uint Environment(bool category0, bool category1, bool category2)
        {
            uint result = 0;
            result |= (category0 ? 1u : 0u) << 0;
            result |= (category1 ? 1u : 0u) << 1;
            result |= (category2 ? 1u : 0u) << 2;
            return result;
        }
        
        /// <summary>
        /// All the layers the map geometry should collide with
        /// </summary>
        public static uint WorldCollisionMatrix(bool ignoreCharacterControllers)
        {
            uint result = 0;
            result |= 1u << 0;
            result |= 1u << 1;
            result |= 1u << 2;
            result |= 1u << 3;
            result |= 1u << 4;
            result |= 1u << 5;
            if(!ignoreCharacterControllers)
                result |= 1u << 6;
            result |= 1u << 7;
            result |= 1u << 8;
            return result;
        }
        
        

        /// <summary>
        /// Layers 0, 1
        /// </summary>
        public static uint EnvironmentStatic()
        {
            uint result = 0;
            result |= (1u) << 0;
            return result;
        }
        
        /// <summary>
        /// Layers 0, 1
        /// </summary>
        public static uint EnvironmentStaticKinematic()
        {
            uint result = 0;
            result |= (1u) << 0;
            result |= (1u) << 1;
            return result;
        }
        
        /// <summary>
        /// Layers 0, 1, 2, 3
        /// </summary>
        public static uint EnvironmentAndCharacters()
        {
            uint result = 0;
            result |= (1u) << 0;
            result |= (1u) << 1;
            result |= (1u) << 2;
            result |= (1u) << 3;
            return result;
        }
        
        public static uint KinematicCharacterBelongsTo()
        {
            uint result = 0;
            result |= 0U;
            result |= 0U;
            result |= 0U;
            result |= (1u) << 3;
            return result;
        }
        
        public static uint KinematicCharacterCollidesWith()
        {
            uint result = 0;
            result |= (1u) << 0;
            result |= (1u) << 1;
            result |= (1u) << 2;
            result |= (1u) << 3;
            result |= (1u) << 4;
            result |= (1u) << 5;
            result |= (1u) << 6;
            return result;
        }
        
        public static uint Pickup()
        {
            uint result = 0;
            result |= (1u) << 13;
            return result;
        }
        
        /// <summary>
        /// Layers 3
        /// </summary>
        public static uint OnlyCharacters()
        {
            uint result = 0;
            result |= (1u) << 3;
            return result;
        }
        
        /// <summary>
        /// Layers 3
        /// </summary>
        public static uint OnlyCharacterControllers()
        {
            uint result = 0;
            result |= (1u) << 6;
            return result;
        }
        /// <summary>
        /// This filter belongs to characters layer and collides with:
        /// other characters, statics, dynamics, kinematics, vehicles, bullets, projectiles, items
        /// </summary>
        /// <returns></returns>
        public static CollisionFilter CharacterTraces()
        {
            return new CollisionFilter
            {
                BelongsTo = KinematicCharacterBelongsTo(),
                CollidesWith = KinematicCharacterCollidesWith(),
                GroupIndex = 0
            };
        }
        
        /// <summary>
        /// Environment Trigger layer
        /// </summary>
        public static uint EnvironmentTriggers()
        {
            uint result = 0;
            result |= 1u << 11;
            return result;
        }

    }
}