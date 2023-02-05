using UnityEngine;
using UnityEditor;

using Unity.Collections;
using Unity.Entities;

namespace Icarus.Space {
    public struct StarfieldComponent : IComponentData {
        public float Distance;
        public FixedString512Bytes Catalog;
        public Entity Prefab;
    }

    [AddComponentMenu("Icarus/Space/Starfield")]
    public class StarfieldAuthoring : MonoBehaviour {
        public float Distance;
        public Object Catalog;
        public GameObject Prefab;
        
        public class StarfieldAuthoringBaker : Baker<StarfieldAuthoring> {
            public override void Bake(StarfieldAuthoring auth) {
                AddComponent(new StarfieldComponent {
                        Distance = auth.Distance,
                        Catalog = AssetDatabase.GetAssetPath(auth.Catalog),
                        Prefab = GetEntity(auth.Prefab)
                    });
            }
        }
    }
}
