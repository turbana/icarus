using UnityEngine;
using Unity.Entities;
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
            OrbitalPosition ppos = GetComponent<OrbitalPosition>(
                GetSingletonEntity<PlayerOrbitTag>());
            LocalToWorld pltw = GetComponent<LocalToWorld>(
                GetSingletonEntity<PlayerTag>());
            
            Entities
                .WithAll<StarfieldTag>()
                .ForEach((ref TransformAspect transform) => {
                    var ltw = transform.LocalToWorld;
                    ltw.Position = PlayerObject.transform.position;
                    transform.LocalToWorld = ltw;
                })
                .WithoutBurst()
                .Run();
        }
    }
}
