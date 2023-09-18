using System;

namespace Junk.Entities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
    public class EditorOnlyAttribute : Attribute
    {
        
    }
}