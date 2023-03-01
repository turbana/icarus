using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

using Icarus.Loading;

/* Physics entities such as colliders are un-parented and set as Static during
 * the baking process. This is not desired as we have interaction colliders
 * that should move/rotate with their parent entity. This system will run once
 * and re-add the parent/child relationship. */

namespace Icarus.Misc {
    [BurstCompile]
    [UpdateInGroup(typeof(IcarusLoadingSystemGroup))]
    public partial class ReParentSystem : SystemBase {
        [ReadOnly]
        private ComponentLookup<LocalToWorld> LTWLookup;
        
        [BurstCompile]
        protected override void OnCreate() {
            LTWLookup = GetComponentLookup<LocalToWorld>(true);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            LTWLookup.Update(this);
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            new ReParentJob {
                LTWLookup = LTWLookup,
                ecb = ecb,
            }.Schedule();
            
            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
            
            this.Enabled = false;
        }

        [BurstCompile]
        private partial struct ReParentJob : IJobEntity {
            [ReadOnly]
            public ComponentLookup<LocalToWorld> LTWLookup;
            public EntityCommandBuffer ecb;

            [BurstCompile]
            public void Execute(Entity child, in ReParent reparent) {
                var parent = reparent.Value;
                var pltw = LTWLookup[parent].Value;
                var cltw = LTWLookup[child].Value;
                var pwt = WorldTransform.FromMatrix(pltw);
                var cwt = WorldTransform.FromMatrix(cltw);
                var clt = (LocalTransform)pwt.InverseTransformTransform(cwt);
                
                // update child entity
                ecb.AddComponent<Parent>(child, new Parent { Value = parent });
                ecb.AddComponent<LocalTransform>(child, clt);
                ecb.RemoveComponent<Static>(child);
                ecb.RemoveComponent<ReParent>(child);
            }
        }
    }
}
