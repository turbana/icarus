using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.VFX;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Misc;
using Icarus.Orbit;

namespace Icarus.UI {
    public struct OrbitalMap : IComponentData {
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

        private static readonly float MAX_RADIUS = 3f;
        private static readonly float MIN_RADIUS = 0.1f;
        private static readonly float ORBIT_THICKNESS = 4f;
        private static readonly float BASE_SCALE = 1.495979e+8f;
        private static readonly int   CONTROL_MIDPOINT = 15;
        
        private static readonly Vector4 ORBIT_COLOR = new Vector4(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Vector4 ORBIT_PLAYER = new Vector4(0f, 1f, 0f, 1f);
        private static readonly Vector4 ORBIT_PARENT = new Vector4(0f, 1f, 1f, 1f);
        private static readonly Vector4 ORBIT_SUN = new Vector4(1f, 1f, 0f, 1f);

        private Dictionary<string, GameObject> OrbitObjects = new Dictionary<string, GameObject>();
        private HashSet<string> ActiveObjects = new HashSet<string>();
        
        [BurstCompile]
        protected override void OnCreate() {
            RequireForUpdate<OrbitalMap>();
            RequireForUpdate<DatumCollection>();
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            var mentity = SystemAPI.GetSingletonEntity<OrbitalMap>();
            var map = SystemAPI.GetComponent<OrbitalMap>(mentity);
            var transform = SystemAPI.GetComponent<LocalTransform>(mentity);
            var vfxconfig = SystemAPI.ManagedAPI.GetSingleton<OrbitalMapVFX>();
            var orbits = new NativeList<OrbitalMapData>(10, Allocator.TempJob);
            var player = SystemAPI.GetSingletonEntity<PlayerOrbitTag>();
            var parent = SystemAPI.GetSingletonEntity<PlayerParentOrbitTag>();
            var sun = SystemAPI.GetSingletonEntity<SunTag>();

            // find map scale
            var datums = SystemAPI.GetSingleton<DatumCollection>();
            var control = (float)datums.GetDouble("OrbitalMap.Scale");
            var delta = CONTROL_MIDPOINT - control;
            float scale;
            if (delta < 0) scale = BASE_SCALE / -delta;
            else           scale = BASE_SCALE * (delta + 2); // +2 because we
                                                             // are at 1x with delta=-1

            // find orbit entities
            Entities
                .WithAny<PlanetTag, DwarfPlanetTag>()
                .ForEach((Entity entity, in OrbitalParameters parms, in OrbitalPosition pos) => {
                    GameObject obj;
                    var name = parms.BodyName.ToString();
                    var e = (float)parms.Eccentricity;
                    var radius = (float)(parms.SemiMajorAxis / scale);

                    // check for radius too large / too small
                    if (entity != sun && (radius < MIN_RADIUS || MAX_RADIUS < radius)) {
                        if (ActiveObjects.Contains(name)) {
                            obj = OrbitObjects[name];
                            obj.SetActive(false);
                            ActiveObjects.Remove(name);
                        }
                        return;
                    };
                    
                    // find orbit object
                    if (OrbitObjects.ContainsKey(name)) {
                        obj = OrbitObjects[name];
                        obj.SetActive(true);
                    } else {
                        obj = new GameObject($"OrbitalMap - {name}", typeof(VisualEffect));
                        obj.transform.position = transform.Position;
                        obj.transform.rotation = transform.Rotation;
                        OrbitObjects[name] = obj;
                    }
                    ActiveObjects.Add(name);
                    
                    // find color
                    var color = ORBIT_COLOR;
                    if (entity == player) color = ORBIT_PLAYER;
                    else if (entity == parent) color = ORBIT_PARENT;
                    else if (entity == sun) color = ORBIT_SUN;
                    
                    // update vfx effect
                    var vfx = obj.GetComponent<VisualEffect>();
                    vfx.visualEffectAsset = vfxconfig.Asset;
                    vfx.SetVector4("Color", color);
                    vfx.SetFloat("Thickness", ORBIT_THICKNESS);
                    vfx.SetFloat("Radius", radius);
                    vfx.Play();
                    
                    // update rotation
                    obj.transform.rotation = (quaternion)parms.OrbitRotation
                        * Quaternion.AngleAxis((float)-pos.Theta * Mathf.Rad2Deg, Vector3.up)
                        * transform.Rotation;
                })
                .WithoutBurst()
                .Run();

            orbits.Dispose();
        }
   }
}
