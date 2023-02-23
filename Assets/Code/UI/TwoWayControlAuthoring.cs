using UnityEngine;
using Unity.Entities;

namespace Icarus.UI {
    public class TwoWayControlAuthoring : MonoBehaviour {
        public GameObject MovingMesh;
        
        public class TwoWayControlAuthoringBaker : Baker<TwoWayControlAuthoring> {
            public override void Bake(TwoWayControlAuthoring auth) {
                AddComponent<Interaction>();
                AddComponent<TwoWayControl>();
                AddComponent<ControlValue>();
            }
        }
    }
}
