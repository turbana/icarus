using Unity.Collections;
using Unity.Entities;
using UnityEngine;

using Icarus.Orbit;

namespace Icarus.Test {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(TestSystemGroup))]
    public partial class TestControlsSystem : SystemBase {
        public static readonly string MAIN_KEY = "t";
        private const string TIME_UP = "=";
        private const string TIME_DOWN = "-";
        private const string TIME_RESET = "0";
        
        protected override void OnUpdate() {
            if (Input.GetKey(MAIN_KEY)) {
                EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
                Entities
                    .ForEach((Entity e, ref OrbitalOptions oo) => {
                        if (Input.GetKeyDown(TIME_UP)) {
                            ecb.AddComponent<TimeControlUpdate>(e, new TimeControlUpdate {Modifier = 2f});
                        } else if (Input.GetKeyDown(TIME_DOWN)) {
                            ecb.AddComponent<TimeControlUpdate>(e, new TimeControlUpdate {Modifier = 0.5f});
                        } else if (Input.GetKeyDown(TIME_RESET)) {
                            ecb.AddComponent<TimeControlUpdate>(e, new TimeControlUpdate {Modifier = -1f});
                        }
                    })
                    .WithoutBurst()
                    .Run();
                this.Dependency.Complete();
                ecb.Playback(this.EntityManager);
                ecb.Dispose();
            }
        }
    }
}
