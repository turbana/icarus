using UnityEngine;
using Unity.Entities;

namespace Icarus.Space {
    public class StarfieldComponent : IComponentData {
        public float Distance;
        public Object Catalog;
        public Entity Prefab;
    }

    [AddComponentMenu("Icarus/Space/Starfield")]
    public class StarfieldAuthoring : MonoBehaviour {
        public float Distance;
        public Object Catalog;
        public GameObject Prefab;
        
        public class StarfieldAuthoringBaker : Baker<StarfieldAuthoring> {
            public override void Bake(StarfieldAuthoring auth) {
                AddComponentObject(new StarfieldComponent {
                        Distance = auth.Distance,
                        Catalog = auth.Catalog,
                        Prefab = GetEntity(auth.Prefab)
                    });
            }
        }
    }
}
