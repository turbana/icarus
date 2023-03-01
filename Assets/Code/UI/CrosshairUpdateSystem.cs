using UnityEngine;
using Unity.Entities;

namespace Icarus.UI {
    public partial class CrosshairUpdateSystem : SystemBase {
        protected override void OnUpdate() {
            var crosshair = SystemAPI.ManagedAPI.GetSingleton<Crosshair>();
            var sr = crosshair.GO.GetComponent<SpriteRenderer>();
            sr.sprite = crosshair.Crosshairs[(int)crosshair.Value];
        }
    }
}
