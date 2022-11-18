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
            OrbitalParent playerParent = GetComponent<OrbitalParent>(player);
            OrbitalPosition playerPosition = GetComponent<OrbitalPosition>(player);
            PlayerRotation playerRotation = GetComponent<PlayerRotation>(
                GetSingletonEntity<PlayerTag>());
            // OrbitalPosition playerPosition = GetCompoen
            // float3 playerPos = GetComponent<OrbitalPosition>(player).LocalToParent.Position;
            // quaternion playerRot = GetComponent<PlayerRotation>(
            //     GetSingletonEntity<PlayerTag>()).Value;

            // float3 playerWorldPos = GetComponent<OrbitalPosition>(player).LocalToWorld.Position;
            // UniformScaleTransform playerToParentTransform = GetComponent<OrbitalPosition>(player).LocalToParent;
            // UniformScaleTransform playersParentToWorldTransform = GetComponent<OrbitalPosition>(playerParent).LocalToWorld;
            
            Entities
                // don't run on player orbit
                .WithNone<PlayerOrbitTag>()
                .ForEach(
                    (int entityInQueryIndex, Entity entity,
                     ref TransformAspect transform,
                     in OrbitalPosition pos, in OrbitalParameters parms, in OrbitalScale scale,
                     in OrbitalParent parent, in RotationalParameters rot) =>
                    {
                        float rscale;
                        float X = 149597870.700f;
                        // find new position
                        // float3 ppos = ((entity == parent) ? float3.zero : pos.LocalToParent.Position);
                        float3 ppos;
                        float3 playerPos;
                        if (parent.Value == playerParent.Value) {
                            ppos = pos.LocalToParent.Position;
                            playerPos = playerPosition.LocalToParent.Position;
                        } else if (entity == playerParent.Value) {
                            ppos = float3.zero;
                            playerPos = playerPosition.LocalToParent.Position;
                        } else {
                            ppos = pos.LocalToWorld.Position;
                            playerPos = playerPosition.LocalToWorld.Position;
                        }
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
                        // quaternion nrot = math.mul(rot.AxialRotation, rot.AxialTilt);
                        // nrot = math.mul(nrot, playerRotation.Value);
                        // quaternion prot = math.mul(rot.AxialTilt, math.inverse(playerRotation.Value));
                        quaternion prot = math.inverse(playerRotation.Value);
                        quaternion pprot = playerRotation.Value;
                        quaternion orot = math.mul(rot.AxialTilt, rot.AxialRotation);
                        // quaternion prot = playerRotation.Value;
                        // quaternion prot = quaternion.EulerXYZ(0f);
                        // prot = math.mul(rot.AxialRotation, prot);
                        // prot = math.mul(prot, math.inverse(rot.AxialTilt));
                        // prot = math.mul(prot, rot.AxialTilt);
                        // prot = math.mul(math.inverse(rot.AxialTilt), prot);
                        newpos = math.mul(prot, newpos);
                        orot = math.mul(prot, orot);
                        // orot = math.mul(playerRotation.Value, orot);
                        // newpos = math.mul(orot, newpos);
                        var ltw = transform.LocalToWorld;
                        ltw.Position = newpos;
                        ltw.Scale = rscale;
                        ltw.Rotation = orot;
                        // ltw.Rotation = math.mul(ltw.Rotation, math.inverse(playerRot));
                        // ltw.Rotation = prot;
                        // ltw.Position = ltw.TransformPoint(newpos);
                        // ltw.Rotation = math.mul(rot.AxialRotation, prot);
                        transform.LocalToWorld = ltw;
                        // transform.Position = newpos;
                        // transform.Position = transform.TransformRotationLocalToWorld(newpos);
                        // ltw.Rotation = math.mul(math.inverse(rot.AxialRotation), prot);
                        // transform.LocalToWorld = ltw;
                        // transform.LocalRotation = math.mul(rot.AxialTilt, rot.AxialRotation);
                        // transform.LocalRotation = math.mul(math.mul(rot.AxialTilt, rot.AxialRotation),
                        //                                    math.inverse(playerRotation.Value));
                        // transform.LocalRotation = math.mul(transform.LocalRotation, math.inverse(playerRotation.Value));

                        // transform.RotateWorld(math.inverse(playerRot));

                        float wdist = math.length(newpos);
                        float wr = rscale / 2f;
                        float wmag = 2f * math.degrees(math.asin(wr / wdist));
                        float gdist = math.distance(newpos, float3.zero);
                        float rdist = math.distance(pos.LocalToWorld.Position, playerPosition.LocalToWorld.Position);
                        float rr = scale.Radius;
                        float rmag = 2f * math.degrees(math.asin(rr / rdist));
                        // UnityEngine.Debug.Log($"i={entityInQueryIndex} wmag={wmag} rmag={rmag} dist={dist} rdist={rdist} gdist={gdist} rr={rr} <<<R={scale.Radius}>>>");
                    })
                .WithoutBurst()
                .ScheduleParallel();
        }
    }
}
