using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Misc;

namespace Icarus.Orbit {
    public struct OrbitalOptions : IComponentData {
        public float TimeScale;
    }

    [AddComponentMenu("Icarus/Orbits/Orbital Options")]
    public class OrbitalOptionsAuthoring : MonoBehaviour {
        public float TimeScale;
        
        public class Baker : Unity.Entities.Baker<OrbitalOptionsAuthoring> {
            public override void Bake(OrbitalOptionsAuthoring parms) {
                AddComponent(new OrbitalOptions {
                        TimeScale = parms.TimeScale
                    });
            }
        }
    }

    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateOrbitalPositionSystem))]
    [UpdateAfter(typeof(UpdateRotationSystem))]
    partial class UpdateOrbitsToPlayerRotationSystem : SystemBase {
        protected override void OnUpdate() {
            quaternion playerRot = GetComponent<PlayerRotation>(
                GetSingletonEntity<PlayerTag>()).Value;

            Entities
                .WithAll<OrbitalOptions>()
                .ForEach((ref TransformAspect transform) => {
                    // transform.Rotation = math.inverse(playerRot);
                })
                .Schedule();
        }
    }

    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateBefore(typeof(UpdateOrbitalPositionSystem))]
    [UpdateBefore(typeof(UpdateRotationSystem))]
    partial class ResetOrbitsToPlayerRotationSystem : SystemBase {
        protected override void OnUpdate() {
            quaternion playerRot = GetComponent<PlayerRotation>(
                GetSingletonEntity<PlayerTag>()).Value;

            Entities
                .WithAll<OrbitalOptions>()
                .ForEach((ref TransformAspect transform) => {
                    transform.Rotation = quaternion.EulerXYZ(0f);
                })
                .Schedule();
        }
    }
}
