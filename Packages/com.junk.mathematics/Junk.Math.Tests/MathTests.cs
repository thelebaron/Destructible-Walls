using System.Collections;
using System.IO;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Junk.Math.Tests
{
    public class MathTests
    {

        [SetUp]
        public void Setup()
        {

        }

        [TearDown]
        protected void TearDown()
        {

        }
        
        
        //[Test]
        public void QuaternionToEulerAngles()
        {
            var x = 0;
            var y = 135;
            var z = 0;
            var eulers = new float3(x,y,z);
            
            var quaternion = Quaternion.Euler(eulers);
            var UnityEngineEulerAnlge = quaternion.eulerAngles;

            var q           = Unity.Mathematics.quaternion.EulerXYZ(eulers);
            var e           = q.eulerXYZ();
            Assert.AreEqual(e, (float3)eulers);
        }
        

        const int kVersion          = 51;
        const int kIncorrectVersion = 13;
        

        //[Test]
        
        public void ParallelSurfaceDirection()
        {
            //check SurfaceParallelFromDirection with unityengine type
        }


        
    }
}