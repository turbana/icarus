using UnityEngine;
using Unity.Entities;

namespace Icarus.UI {
    public class CrosshairAuthoring : MonoBehaviour {
        public Sprite[] Crosshairs;
        
        public class CrosshairAuthoringBaker : Baker<CrosshairAuthoring> {
            public override void Bake(CrosshairAuthoring auth) {
                var go = GameObject
                    .Find("MainCamera").transform
                    .Find("Canvas")
                    .Find("Crosshair")
                    .gameObject;
                DependsOn(go);
                AddComponentObject<Crosshair>(new Crosshair {
                        Value = CrosshairType.Normal,
                        GO = go,
                        Crosshairs = auth.Crosshairs,
                    });
            }
        }
    }
}
