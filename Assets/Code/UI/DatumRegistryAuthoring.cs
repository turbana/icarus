using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Icarus.UI {
    public class DatumRegistryAuthoring : MonoBehaviour {
        private static int INITIAL_REGISTRY_SIZE = 1000;
        
        public class DatumRegistryAuthoringBaker : Baker<DatumRegistryAuthoring> {
            public override void Bake(DatumRegistryAuthoring auth) {
                AddComponent<DatumRegistry>(new DatumRegistry {
                        Map = new UnsafeHashMap<FixedString64Bytes, Entity>(INITIAL_REGISTRY_SIZE, Allocator.Persistent),
                        BackMap = new UnsafeHashMap<Entity, DynamicBuffer<DatumBackRef>>(INITIAL_REGISTRY_SIZE, Allocator.Persistent),
                    });
            }
        }
    }
}
