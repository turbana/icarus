using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateOrbitalPositionSystem))]
    public partial class UpdateGamePositionSystem : SystemBase {
        protected override void OnUpdate() {
            Entity player = GetSingletonEntity<PlayerOrbitTag>();
            Entity parent = GetComponent<OrbitalParent>(player).Value;
            float3 playerPos = GetComponent<OrbitalPosition>(player).LocalToParent.Position;

            Entities
                // don't run on player orbit
                .WithNone<PlayerOrbitTag>()
                .ForEach(
                    (int entityInQueryIndex, Entity entity,
                     ref TransformAspect transform,
                     in OrbitalPosition pos, in OrbitalParameters parms, in OrbitalScale scale) =>
                    {
                        float rscale;
                        float X = 149597870.700f;
                        // find new position
                        float3 ppos = ((entity == parent) ? float3.zero : pos.LocalToParent.Position);
                        // find new scale
                        float dist = math.distance(ppos, playerPos);
                        ppos = ppos - playerPos;
                        float sdist = dist - scale.Radius;
                        float desired = 1000f + math.sqrt(sdist / X) * 100f;
                        float A = scale.Radius / dist;
                        float S = -((A * desired) / (A - 1f));
                        rscale = S * 2f;
                        float3 newpos = math.normalize(ppos) * (desired + rscale / 2f);
                        if (dist < 1000f) {
                            newpos = ppos;
                            rscale = 1f;
                        }
                        var ltw = transform.LocalToWorld;
                        ltw.Position = newpos;
                        ltw.Scale = rscale;
                        transform.LocalToWorld = ltw;
                    })
                .WithoutBurst()
                .ScheduleParallel();
        }
    }
}
