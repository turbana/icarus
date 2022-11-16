using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Misc;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateOrbitalPositionSystem))]
    public partial class StarfieldRotationSystem : SystemBase {
        private GameObject PlayerObject;

        protected override void OnCreate() {
            PlayerObject = GameObject.FindWithTag("Player");
        }
        
        protected override void OnUpdate() {
            quaternion playerRot = GetComponent<PlayerRotation>(
                GetSingletonEntity<PlayerTag>()).Value;
            
            Entities
                .WithAll<StarfieldTag>()
                .ForEach((ref TransformAspect transform) => {
                    // var ltw = transform.LocalToWorld;
                    // ltw.Position = PlayerObject.transform.position;
                    // ltw.Rotation = math.inverse(playerRot);
                    // transform.LocalToWorld = ltw;
                    transform.Position = PlayerObject.transform.position;
                    transform.Rotation = math.inverse(playerRot);
                })
                .WithoutBurst()
                .Run();
        }
    }
}
