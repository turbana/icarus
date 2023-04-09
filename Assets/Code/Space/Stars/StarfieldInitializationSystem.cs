using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Icarus.Loading;

namespace Icarus.Space {
    public struct StarData {
        public string name;
        public float ra;
        public float dec;
        public float mag;
        public float temp;

        public override string ToString() => $"{name} ({ra}ra {dec}dec) {mag}mag {temp}K";
    }

    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(IcarusLoadingSystemGroup))]
    public partial class StarfieldInitializationSystem : SystemBase {
        protected override void OnUpdate() {
            var entity = SystemAPI.GetSingletonEntity<StarfieldComponent>();
            var comp = SystemAPI.GetComponent<StarfieldComponent>(entity);
            var buffer = SystemAPI.GetBuffer<StarSetup>(entity);

            var entities = new NativeArray<Entity>(buffer.Length, Allocator.TempJob);
            EntityManager.Instantiate(comp.Prefab, entities);
            
            for (int i=0; i<entities.Length; i++) {
                EntityManager.SetComponentData(entities[i], buffer[i].Position);
                var sprite = SystemAPI.ManagedAPI.GetComponent<SpriteRenderer>(entities[i]);
                var color = buffer[i].Color;
                sprite.color = new Color(color.x, color.y, color.z, color.w);
            }

#if !UNITY_EDITOR
            UnityEngine.Debug.Log($"loaded {entities.Length} stars");
#endif

            entities.Dispose();
            this.Enabled = false;
            return;
        }
    }
}
