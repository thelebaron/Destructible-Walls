using System;
using Unity.Entities;
using UnityEngine;

namespace Junk.Entities
{
    [Serializable]
    public struct UnmanagedLightData : IComponentData, IEnableableComponent
    {
        public float Intensity;
        public float Range;
        public Color Color;
        public bool  Enabled;
        public float ShadowStrength;

        public static UnmanagedLightData GetDefault()
        {
            return new UnmanagedLightData
            {
                Intensity      = 1,
                Range          = 10,
                Color          = Color.white,
                Enabled        = true,
                ShadowStrength = 1
            };
        }
    }
    
    public struct UnmanagedLightReference : IComponentData, IEnableableComponent
    {
        public Entity Entity;
    }
}