using UnityEngine;
using UnityEngine.VFX;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
// using Unity.Jobs;
using Unity.Mathematics;

using Icarus.Misc;
using Icarus.Orbit;

namespace Icarus.UI {
    public struct OrbitalMap : IComponentData {
        public double Scale;
        public double3 CenterPoint;
    }

    public class OrbitalMapVFX : IComponentData {
        public VisualEffectAsset Asset;
    }

    public struct OrbitalMapData {
        public float Radius;
        public float Eccentricity;
        public double3 LocalToWorld;
        public FixedString64Bytes Name;
    }

#if UNITY_EDITOR
    public class OrbitalMapAuthoring : MonoBehaviour {
        public VisualEffectAsset OrbitEffect;
        
        public class OrbitalMapAuthoringBaker : Baker<OrbitalMapAuthoring> {
            public override void Bake(OrbitalMapAuthoring auth) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new OrbitalMap {
                        Scale = 1.495979e+8,
                        CenterPoint = double3.zero,
                    });
                AddComponentObject(entity, new OrbitalMapVFX {
                        Asset = auth.OrbitEffect,
                    });
            }
        }
    }
#endif
    
    [BurstCompile]
    [UpdateInGroup(typeof(IcarusSimulationSystemGroup))]
    public partial class UpdateOrbitalMapSystem : SystemBase {
        public GameObject MapObject;

        private static readonly float MAX_RADIUS = 5f;
        private static readonly float ORBIT_THICKNESS = 4f;
        private static readonly Vector4 ORBIT_COLOR = new Vector4(0f, 1f, 0f, 1f);
        
        [BurstCompile]
        protected override void OnCreate() {
            RequireForUpdate<OrbitalMap>();
            // XXX
            this.Enabled = false;
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            var map = SystemAPI.GetSingleton<OrbitalMap>();
            var vfxconfig = SystemAPI.ManagedAPI.GetSingleton<OrbitalMapVFX>();
            var orbits = new NativeList<OrbitalMapData>(10, Allocator.TempJob);

            // find orbit entities
            Entities
                .WithAll<PlanetTag>()
                .ForEach((Entity entity, in OrbitalParameters parms, in OrbitalPosition pos) => {
                    var radius = (float)(parms.SemiMajorAxis / map.Scale);
                    if (radius >= MAX_RADIUS) return;
                    var e = (float)parms.Eccentricity;

                    // find parent object
                    if (MapObject is null) {
                        MapObject = GameObject.Find("/Orbit Map");
                    }
                    // find orbit object
                    var name = $"OrbitalMap - {parms.BodyName}";
                    var obj = GameObject.Find("/" + name);
                    if (obj is null) {
                        Debug.Log($"creating [{name}] with [{vfxconfig.Asset}]");
                        obj = new GameObject(name, typeof(VisualEffect));
                        obj.transform.position = MapObject.transform.position;
                        obj.transform.rotation = MapObject.transform.rotation;
                    }
                    // update vfx effect
                    var vfx = obj.GetComponent<VisualEffect>();
                    vfx.visualEffectAsset = vfxconfig.Asset;
                    vfx.SetVector4("Color", ORBIT_COLOR);
                    vfx.SetFloat("Thickness", ORBIT_THICKNESS);
                    vfx.SetFloat("Radius", radius);
                    vfx.Play();
                    // update position
                    // obj.transform.position = MapObject.transform.position
                    //     + (Vector3)(float3)(pos.LocalToWorld / map.Scale);
                })
                // .Schedule();
                .WithoutBurst()
                .Run();

            // update vfx graph objects
            // Job
            //     .WithCode(() => {})
            //     .WithoutBurst()
            //     .Run();
            
            orbits.Dispose();
        }
   }
}
