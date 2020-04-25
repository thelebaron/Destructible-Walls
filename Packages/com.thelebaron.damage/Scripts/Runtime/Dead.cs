using Unity.Entities;

namespace thelebaron.damage
{
    /// <summary>
    /// Tag component - Exclude or include from certain systems using this.
    /// </summary>
    [GenerateAuthoringComponent]
    public struct Dead : IComponentData { }

}