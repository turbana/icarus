using Unity.Entities;
using Unity.Physics;

namespace Icarus.Misc {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(Unity.Physics.Systems.PhysicsSystemGroup))]
    public partial class IcarusSimulationSystemGroup : ComponentSystemGroup {}
}
