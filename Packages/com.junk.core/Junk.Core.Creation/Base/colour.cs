
using System;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace Junk.Core.Creation
{
    /// <summary>
    ///   <para>Representation of RGBA colors. Serializable.</para>
    /// </summary>
    [Serializable]
    public struct colour
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public static implicit operator Color(colour c)
        {
            return new Color
            {
                r = c.r,
                g = c.g,
                b = c.b,
                a = c.a
            };
        }

        public static implicit operator colour(Color c)
        {
            return new colour
            {
                r = c.r,
                g = c.g,
                b = c.b,
                a = c.a
            };
        }
        /// <summary>
        ///   <para>Constructs a new color with given Color class.</para>
        /// </summary>
        public colour(Color c)
        {
            this.r = c.r;
            this.g = c.g;
            this.b = c.b;
            this.a = c.a;
        }

        /// <summary>
        ///   <para>Constructs a new color with given Color class.</para>
        /// </summary>
        public Color GetColor()
        {
            var c = new Color {r = r, g = g, b = b, a = a};
            return c;
        }

        public static Color[] ToArray(colour[] colors)
        {
            var array = new Color[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                array[i] = colors[i].GetColor();
            }

            return array;
        }
    }
}