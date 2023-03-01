using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;

namespace Icarus.UI {
    [UpdateInGroup(typeof(IcarusInteractionSystemGroup))]
    public partial class CrosshairUpdateSystem : SystemBase {
        protected override void OnUpdate() {
            var crosshair = SystemAPI.ManagedAPI.GetSingleton<Crosshair>();
            var img = crosshair.GO.GetComponent<Image>();
            img.sprite = crosshair.Crosshairs[(int)crosshair.Value];
        }
    }
}
