using Unity.Entities;

namespace Icarus.UI {
    /* UserInputSystemGroup is where user input in queried */
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst=true)]
    [UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    public partial class UserInputSystemGroup : ComponentSystemGroup {}
    
    /* InteractionSystemGroup is where any systems that need to run each frame
     * are performed */
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class IcarusInteractionSystemGroup : ComponentSystemGroup {}
}
