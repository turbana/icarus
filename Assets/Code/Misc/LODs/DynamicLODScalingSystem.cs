using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

/*
 * LOD scaling in ECS is based on the distance from the camera to the distances
 * set in MeshLODGroupComponent (LODDistances0/1). Those distances are set
 * during bake and are not updated in realtime. As most of the objects we're
 * using are both moved and scaled in realtime (planets, ships, asteroids) we
 * need a system to keep the LOD distances in sync.
 *
 * These systems will save the initial LOD distances (using
 * InitialLODRangesComponent) and will use them along with the LocalToWorld's
 * scale value to update the LOD distances at runtime.
 *
 * NOTE: we need to set the LOD distances *after* TransformSystemGroup, not sure
 * why exactly. Possibly LocalToWorld isn't right before then?
 */

namespace Icarus.Misc {
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class DynamicLODScalingSystemGroup : ComponentSystemGroup {}

    public struct InitialLODRangesComponent : IComponentData {
        public float4 LODDistances0;
        public float4 LODDistances1;
    }
    
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(DynamicLODScalingSystemGroup))]
    public partial class SaveInitialLODRangesSystem : SystemBase {
        protected override void OnUpdate() {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            Entities
                .WithNone<InitialLODRangesComponent>()
                .ForEach((Entity entity, in MeshLODGroupComponent lod) => {
                    ecb.AddComponent<InitialLODRangesComponent>(
                        entity,
                        new InitialLODRangesComponent {
                            LODDistances0 = lod.LODDistances0,
                            LODDistances1 = lod.LODDistances1
                        });
                })
                .Schedule();
            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
        }
    }

    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(DynamicLODScalingSystemGroup))]
    [UpdateAfter(typeof(SaveInitialLODRangesSystem))]
    public partial class DynamicLODScalingSystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                .WithNone<DisableRendering>()
                .ForEach(
                    (ref MeshLODGroupComponent lod,
                     in InitialLODRangesComponent initial, in LocalTransform ltw) =>
                    {
                        lod.LODDistances0 = ltw.Scale * initial.LODDistances0;
                        lod.LODDistances1 = ltw.Scale * initial.LODDistances1;
                    })
                .ScheduleParallel();
        }
    }
}
