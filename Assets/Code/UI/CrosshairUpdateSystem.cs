using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;

namespace Icarus.UI {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(IcarusInteractionSystemGroup))]
    public partial class CrosshairUpdateSystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                .WithChangeFilter<Crosshair>()
                .ForEach((CrosshairConfig config, in Crosshair crosshair) => {
                    if (config.GO is null) {
                        // needs to be in-sync with CrosshairAuthoring.cs
                        config.GO = GameObject
                            .Find("Screen Canvas").transform
                            .Find("Crosshair")
                            .gameObject;
                    }
                    var img = config.GO.GetComponent<Image>();
                    img.sprite = config.Crosshairs[(int)crosshair.Value];
                    })
                .WithoutBurst()
                .Run();
        }
    }
}
