using System.Collections.Generic;
using System.Linq;
using thelebaron.Damage;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;

namespace Destructibles
{
    public struct Strain : IComponentData
    {
        public float Current;
        public float Threshold;
    }
}