using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Misc {
    public struct PlayerTag : IComponentData {}

    [AddComponentMenu("Icarus/Misc/Player Tag")]
    public class PlayerAuthoring : MonoBehaviour {
        public class Baker : Unity.Entities.Baker<PlayerAuthoring> {
            public override void Bake(PlayerAuthoring parms) {
                AddComponent(new PlayerTag());
                AddComponent(new Icarus.Orbit.PlayerRotation {
                        Value = quaternion.EulerXYZ(0f)
                    });
            }
        }
    }
}

namespace Icarus.Orbit {
    public struct PlayerRotation : IComponentData {
        public quaternion Value;
    }

    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    public partial class TestChangePlayerRotationSystem : SystemBase {
        protected override void OnUpdate() {
            OrbitalOptions opts = SystemAPI.GetSingleton<OrbitalOptions>();
            float dt = SystemAPI.Time.DeltaTime * opts.TimeScale;
            float3 rps = math.radians(new float3(0f, 1f, 0f) * dt);
            Entities
                .ForEach((ref PlayerRotation rot) => {
                    rot.Value = math.mul(rot.Value, quaternion.EulerYXZ(rps));
                })
                .Schedule();
        }
    }
}
