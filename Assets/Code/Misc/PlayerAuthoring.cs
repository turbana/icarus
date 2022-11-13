using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace Icarus.Misc {
    public struct PlayerTag : IComponentData {
        public Entity Obj;
    }

    [AddComponentMenu("Icarus/Misc/Player Tag")]
    public class PlayerAuthoring : MonoBehaviour {
        public class Baker : Unity.Entities.Baker<PlayerAuthoring> {
            public override void Bake(PlayerAuthoring parms) {
                GameObject player = GameObject.FindWithTag("Player");
                AddComponent(new PlayerTag {
                        Obj = GetEntity(player)
                        // Obj = player
                    });
            }
        }
    }

    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial class PlayerSlaveToGameObjectSystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                // .WithAll<PlayerTag>()
                .ForEach((ref TransformAspect transform, in PlayerTag player) => {
                    LocalToWorld pltw = GetComponent<LocalToWorld>(player.Obj);
                    var ltw = transform.LocalToWorld;
                    ltw.Position = pltw.Position;
                    transform.LocalToWorld = ltw;
                    })
                .Schedule();
        }
    }
}
