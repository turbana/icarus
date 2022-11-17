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
            Entities
                .ForEach((Entity entity, in SunLightComponent sun, in TransformAspect transform) => {
                    // LightObject.transform.position = new Vector3(0f, 10f, 0f);
                    LightObject.transform.position = transform.Position;
                    LightObject.transform.LookAt(Vector3.zero);
                })
                .WithoutBurst()
                .Run();
        }
    }
}
