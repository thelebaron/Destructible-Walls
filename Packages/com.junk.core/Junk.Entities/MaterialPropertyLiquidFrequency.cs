using Unity.Entities;
using Unity.Rendering;

namespace Junk.Entities
{
    [MaterialProperty("_LiquidFrequency")]
    public struct MaterialPropertyLiquidFrequency : IComponentData, IEnableableComponent
    {
        public float Value;
    }
}