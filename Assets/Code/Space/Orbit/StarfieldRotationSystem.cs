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
            quaternion playerRot = SystemAPI.GetComponent<PlayerRotation>(
                SystemAPI.GetSingletonEntity<PlayerTag>()).Value;
            
            Entities
                .WithAll<StarfieldTag>()
                .ForEach((ref LocalTransform transform) => {
                    transform.Position = PlayerObject.transform.position;
                    transform.Rotation = math.inverse(playerRot);
                })
                .WithoutBurst()
                .Run();
        }
    }
}
