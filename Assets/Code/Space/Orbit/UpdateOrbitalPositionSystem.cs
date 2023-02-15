using System;

using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Graphics;
using Icarus.Mathematics;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    public partial class UpdateOrbitalPositionSystem : SystemBase {
        protected override void OnUpdate() {
            OrbitalOptions opts = SystemAPI.GetSingleton<OrbitalOptions>();
            float dt = SystemAPI.Time.DeltaTime * opts.TimeScale;
            var OrbitalParentTypeHandle = GetSharedComponentTypeHandle<OrbitalParent>();
            var RotationalParametersLookup = GetComponentLookup<RotationalParameters>(true);
            var player = SystemAPI.GetSingletonEntity<PlayerOrbitTag>();
            var buffer = SystemAPI.GetSingletonEntity<TextUpdate>();
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            new UpdateOrbitalPositionJob {
                ecb = ecb.AsParallelWriter(),
                Player = player,
                DeltaTime = dt,
                OrbitalParentTypeHandle = OrbitalParentTypeHandle,
                RotationalParametersLookup = RotationalParametersLookup,
                TextUpdateEntity = buffer
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
            [ReadOnly]
            public Entity TextUpdateEntity;

            private dquaternion ParentTilt;

            [BurstCompile]
            public bool OnChunkBegin(in ArchetypeChunk chunk, int index, bool useMask, in v128 mask) {
                var parent = chunk.GetSharedComponent<OrbitalParent>(OrbitalParentTypeHandle);
                ParentTilt = RotationalParametersLookup[parent.Value].AxialTilt;
                return true;
            }

            [BurstCompile]
            public void OnChunkEnd(in ArchetypeChunk chunk, int index, bool useMask, in v128 mask, bool wasExecuted) {}

            // [BurstCompile]
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
                    // orbital position
                    Text(index, "Player.Orbit.MeanMotion", n, TextUpdateFormat.Number1_10);
                    Text(index, "Player.Orbit.MeanAnomaly", M, TextUpdateFormat.Number1_10);
                    Text(index, "Player.Orbit.EccentricAnomaly", E, TextUpdateFormat.Number1_10);
                    Text(index, "Player.Orbit.Beta", beta, TextUpdateFormat.Number1_10);
                    Text(index, "Player.Orbit.ElapsedTime", elapsed, TextUpdateFormat.Number9_2);
                    Text(index, "Player.Orbit.Theta", theta, TextUpdateFormat.Number1_10);
                    Text(index, "Player.Orbit.Altitude", altitude, TextUpdateFormat.Number12_0);
                    // orbital parameters
                    Text(index, "Player.Orbit.Period", parms.Period, TextUpdateFormat.Number9_2);
                    Text(index, "Player.Orbit.Eccentricity", parms.Eccentricity, TextUpdateFormat.Number6_5);
                    Text(index, "Player.Orbit.SemiMajorAxis", parms.SemiMajorAxis, TextUpdateFormat.Number9_2);
                    Text(index, "Player.Orbit.Inclination", parms.Inclination, TextUpdateFormat.Number6_5);
                    Text(index, "Player.Orbit.AscendingNode", parms.AscendingNode, TextUpdateFormat.Number6_5);
                    // timings
                    // TODO rising/falling nodes
                    double per = parms.Period - elapsed;
                    double apo = parms.Period/2 - elapsed;
                    if (apo < 0) apo += parms.Period;
                    Text(index, "Player.Orbit.Time.Periapsis", per, TextUpdateFormat.Number9_2);
                    Text(index, "Player.Orbit.Time.Apoapsis", apo, TextUpdateFormat.Number9_2);
                }
            }

            private void Text(int index, FixedString64Bytes key, double value, TextUpdateFormat fmt) {
                ecb.AppendToBuffer<TextUpdate>(index, TextUpdateEntity, new TextUpdate(key, value, fmt));
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
