using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using Icarus.UI;
using Icarus.Mathematics;
using Icarus.Orbit;

namespace Icarus.Controls {
    public partial struct BridgeJumpTargetJumpTag : IComponentData {}

    [BurstCompile]
    [UpdateInGroup(typeof(IcarusInteractionSystemGroup))]
    public partial class BridgeJumpTargetJump : SystemBase {
        public ComponentLookup<OrbitalDatabaseComponent> DatabaseLookup;

        [BurstCompile]
        protected override void OnCreate() {
            DatabaseLookup = GetComponentLookup<OrbitalDatabaseComponent>(true);
        }

        [BurstCompile]
        protected override void OnUpdate() {
            DatabaseLookup.Update(this);
            var DBL = DatabaseLookup;
            var databaseEntity = SystemAPI.GetSingletonEntity<OrbitalDatabaseComponent>();
            var player = SystemAPI.GetSingletonEntity<PlayerOrbitTag>();
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            Entities
                .WithReadOnly(DBL)
                .WithAll<BridgeJumpTargetJumpTag>()
                .ForEach((in DatumCollection datums) => {
                    if (datums.IsPressed("Bridge.JumpTarget.DebugJump")) {
                        var database = DBL[databaseEntity];
                        var parentName = datums.GetString64("Planned.Orbit.Target");
                        
                        // don't jump if target is empty
                        if (parentName.IsEmpty) return;
                        
                        // fetch datum values
                        var sma = datums.GetDouble("Planned.Orbit.SemiMajorAxis");
                        var inc = datums.GetDouble("Planned.Orbit.Inclination");
                        var ecc = datums.GetDouble("Planned.Orbit.Eccentricity");
                        var aan = datums.GetDouble("Planned.Orbit.AscendingNode");
                        var pr = datums.GetDouble("Planned.Orbit.SMA");
                        
                        // find parent
                        var parentEntity = database.LookupEntity(parentName);
                        var parentData = database.LookupData(parentName);
                        
                        // fix-up sma when parent-relative is set
                        if (pr == 1) {
                            sma *= parentData.Radius;
                        }

                        // don't jump if we'd be inside the body
                        if (sma <= parentData.Radius) return;
                        
                        // find period
                        // TODO assume 0 mass for player ship
                        var period = dmath.Period(sma, parentData.Mass, 0.0);
                        
                        // perform jump
                        UnityEngine.Debug.Log($"jumping to {parentName} @{sma}km {period}s i{inc} e{ecc} an{aan} ({pr})");

                        // update parameters
                        ecb.SetComponent(player, new OrbitalParameters {
                            Period = period,
                            Eccentricity = ecc,
                            SemiMajorAxis = sma,
                            Inclination = inc,
                            AscendingNode = aan,
                            OrbitRotation = dquaternion.EulerYXZ(math.radians(inc), math.radians(aan), 0),
                        });

                        ecb.SetSharedComponent(player, new OrbitalParent {
                            Value = parentEntity,
                            Name = parentName,
                        });
                    }
                })
                .Schedule();

            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
        }
    }
}
