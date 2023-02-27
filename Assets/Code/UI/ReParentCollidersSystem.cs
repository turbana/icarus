using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

/* Physics entities such as colliders are un-parented and set as Static during
 * the baking process. This is not desired as we have interaction colliders
 * that should move/rotate with their parent entity. This system will run once
 * and re-add the parent/child relationship. */

namespace Icarus.UI {
    [BurstCompile]
    [UpdateInGroup(typeof(UpdateInteractionSystemGroup))]
    public partial class ReParentCollidersSystem : SystemBase {
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
                LTW = LTWLookup,
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
            public ComponentLookup<LocalToWorld> LTW;
            public EntityCommandBuffer ecb;

            [BurstCompile]
            public void Execute(Entity child, in InteractionControl control) {
                var parent = control.Control;
                var pltw = LTW[parent].Value;
                var cltw = LTW[child].Value;
                var pwt = WorldTransform.FromMatrix(pltw);
                var cwt = WorldTransform.FromMatrix(cltw);
                var plt = LocalTransform.FromMatrix(pltw);
                var clt = (LocalTransform)pwt.InverseTransformTransform(cwt);
                    
                // update child
                ecb.AddComponent<Parent>(child, new Parent { Value = parent });
                ecb.RemoveComponent<Static>(child);
                ecb.AddComponent<LocalTransform>(child, clt);

                // update parent
                ecb.RemoveComponent<Static>(parent);
                ecb.AddComponent<LocalTransform>(parent, plt);
            }
        }
    }
}
