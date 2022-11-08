using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateSolarPositionSystem))]
    public partial class UpdateGamePositionSystem : SystemBase {
        protected override void OnUpdate() {
            // Entity player = SystemAPI.GetSingletonEntity<PlayerOrbitTag>();
            // TransformAspect playerTransform =
            //     SystemAPI.GetAspectRO<TransformAspect>(player);
            // float3 playerPos = playerTransform.Position;
            // float3 playerPos =
            //     GetComponent<OrbitalParameters>(
            //         GetSingletonEntity<PlayerOrbitTag>())
            //     .SolarPosition;

            Entity player = GetSingletonEntity<PlayerOrbitTag>();
            // Entity parent = GetComponent<Parent>(player).Value;
            Entity parent = GetComponent<OrbitalParent>(player).Value;
            OrbitalParameters pparms = GetComponent<OrbitalParameters>(player);
            float3 playerPos = pparms.ParentPosition;
            // float3 playerPos = pparms.SolarPosition;

            // EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            // EntityCommandBuffer.ParallelWriter pecb = ecb.AsParallelWriter();
            
            Entities
                // don't run on player orbit
                .WithNone<PlayerOrbitTag>()
                .ForEach(
                    (int entityInQueryIndex, Entity entity,
                     ref TransformAspect transform, in OrbitalParameters parms,
                     in OrbitalScale scale) =>
                    {
                        float rscale;
                        float X = 149597870.700f;
                        // find new position
                        // float3 pos = parms.SolarPosition - playerPos;
                        float3 pos = ((entity == parent) ? float3.zero : parms.ParentPosition);
                        // float3 pos = parms.SolarPosition;
                        // find new scale
                        float dist = math.distance(pos, playerPos);
                        pos = pos - playerPos;
                        float sdist = dist - scale.Radius;
                        float desired = 1000f + math.sqrt(sdist / X) * 100f;
                        // float dscale = desired / sdist;
                        // rscale = (scale.Radius) * (desired / sdist) * 1f;
                        // float a = math.atan(scale.Radius / dist);
                        // float A = math.tan(a);
                        float A = scale.Radius / dist;
                        float S = -((A * desired) / (A - 1f));
                        rscale = S * 2f;
                        float3 newpos = math.normalize(pos) * (desired + rscale / 2f);
                        // UnityEngine.Debug.Log(
                        //     $"0 id={entityInQueryIndex} pos={pos} dist={dist} r={scale.Radius} sdist={sdist} desired={desired} rscale={rscale} newpos={newpos}");
                        // UnityEngine.Debug.Log(
                        //     $"1 id={entityInQueryIndex} theta={parms.Theta} dist={parms.ParentDistance} ptheta={pparms.Theta} pdist={pparms.ParentDistance}");
                        // UnityEngine.Debug.Log(
                        //     $"2 id={entityInQueryIndex} Pos={parms.ParentPosition} pPos={pparms.ParentPosition}");
                        // float newdist = math.distance(float3.zero, newpos);
                        // UnityEngine.Debug.Log(
                        //     $"3 id={entityInQueryIndex} newdist={newdist}");
                        if (dist < 1000f) {
                            newpos = pos;
                            rscale = 1f;
                        }
                        var ltw = transform.LocalToWorld;
                        ltw.Position = newpos;
                        ltw.Scale = rscale;
                        transform.LocalToWorld = ltw;
                        // transform.Position = newpos;
                    })
                .WithoutBurst()
                .ScheduleParallel();
        }
    }
}
