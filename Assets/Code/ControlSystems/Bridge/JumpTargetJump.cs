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
        public ComponentLookup<DatumDouble> DatumDoubleLookup;
        public ComponentLookup<DatumString64> DatumString64Lookup;
        public ComponentLookup<OrbitalDatabaseComponent> DatabaseLookup;

        [BurstCompile]
        protected override void OnCreate() {
            DatumDoubleLookup = GetComponentLookup<DatumDouble>(true);
            DatumString64Lookup = GetComponentLookup<DatumString64>(true);
            DatabaseLookup = GetComponentLookup<OrbitalDatabaseComponent>(true);
        }

        [BurstCompile]
        protected override void OnUpdate() {
            DatumDoubleLookup.Update(this);
            DatumString64Lookup.Update(this);
            DatabaseLookup.Update(this);
            var DDL = DatumDoubleLookup;
            var DSL = DatumString64Lookup;
            var DBL = DatabaseLookup;
            var databaseEntity = SystemAPI.GetSingletonEntity<OrbitalDatabaseComponent>();
            var player = SystemAPI.GetSingletonEntity<PlayerOrbitTag>();
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            Entities
                .WithReadOnly(DDL)
                .WithReadOnly(DSL)
                .WithReadOnly(DBL)
                .WithAll<BridgeJumpTargetJumpTag>()
                .ForEach((in DatumRefBufferCollection index,
                          in DynamicBuffer<DatumRefBuffer> buffers) => {
                    var buttonEntity = buffers[index["Bridge.JumpTarget.DebugJump"]].Entity;
                    var buttonDatum = DDL[buttonEntity];

                    if (buttonDatum.Dirty && buttonDatum.Value == 1) {
                        var database = DBL[databaseEntity];
                        var parentName = DSL[buffers[index["Planned.Orbit.Target"]].Entity].Value;
                        
                        // don't jump if target is empty
                        if (parentName.IsEmpty) return;
                        
                        // fetch datum values
                        var sma = DDL[buffers[index["Planned.Orbit.SemiMajorAxis"]].Entity].Value;
                        var inc = DDL[buffers[index["Planned.Orbit.Inclination"]].Entity].Value;
                        var ecc = DDL[buffers[index["Planned.Orbit.Eccentricity"]].Entity].Value;
                        var aan = DDL[buffers[index["Planned.Orbit.AscendingNode"]].Entity].Value;
                        var pr = DDL[buffers[index["Bridge.TargetJump.SMA"]].Entity].Value;
                        
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
