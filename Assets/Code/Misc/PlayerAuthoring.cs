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
                AddComponentObject<GameObject>(player);
                AddComponent(new PlayerTag());
            }
        }
    }
}
