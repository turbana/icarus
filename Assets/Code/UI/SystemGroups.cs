using Unity.CharacterController;
using Unity.Entities;

using Icarus.Loading;

namespace Icarus.UI {
    /* UserInputSystemGroup is where user input in queried */
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(IcarusLoadingSystemGroup))]
    public partial class UserInputSystemGroup : ComponentSystemGroup {}
    
    /* InteractionSystemGroup is where any systems that need to run each frame
     * are performed */
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class IcarusInteractionSystemGroup : ComponentSystemGroup {}

    /* IcarusPresentationSystemGroup is where we update any objects or meshes
     * to match what the Simulation systems updated. */
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(Unity.Rendering.EntitiesGraphicsSystem))]
    public partial class IcarusPresentationSystemGroup : ComponentSystemGroup {}
}
