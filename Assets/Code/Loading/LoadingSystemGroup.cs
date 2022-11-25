using Unity.Entities;
using Unity.Scenes;

namespace Icarus.Loading {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(SceneSystemGroup))]
    public partial class LoadingSystemGroup : ComponentSystemGroup {}
}
