using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            new UpdateOrbitalPositionJob {
                ecb = ecb.AsParallelWriter(),
                DeltaTime = dt,
                OrbitalParentTypeHandle = OrbitalParentTypeHandle,
                RotationalParametersLookup = RotationalParametersLookup
            }.ScheduleParallel();

            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public partial struct UpdateOrbitalPositionJob : IJobEntity, IJobEntityChunkBeginEnd {
            public EntityCommandBuffer.ParallelWriter ecb;
            [ReadOnly]
            public float DeltaTime;
            [ReadOnly]
            public SharedComponentTypeHandle<OrbitalParent> OrbitalParentTypeHandle;
            [ReadOnly]
            public ComponentLookup<RotationalParameters> RotationalParametersLookup;

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
