using Unity.Entities;

namespace Junk.Entities.Hybrid
{
    public partial class GameMenuRef : IComponentData
    {
        // this is a monobehaviour reference
        public GameMenuBehaviour GameMenuBehaviour;
    }

}