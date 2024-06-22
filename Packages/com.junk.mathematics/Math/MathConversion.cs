using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Junk.Math
{
    public static partial class maths
    {

    }
    
    public struct ByteBool
    {
        public byte Value;
        
        public ByteBool(bool value)
        {
            Value = value ? (byte) 1 : (byte) 0;
        }
        
        public static implicit operator bool(ByteBool value)
        {
            return value.Value == 1;
        }
        
        public static implicit operator ByteBool(bool value)
        {
            return new ByteBool(value);
        }
        
        public static implicit operator byte(ByteBool value)
        {
            return value.Value;
        }
        
        public static implicit operator ByteBool(byte value)
        {
            return new ByteBool(value == 1);
        }
    }
    
    
    public static partial class maths
    {
        public static float4 ToFloat4(this Color color)
        {
            return new float4(color.r, color.b, color.g, color.a);
        }
        
        public static bool AsBool(this byte value)
        {
            if (value == 1)
                return true;
            if (value == 0)
                return false;
            
            throw new System.Exception("Byte bool value set outside bounds of 0 or 1");
        }
        
        
        
        public static bool IsTrue(this byte value)
        {
            return value == 1;
        }
        
        public static bool IsFalse(this byte value)
        {
            return value == 0;
        }
        
        public static byte ToByte(this bool value)
        {
            return (byte) (value ? 1 : 0);
        }

        /*public float toAxisAngle(quaternion q)
        {
            var qq = new Quaternion();
            qq.ToAngleAxis();
        }*/
        
    }

}