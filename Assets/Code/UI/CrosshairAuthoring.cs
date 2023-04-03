using System;
using UnityEngine;
using Unity.Entities;

namespace Icarus.UI {
    public class CrosshairAuthoring : MonoBehaviour {
        public Sprite[] Crosshairs;
        
#if UNITY_EDITOR
        public class CrosshairAuthoringBaker : Baker<CrosshairAuthoring> {
            public override void Bake(CrosshairAuthoring auth) {
                try {
                    var entity = GetEntity(TransformUsageFlags.Dynamic);
                    var go = GameObject
                        .Find("Screen Canvas").transform
                        .Find("Crosshair")
                        .gameObject;
                    DependsOn(go);
                    AddComponent<Crosshair>(entity, new Crosshair {
                            Value = CrosshairType.Normal,
                        });
                    AddComponentObject<CrosshairConfig>(entity, new CrosshairConfig {
                            GO = go,
                            Crosshairs = auth.Crosshairs,
                        });
                } catch (NullReferenceException) {}
            }
        }
#endif
    }
}
