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
                AddComponent(new SunLightComponent());
            }
        }
    }

    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateGamePositionSystem))]
    public partial class UpdateSunLightDirectionSystem : SystemBase {
        protected GameObject LightObject;
        
        protected override void OnCreate() {
            foreach (Light light in Object.FindObjectsOfType<Light>()) {
                if (light.type == LightType.Directional) {
                    LightObject = light.gameObject;
                    break;
                }
            }
        }
        
        protected override void OnUpdate() {
            var entity = SystemAPI.GetSingletonEntity<SunLightComponent>();
            var ltw = SystemAPI.GetComponent<LocalToWorld>(entity);
            LightObject.transform.position = ltw.Position;
            LightObject.transform.LookAt(Vector3.zero);
        }
    }
}
