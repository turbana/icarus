using System;

using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Mathematics;
using Icarus.UI;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    public partial class UpdateOrbitalPositionSystem : SystemBase {
        protected override void OnUpdate() {
            OrbitalOptions opts = SystemAPI.GetSingleton<OrbitalOptions>();
            float dt = SystemAPI.Time.DeltaTime * opts.TimeScale;
            var OrbitalParentTypeHandle = GetSharedComponentTypeHandle<OrbitalParent>();
            var RotationalParametersLookup = GetComponentLookup<RotationalParameters>(true);
            var DatumDoubleLookup = GetComponentLookup<DatumDouble>(false);
            var DatumStringLookup = GetComponentLookup<DatumString64>(false);
            var DatumRefLookup = GetBufferLookup<DatumRefBuffer>(true);
            var player = SystemAPI.GetSingletonEntity<PlayerOrbitTag>();
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            new UpdateOrbitalPositionJob {
                ecb = ecb.AsParallelWriter(),
                Player = player,
                DeltaTime = dt,
                OrbitalParentTypeHandle = OrbitalParentTypeHandle,
                RotationalParametersLookup = RotationalParametersLookup,
                DatumDoubleLookup = DatumDoubleLookup,
                DatumStringLookup = DatumStringLookup,
                DatumRefLookup = DatumRefLookup,
            }.ScheduleParallel();

            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public partial struct UpdateOrbitalPositionJob : IJobEntity, IJobEntityChunkBeginEnd {
            public EntityCommandBuffer.ParallelWriter ecb;
            [ReadOnly]
            public Entity Player;
            [ReadOnly]
            public float DeltaTime;
            [ReadOnly]
            public SharedComponentTypeHandle<OrbitalParent> OrbitalParentTypeHandle;
            [ReadOnly]
            public ComponentLookup<RotationalParameters> RotationalParametersLookup;
            [NativeDisableParallelForRestriction]
            public ComponentLookup<DatumDouble> DatumDoubleLookup;
            [NativeDisableParallelForRestriction]
            public ComponentLookup<DatumString64> DatumStringLookup;
            [ReadOnly]
            public BufferLookup<DatumRefBuffer> DatumRefLookup;

            private dquaternion ParentTilt;
            private FixedString64Bytes ParentName;

            [BurstCompile]
            public bool OnChunkBegin(in ArchetypeChunk chunk, int index, bool useMask, in v128 mask) {
                var parent = chunk.GetSharedComponent<OrbitalParent>(OrbitalParentTypeHandle);
                ParentTilt = RotationalParametersLookup[parent.Value].AxialTilt;
                ParentName = parent.Name;
                return true;
            }

            [BurstCompile]
            public void OnChunkEnd(in ArchetypeChunk chunk, int index, bool useMask, in v128 mask, bool wasExecuted) {}

            [BurstCompile]
            public void Execute([ChunkIndexInQuery] int index, Entity entity,
                                in OrbitalPosition pos, in OrbitalParentPosition ppos,
                                in OrbitalParameters parms)
            {
                // update elapsed time
                double elapsed = (pos.ElapsedTime + DeltaTime) % parms.Period;
                // mean motion
                double n = (2.0 * System.Math.PI) / parms.Period;
                // mean anomaly
                double M = n * elapsed;
                // eccentric anomaly
                double e = parms.Eccentricity;
                double E = EccentricAnomaly(M, e);
                // true anomaly
                // https://en.wikipedia.org/wiki/True_anomaly#From_the_eccentric_anomaly
                double beta = e / (1f + math.sqrt(1 - math.pow(e, 2f)));
                double theta = E + 2f * math.atan((beta * math.sin(E)) / (1f - beta * math.cos(E)));
                // parent distance
                double altitude = parms.SemiMajorAxis * (1f - e * math.cos(E));
                // update position within orbit
                dquaternion rot = dmath.mul(parms.OrbitRotation,
                                            dquaternion.RotateY(-theta));
                // place our orbit relative to our parent's axial tilt
                rot = dmath.mul(ParentTilt, rot);
                // update game position
                var ltp = dmath.forward(rot) * altitude;

                // update component
                ecb.SetComponent<OrbitalPosition>(index, entity, new OrbitalPosition {
                        ElapsedTime = elapsed,
                        Theta = theta,
                        Altitude = altitude,
                        LocalToWorld = ppos.Value + ltp,
                        LocalToParent = ltp
                    });

                if (entity == Player) {
                    var buffer = DatumRefLookup[entity];
                    // NOTE: buffer indexes must match order defined in
                    // PlayerOrbitAuthoring.
                    // orbital position
                    SetDatum(buffer[0], n);
                    SetDatum(buffer[1], M);
                    SetDatum(buffer[2], E);
                    SetDatum(buffer[3], beta);
                    SetDatum(buffer[4], elapsed);
                    SetDatum(buffer[5], theta);
                    SetDatum(buffer[6], altitude);
                    // orbit parameters
                    SetDatum(buffer[7], parms.Period);
                    SetDatum(buffer[8], parms.Eccentricity);
                    SetDatum(buffer[9], parms.SemiMajorAxis);
                    SetDatum(buffer[10], parms.Inclination);
                    SetDatum(buffer[11], parms.AscendingNode);
                    // timings
                    // TODO rising/falling nodes
                    double per = parms.Period - elapsed;
                    double apo = parms.Period/2 - elapsed;
                    if (apo < 0) apo += parms.Period;
                    SetDatum(buffer[12], per);
                    SetDatum(buffer[13], apo);
                    // set parent name datum
                    var datum = DatumStringLookup[buffer[14].Entity];
                    datum.Value = ParentName;
                    DatumStringLookup[buffer[14].Entity] = datum;
                }
            }

            private void SetDatum(DatumRefBuffer dref, double value) {
                var datum = DatumDoubleLookup[dref.Entity];
                datum.Value = value;
                DatumDoubleLookup[dref.Entity] = datum;
            }
        }
        
        // taken from: https://squarewidget.com/keplers-equation/
        // which itself was taken from: Meeus, Jean. Astronomical Algorithms. 2nd Ed. Willmann-Bell. 1998. (p. 199)
        [BurstCompile]
        private static double EccentricAnomaly(double M, double e) {
            double E0 = M;
            double E1 = 0;
            for(int i = 0; i < 6; i++) {
                E1 = E0 + (math.mad(e, math.sin(E0), M) - E0) /
                          (1f - e * math.cos(E0));
                E0 = E1;
            }
            return E0;
        }
    }
}
