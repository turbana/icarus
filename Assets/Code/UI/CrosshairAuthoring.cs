using UnityEngine;
using Unity.Entities;

namespace Icarus.UI {
    public class CrosshairAuthoring : MonoBehaviour {
        public Sprite[] Crosshairs;
        
        public class CrosshairAuthoringBaker : Baker<CrosshairAuthoring> {
            public override void Bake(CrosshairAuthoring auth) {
                var go = GameObject
                    .Find("Screen Canvas").transform
                    .Find("Crosshair")
                    .gameObject;
                DependsOn(go);
                AddComponent<Crosshair>(new Crosshair {
                        Value = CrosshairType.Normal,
                    });
                AddComponentObject<CrosshairConfig>(new CrosshairConfig {
                        GO = go,
                        Crosshairs = auth.Crosshairs,
                    });
            }
        }
    }
}
