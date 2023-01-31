using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    public partial class UpdateOrbitalPositionSystem : SystemBase {
        protected override void OnUpdate() {
            OrbitalOptions opts = SystemAPI.GetSingleton<OrbitalOptions>();
            float dt = SystemAPI.Time.DeltaTime * opts.TimeScale;
            var OrbitalParentTypeHandle = GetSharedComponentTypeHandle<OrbitalParent>();
            var RotationalParametersLookup = GetComponentLookup<RotationalParameters>(true);

            new UpdateOrbitalPositionJob {
                DeltaTime = dt,
                OrbitalParentTypeHandle = OrbitalParentTypeHandle,
                RotationalParametersLookup = RotationalParametersLookup
            }.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct UpdateOrbitalPositionJob : IJobEntity, IJobEntityChunkBeginEnd {
            [ReadOnly]
            public float DeltaTime;
            [ReadOnly]
            public SharedComponentTypeHandle<OrbitalParent> OrbitalParentTypeHandle;
            [ReadOnly]
            public ComponentLookup<RotationalParameters> RotationalParametersLookup;

            private quaternion ParentTilt;

            [BurstCompile]
            public bool OnChunkBegin(in ArchetypeChunk chunk, int index, bool useMask, in v128 mask) {
                var parent = chunk.GetSharedComponent<OrbitalParent>(OrbitalParentTypeHandle);
                ParentTilt = RotationalParametersLookup[parent.Value].AxialTilt;
                return true;
            }

            [BurstCompile]
            public void OnChunkEnd(in ArchetypeChunk chunk, int index, bool useMask, in v128 mask, bool wasExecuted) {}

            [BurstCompile]
            public void Execute(Entity entity, ref OrbitalPosition pos,
                                in OrbitalParentPosition ppos, in OrbitalParameters parms)
            {
                // update elapsed time
                pos.ElapsedTime = (pos.ElapsedTime + DeltaTime) % parms.Period;
                // mean motion
                float n = (2f * math.PI) / parms.Period;
                // mean anomaly
                float M = n * pos.ElapsedTime;
                // eccentric anomaly
                float e = parms.Eccentricity;
                float E = EccentricAnomaly(M, e);
                // true anomaly
                // https://en.wikipedia.org/wiki/True_anomaly#From_the_eccentric_anomaly
                float beta = e / (1f + math.sqrt(1 - math.pow(e, 2f)));
                pos.Theta = E + 2f * math.atan((beta * math.sin(E)) / (1f - beta * math.cos(E)));
                // parent distance
                pos.Altitude = parms.SemiMajorAxis * (1f - e * math.cos(E));
                // update position within orbit
                quaternion rot = math.mul(parms.OrbitRotation, quaternion.RotateY(-pos.Theta));
                // place our orbit relative to our parent's axial tilt
                rot = math.mul(ParentTilt, rot);
                        
                // update game position
                var ltp = pos.LocalToParent;
                ltp.Position = math.mul(rot, math.forward() * pos.Altitude);
                pos.LocalToParent = ltp;
                pos.LocalToWorld = ppos.Value.TransformTransform(pos.LocalToParent);
            }
        }
        
        // taken from: https://squarewidget.com/keplers-equation/
        // which itself was taken from: Meeus, Jean. Astronomical Algorithms. 2nd Ed. Willmann-Bell. 1998. (p. 199)
        [BurstCompile]
        private static float EccentricAnomaly(float M, float e) {
            float E0 = M;
            float E1 = 0;
            for(int i = 0; i < 6; i++) {
                E1 = E0 + (math.mad(e, math.sin(E0), M) - E0) /
                          (1f - e * math.cos(E0));
                E0 = E1;
            }
            return E0;
        }
    }
}
