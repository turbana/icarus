using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

namespace Icarus.Orbit {
    struct OrbitalParameters : IComponentData {
        public float DeltaTheta;
        public float Theta;
    }

    [AddComponentMenu("Icarus/Orbits/Orbital Parameters")]
    public class OrbitalParametersAuthoring : MonoBehaviour {
        public float DeltaTheta;
        public float InitialTheta = 0f;
        
        public class Baker : Unity.Entities.Baker<OrbitalParametersAuthoring> {
            public override void Bake(OrbitalParametersAuthoring parms) {
                // Debug.Log("baking");
                AddComponent(new OrbitalParameters {
                        DeltaTheta = math.radians(parms.DeltaTheta),
                        Theta = math.radians(parms.InitialTheta)
                    });
            }
        }
    }

    // [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    // public partial class TestBakingSystem : SystemBase {
    //     protected override void OnUpdate() {
    //         Debug.Log("baking system");
    //     }
    // }
}
