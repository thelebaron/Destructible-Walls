using System;
using System.Collections.Generic;
using Junk.Core.Creation;
using UnityEngine;

namespace Junk.Destroy.Hybrid
{
    /// <summary>
    /// Note the reason this isnt just a class is so we can inspect this as its own
    /// unique thing in the editor inspector.
    /// </summary>
    public class FractureChild : ScriptableObject
    {
        [HideInInspector] public FractureCache Parent;
        public                   Material      InsideMaterial;
        public                   Material      OutsideMaterial;
        public                   Shape         Shape;
        [HideInInspector] public FractureCache FractureCache;

        public FractureCache GetRoot()
        {
            /*var parent = Parent.Parent;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }*/
            return null;
        }
    }
}