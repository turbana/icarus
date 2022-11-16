using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Misc;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateOrbitalPositionSystem))]
    public partial class UpdateGamePositionSystem : SystemBase {
        protected override void OnUpdate() {
            Entity player = GetSingletonEntity<PlayerOrbitTag>();
            Entity parent = GetComponent<OrbitalParent>(player).Value;
            float3 playerPos = GetComponent<OrbitalPosition>(player).LocalToParent.Position;
            quaternion playerRot = GetComponent<PlayerRotation>(
                GetSingletonEntity<PlayerTag>()).Value;

            float3 playerWorldPos = GetComponent<OrbitalPosition>(player).LocalToWorld.Position;
            UniformScaleTransform playerToParentTransform = GetComponent<OrbitalPosition>(player).LocalToParent;
            UniformScaleTransform playersParentToWorldTransform = GetComponent<OrbitalPosition>(parent).LocalToWorld;
            
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
                        float sdist = dist - scale.Radius;
                        float desired = 1000f + math.sqrt(sdist / X) * 100f;
                        float A = scale.Radius / dist;
                        float S = -((A * desired) / (A - 1f));
                        rscale = S * 2f;
                        // ppos = math.normalize(math.mul(math.inverse(playerRot), ppos - playerPos));
                        ppos = math.normalize(ppos - playerPos);
                        float3 newpos = ppos * (desired + rscale / 2f);
                        // float3 up = math.cross(ppos, new float3(0f, 1f, 0f));
                        // quaternion rotToPlayer = math.inverse(playerRot);
                        // quaternion rotToPlayer = quaternion.LookRotation(up, ppos);
                        // quaternion rotToPlayer = math.inverse(quaternion.LookRotation(ppos, up));
                        if (dist < 1000f) {
                            newpos = ppos;
                            rscale = 1f;
                        }
                        var ltw = transform.LocalToWorld;
                        // ltw.Position = newpos;
                        ltw.Scale = rscale;
                        // ltw.Rotation = math.mul(ltw.Rotation, math.inverse(playerRot));
                        ltw.Rotation = math.inverse(playerRot);
                        transform.LocalToWorld = ltw;
                        // transform.Position = newpos;
                        transform.Position = transform.TransformRotationLocalToWorld(newpos);

                        // transform.RotateWorld(math.inverse(playerRot));

                        float wdist = math.length(newpos);
                        float wr = rscale / 2f;
                        float wmag = 2f * math.degrees(math.asin(wr / wdist));
                        float rdist = math.distance(pos.LocalToWorld.Position, playerWorldPos);
                        float rr = scale.Radius;
                        float rmag = 2f * math.degrees(math.asin(rr / rdist));
                        // UnityEngine.Debug.Log($"i={entityInQueryIndex} wmag={wmag} rmag={rmag} dist={dist} rdist={rdist} rr={rr} <<<R={scale.Radius}>>>");
                    })
                .WithoutBurst()
                .ScheduleParallel();
        }
    }
}
