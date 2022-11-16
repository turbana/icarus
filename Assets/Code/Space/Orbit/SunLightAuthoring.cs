using UnityEngine;
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
                foreach (Light light in Object.FindObjectsOfType<Light>()) {
                    if (light.type == LightType.Directional) {
                        Debug.Log("added sun light");
                        AddComponentObject<Light>(light);
                        break;
                    }
                }
            }
        }
    }

    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateGamePositionSystem))]
    public partial class UpdateSunLightDirectionSystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                .ForEach((Entity entity, in SunLightComponent sun, in TransformAspect transform) => {
                    Light light = this.EntityManager.GetComponentObject<Light>(entity);
                    // light.transform.position = transform.Position;
                    var rand = new Unity.Mathematics.Random((uint)System.Diagnostics.Stopwatch.GetTimestamp());
                    Vector3 pos = new Vector3(rand.NextFloat(-1000f, 1000f),
                                              rand.NextFloat(-1000f, 1000f),
                                              rand.NextFloat(-1000f, 1000f));
                    light.transform.LookAt(pos);
                    return;
                })
                .WithoutBurst()
                .Run();
        }
    }
}
