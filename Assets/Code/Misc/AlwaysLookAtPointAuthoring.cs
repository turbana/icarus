using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Misc {
    public struct AlwaysLookAtPoint : IComponentData {
        public float3 Point;
    }

    [AddComponentMenu("Icarus/Misc/Always Look At Point")]
    public class AlwaysLookAtPointAuthoring : MonoBehaviour {
        public Vector3 Point;

        public class AlwaysLookAtPointBaking : Baker<AlwaysLookAtPointAuthoring> {
            public override void Bake(AlwaysLookAtPointAuthoring parms) {
                AddComponent(new AlwaysLookAtPoint { Point = parms.Point });
            }
        }
    }

    [RequireMatchingQueriesForUpdate]
    // XXX what group should this be in?
    [UpdateInGroup(typeof(Icarus.Orbit.UpdateOrbitSystemGroup))]
    public partial class AlwaysLookAtPointSystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                .ForEach((ref TransformAspect transform, in AlwaysLookAtPoint alap) => {
                    transform.LookAt(alap.Point);
                })
                .ScheduleParallel();
        }
    }
}
