using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Icarus.UI {
    public class MultiWayControlAuthoring : BaseControlAuthoring {
        [Tooltip("Number of stops this switch supports")]
        public int Stops = 2;
        [Tooltip("Total degrees of travel this switch moves")]
        public float RotateAngle = 80f;
        
        public class MultiWayControlAuthoringBaker : Baker<MultiWayControlAuthoring> {
            public override void Bake(MultiWayControlAuthoring auth) {
                AddComponent<ControlValue>();
                AddComponent<ControlSettings>(new ControlSettings {
                        Stops = auth.Stops,
                        Rotation = (auth.RotateAngle * Mathf.Deg2Rad) / (auth.Stops - 1),
                        Movement = float3.zero,
                    });
            }
        }
    }
}
