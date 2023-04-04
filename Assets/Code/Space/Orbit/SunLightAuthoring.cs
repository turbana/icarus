using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    public struct SunLightComponent : IComponentData {
    }

    [AddComponentMenu("Icarus/Orbits/Sun Light")]
    public class SunLightAuthoring : MonoBehaviour {
        public class SunLightAuthoringBaker : Baker<SunLightAuthoring> {
            public override void Bake(SunLightAuthoring auth) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SunLightComponent());
            }
        }
    }

    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateGamePositionSystem))]
    public partial class UpdateSunLightDirectionSystem : SystemBase {
        protected GameObject LightObject;
        
        protected override void OnCreate() {
            RequireForUpdate<SunLightComponent>();
            LightObject = GetSun();
        }

        private GameObject GetSun() {
            foreach (Light light in Object.FindObjectsOfType<Light>()) {
                if (light.type == LightType.Directional) {
                    return light.gameObject;
                }
            }
            return null;
        }
        
        protected override void OnUpdate() {
            var entity = SystemAPI.GetSingletonEntity<SunLightComponent>();
            var ltw = SystemAPI.GetComponent<LocalToWorld>(entity);
            var isNaN = ltw.Position.x is float.NaN || ltw.Position.y is float.NaN || ltw.Position.z is float.NaN;
            if (!isNaN) {
                if (LightObject is null) {
                    LightObject = GetSun();
                }
                LightObject.transform.position = ltw.Position;
                LightObject.transform.LookAt(Vector3.zero);
            }
        }
    }
}
