using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;

using Icarus.Misc;

namespace Icarus.Orbit {
    public struct OrbitRenderingEnabled : IComponentData {}
    public struct OrbitRenderingDisabled : IComponentData {}
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    public partial class UpdateOrbitalRenderingSystem : SystemBase {
        private EntityQuery EnableRendering;
        private EntityQuery DisableRendering;
        private BufferLookup<LinkedEntityGroup> ChildrenLookup;
        
        [BurstCompile]
        protected override void OnCreate() {
            EnableRendering = new EntityQueryBuilder(Allocator.TempJob)
                .WithAll<OrbitRenderingEnabled, DisableRendering>()
                .Build(this);
            DisableRendering = new EntityQueryBuilder(Allocator.TempJob)
                .WithAll<OrbitRenderingDisabled>()
                .WithNone<DisableRendering>()
                .Build(this);
            ChildrenLookup = GetBufferLookup<LinkedEntityGroup>(true);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            var ecb0 = new EntityCommandBuffer(Allocator.TempJob);
            var ecb1 = new EntityCommandBuffer(Allocator.TempJob);
            ChildrenLookup.Update(this);

            // enable rendering
            var job0 = new PerformRenderingTagUpdate {
                pecb = ecb0.AsParallelWriter(),
                children = ChildrenLookup,
                add = false
            }.ScheduleParallel(EnableRendering, this.Dependency);
            
            // disable rendering
            var job1 = new PerformRenderingTagUpdate {
                pecb = ecb1.AsParallelWriter(),
                children = ChildrenLookup,
                add = true
            }.ScheduleParallel(DisableRendering, this.Dependency);

            this.Dependency = JobHandle.CombineDependencies(job0, job1);
            this.Dependency.Complete();
            
            ecb0.Playback(this.EntityManager);
            ecb0.Dispose();
            ecb1.Playback(this.EntityManager);
            ecb1.Dispose();
        }
    }
    
    [BurstCompile]
    [WithAll(typeof(NeverMatchTag))] // disable internal query generation
    public partial struct PerformRenderingTagUpdate : IJobEntity {
        [ReadOnly]
        public bool add;
        [ReadOnly]
        public BufferLookup<LinkedEntityGroup> children;
        public EntityCommandBuffer.ParallelWriter pecb;
        
        [BurstCompile]
        public void Execute(Entity entity, [ChunkIndexInQuery] int index) {
            if (!children.HasBuffer(entity)) return;
            var buffer = children[entity];

            for (int i=0; i<buffer.Length; i++) {
                if (add) {
                    pecb.AddComponent<DisableRendering>(index, buffer[i].Value);
                } else {
                    pecb.RemoveComponent<DisableRendering>(index, buffer[i].Value);
                }
            }
        }
    }
}
