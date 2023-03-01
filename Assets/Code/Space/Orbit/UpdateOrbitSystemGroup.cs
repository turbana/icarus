using Unity.Entities;

using Icarus.Misc;

namespace Icarus.Orbit {
    [UpdateInGroup(typeof(IcarusSimulationSystemGroup))]
    public partial class UpdateOrbitSystemGroup : ComponentSystemGroup {}
}
