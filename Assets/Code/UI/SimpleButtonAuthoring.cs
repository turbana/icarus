using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Icarus.UI {
    public class SimpleButtonAuthoring : BaseControlAuthoring {
        public bool IsToggle;
        public Vector3 Movement;
        
        public class SimpleButtonAuthoringBaker : Baker<SimpleButtonAuthoring> {
            public override void Bake(SimpleButtonAuthoring auth) {
                AddComponent<ControlValue>(new ControlValue {
                        Value = 0,
                        PreviousValue = 1,
                    });
                AddComponent<ControlSettings>(new ControlSettings {
                        Stops = 2,
                        Rotation = 0f,
                        Movement = (float3)auth.Movement,
                    });
            }
        }
    }
}
