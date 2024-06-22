using Unity.Physics.Authoring;

namespace Junk.Physics.Hybrid
{
    public static class DefaultPhysics
    {
        /*
         * 0: Static Environment
         * 1: Dynamic Environment
         * 2: Kinetic Environment
         * 3: Characters
         * 4: Vehicles
         * 5: Projectiles
         * 6: Ground Sensors
         * 7: Character Sensors
         * 8: Ragdolls
         * 9:
         * 10: Character Kinematic Movement Collision
         * 11:
         * 12: Environment Collision
         * 13: Pickup Collision
         * 
         * missing: Trigger
         */
        
        public static PhysicsCategoryTags RagdollRigidbodyKinematicBelongsTo()
        {
            var belongsTo = new PhysicsCategoryTags();
            belongsTo.Category08 = true; // Ragdolls
            return belongsTo;
        }
        
        /// <summary>
        /// Character predefined filters
        /// todo rename for character ragdoll
        /// </summary>
        public static PhysicsCategoryTags KinematicBelongsTo()
        {
            var belongsTo = new PhysicsCategoryTags();
            belongsTo.Category03 = true; // Characters
            belongsTo.Category08 = true; // Ragdolls
            return belongsTo;
        }
        
        /// <summary>
        /// Character predefined filters
        /// todo rename for character ragdoll
        /// </summary>
        public static PhysicsCategoryTags KinematicCollidesWith()
        {
            var belongsTo = new PhysicsCategoryTags();
            //belongsTo.Category00 = true; // Bullets
            belongsTo.Category01 = true; // Bullets
            belongsTo.Category02 = true; // Bullets
            //belongsTo.Category03 = true; // Bullets
            //belongsTo.Category04 = true; // Bullets
            belongsTo.Category05 = true; // Bullets
            return belongsTo;
        }
        
        /// <summary>
        /// Character predefined filters
        /// todo rename for character ragdoll
        /// </summary>
        public static PhysicsCategoryTags Ragdoll_DynamicBelongsTo()
        {
            var belongsTo = new PhysicsCategoryTags();
            belongsTo.Category10 = true;  // Character Ragdoll Collision
            return belongsTo;
        }
        
        /// <summary>
        /// Character predefined filters
        /// todo rename for character ragdoll
        /// </summary>
        public static PhysicsCategoryTags Ragdoll_DynamicCollidesWith()
        {
            var belongsTo = new PhysicsCategoryTags(); 
            belongsTo.Category00 = true; // Static Env
            belongsTo.Category01 = true; // Kineamtic Env
            belongsTo.Category02 = true; // Dynamic Env
            belongsTo.Category03 = true; // Characters
            belongsTo.Category04 = true; // Vehicles 
            belongsTo.Category05 = true; // Projectiles 
            belongsTo.Category10 = true; // Character Ragdoll Collision
            return belongsTo;
        }
        
        /// <summary>
        /// Character predefined filters
        /// todo rename for character ragdoll
        /// </summary>
        public static PhysicsCategoryTags Ragdoll_BulletsCollidesWith()
        {
            var belongsTo = new PhysicsCategoryTags(); 
            belongsTo.Category05 = true; // Projectiles 
            return belongsTo;
        }
        
        // Characters
        /// <summary>
        /// Character overlap belongs to and collides with Characters layer(03)
        /// </summary>
        public static PhysicsCategoryTags CharacterOverlapQueries()
        {
            var belongsTo = new PhysicsCategoryTags();
            belongsTo.Category03 = true; // Characters
            return belongsTo;
        }
        
    }
}