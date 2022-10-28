using Unity.Entities;
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
            float3 playerPos =
                GetComponent<OrbitalParameters>(
                    GetSingletonEntity<PlayerOrbitTag>())
                .SolarPosition;
            
            Entities
                // don't run on player orbit
                .WithNone<PlayerOrbitTag>()
                .ForEach(
                    (ref TransformAspect transform, in OrbitalParameters parms,
                     in OrbitalScale scale) =>
                    {
                        float METERS_IN_AU = 149597870700f;
                        // find new position
                        float3 pos = parms.SolarPosition - playerPos;
                        // find new scale
                        float dist = math.distance(parms.SolarPosition, playerPos);
                        float sdist = dist - scale.Radius;
                        float desired = 1000f + math.sqrt(sdist) * 100f;
                        float dscale = desired / sdist;
                        float rscale = (scale.Radius / dist ) * (desired / sdist);
                        float3 newpos = math.normalize(pos) * (desired + rscale / 2f);
                        // UnityEngine.Debug.Log(
                        //     $"pos={pos} dist={dist} r={scale.Radius} sdist={sdist} desired={desired} dscale={dscale} newpos={newpos} rscale={rscale}");
                        var ltw = transform.LocalToWorld;
                        ltw.Scale = rscale;
                        transform.LocalToWorld = ltw;
                        transform.Position = newpos;
                    })
                // .WithoutBurst()
                .ScheduleParallel();
        }
    }
}
