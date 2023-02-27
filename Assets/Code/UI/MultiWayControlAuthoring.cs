using UnityEngine;
using Unity.Entities;

namespace Icarus.UI {
    public class MultiWayControlAuthoring : MonoBehaviour {
        [Tooltip("Number of stops this switch supports")]
        public int Stops = 2;
        [Tooltip("Total degrees of travel this switch moves")]
        public float RotateAngle = 80f;
        
        public class MultiWayControlAuthoringBaker : Baker<MultiWayControlAuthoring> {
            public override void Bake(MultiWayControlAuthoring auth) {
                AddComponent<ControlValue>();
                AddComponent<ControlSettings>(new ControlSettings {
                        Stops = auth.Stops,
                        RotateAngle = (auth.RotateAngle * Mathf.Deg2Rad) / (auth.Stops - 1),
                    });
            }
        }
    }
}
