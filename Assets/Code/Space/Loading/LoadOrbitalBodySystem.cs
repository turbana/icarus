using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Orbit;

namespace Icarus.Loading {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(LoadingSystemGroup))]
    public partial class LoadOrbitalBodySystem : SystemBase {
        protected override void OnUpdate() {
            var database = GetSingleton<OrbitalDatabaseComponent>();
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            Entities
                .ForEach((Entity entity, in OrbitalBodyToLoadComponent body) => {
                    var data = database.DataMap[body.Name];
                    var player = data.Type == "Player";
                    
                    // set orbital tag
                    if (data.Type == "Planet") ecb.AddComponent<PlanetTag>(entity);
                    else if (data.Type == "Moon") ecb.AddComponent<MoonTag>(entity);
                    else if (data.Type == "DwarfPlanet") ecb.AddComponent<DwarfPlanetTag>(entity);
                    else if (data.Type == "Asteroid") ecb.AddComponent<AsteroidTag>(entity);
                    else if (data.Type == "Ship") ecb.AddComponent<ShipTag>(entity);
                    else if (data.Type == "Player") ecb.AddComponent<ShipTag>(entity);
                    else throw new System.Exception("invalid orbital body type: " + data.Type);
                    
                    // add orbital parameters
                    ecb.AddComponent<OrbitalParameters>(entity, new OrbitalParameters {
                            Period = data.Period,
                            Eccentricity = data.Eccentricity,
                            SemiMajorAxis = data.SemiMajorAxis,
                            Inclination = data.Inclination,
                            AscendingNode = data.AscendingNode,
                            OrbitRotation = quaternion.EulerYXZ(math.radians(data.Inclination),
                                                                math.radians(data.AscendingNode),
                                                                0f)
                        });
                    
                    // add orbital position
                    ecb.AddComponent<OrbitalPosition>(entity, new OrbitalPosition {
                            ElapsedTime = data.ElapsedTime,
                            LocalToParent = UniformScaleTransform.FromPosition(float3.zero),
                            LocalToWorld = UniformScaleTransform.FromPosition(float3.zero)
                        });
                    
                    // skip rotational and scale components on the player
                    if (!player) {
                        // add orbital rotation
                        ecb.AddComponent<RotationalParameters>(entity, new RotationalParameters {
                                Tilt = data.AxialTilt,
                                NorthPoleRA = data.NorthPoleRA,
                                Period = data.RotationPeriod,
                                ElapsedTime = data.RotationElapsedTime,
                                AxialRotation = quaternion.EulerXYZ(0f),
                                AxialTilt = math.mul(quaternion.RotateX(-math.radians(data.AxialTilt)),
                                                     quaternion.RotateY(-math.radians(data.NorthPoleRA)))
                            });
                        
                        // add orbital scale
                        ecb.AddComponent<OrbitalScale>(entity, new OrbitalScale {
                                Radius = data.Radius
                            });
                    }

                    // add special sunlight component on the sun
                    if (data.Name == "Sun") {
                        ecb.AddComponent<SunLightComponent>(entity);
                    }

                    // add to database
                    database.EntityMap[data.Name] = entity;
                })
                .Schedule();

            Entities
                .ForEach((Entity entity, ref OrbitalBodyToLoadComponent body) => {
                    var data = database.DataMap[body.Name];
                    
                    // add orbital parent
                    ecb.AddComponent<OrbitalParent>(entity, new OrbitalParent {
                            Value = database.EntityMap[data.Parent],
                            ParentToWorld = UniformScaleTransform.FromScale(1f)
                        });
                    
                    // remove fixup components
                    ecb.RemoveComponent<OrbitalBodyToLoadComponent>(entity);
                })
                .Schedule();

            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
        }
    }
}
