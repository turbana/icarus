using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Misc {
    [AddComponentMenu("Icarus/Misc/Register ECS Prefab")]
    public class PrefabAuthoring : MonoBehaviour {
        public class PrefabAuthoringBaker : Baker<PrefabAuthoring> {
            public override void Bake(PrefabAuthoring auth) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                DependsOn(auth.gameObject);
                RegisterPrefabForBaking(auth.gameObject);
                AddComponent<Prefab>(entity);
            }
        }
    }
}
