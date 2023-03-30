using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Orbit;

namespace Icarus.Misc {
    public struct PlayerTag : IComponentData {}

    [AddComponentMenu("Icarus/Misc/Player Tag")]
    public class PlayerAuthoring : MonoBehaviour {
        public class PlayerAuthoringBaker : Baker<PlayerAuthoring> {
            public override void Bake(PlayerAuthoring parms) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerTag());
                AddComponent(entity, new PlayerRotation {
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
}

namespace Icarus.Test {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    public partial class TestChangePlayerRotationSystem : SystemBase {
        protected override void OnUpdate() {
            OrbitalOptions opts = SystemAPI.GetSingleton<OrbitalOptions>();
            float dt = SystemAPI.Time.DeltaTime * opts.TimeScale;
            float3 rps = math.radians(new float3(0f, 0f, 0f) * dt);
            Entities
                .ForEach((ref PlayerRotation rot) => {
                    rot.Value = math.mul(rot.Value, quaternion.EulerYXZ(rps));
                })
                .Schedule();
        }
    }
}
