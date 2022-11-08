using Unity.Collections;
using Unity.Entities;

using Icarus.Orbit;

namespace Icarus.Test {
    public struct TimeControlUpdate : IComponentData {
        public float Modifier;
    }
    
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(TestSystemGroup))]
    [UpdateAfter(typeof(TestControlsSystem))]
    public partial class TestTimeControlSystem : SystemBase {
        protected override void OnUpdate() {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            Entities
                .ForEach((Entity e, ref TimeControlUpdate update, ref OrbitalOptions oo) => {
                    float scale = oo.TimeScale * update.Modifier;
                    if (scale < 0f) scale = 1f;
                    oo.TimeScale = scale;
                    ecb.RemoveComponent<TimeControlUpdate>(e);
                    UnityEngine.Debug.Log($"time scale = {scale}x");
                })
                .WithoutBurst()
                .Schedule();
            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
        }
    }
}
