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
                        
                        float dist = math.distance(ppos, playerPos);
                        float sdist = dist - scale.Radius;
                        float desired = 1000f + math.sqrt(sdist / X) * 100f;
                        float A = scale.Radius / dist;
                        float S = -((A * desired) / (A - 1f));
                        rscale = S * 2f;
                        ppos = ppos - playerPos;
                        float3 newpos = math.normalize(ppos) * (desired + rscale / 2f);
                        
                        if (dist < 1000f) {
                            newpos = ppos;
                            rscale = 1f;
                        }
                        
                        quaternion prot = math.inverse(playerRotation.Value);
                        quaternion pprot = playerRotation.Value;
                        quaternion orot = math.mul(rot.AxialTilt, rot.AxialRotation);
                        newpos = math.mul(prot, newpos);
                        orot = math.mul(prot, orot);

                        transform.LocalToWorld = UniformScaleTransform.FromPositionRotationScale(newpos, orot, rscale);
                        
                        float wdist = math.length(newpos);
                        float wr = rscale / 2f;
                        float wmag = 2f * math.degrees(math.asin(wr / wdist));
                        float gdist = math.distance(newpos, float3.zero);
                        float rdist = math.distance(pos.LocalToWorld.Position, playerPosition.LocalToWorld.Position);
                        float rr = scale.Radius;
                        float rmag = 2f * math.degrees(math.asin(rr / rdist));
                        // if (scale.Radius == 6371f) {
                        //     UnityEngine.Debug.Log($"i={entityInQueryIndex} wmag={wmag} rmag={rmag} dist={dist} rdist={rdist} gdist={gdist} rr={rr} <<<R={scale.Radius}>>> pos={newpos}");
                        // }
                    })
                .WithoutBurst()
                .ScheduleParallel();
        }
    }
}
