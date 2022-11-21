using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Icarus.Space {
    public struct StarData {
        public string name;
        public float ra;
        public float dec;
        public float mag;
        public float temp;

        public override string ToString() => $"{name} ({ra}ra {dec}dec) {mag}mag {temp}K";
    }

    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class StarfieldInitializationSystem : SystemBase {
        private const float DEGREES_PER_HOUR = 360f / 24f;
        
        protected override void OnUpdate() {
            Entity entity = GetSingletonEntity<StarfieldComponent>();
            StarfieldComponent config = this.EntityManager
                .GetComponentObject<StarfieldComponent>(entity);
            string catalog = AssetDatabase.GetAssetPath(config.Catalog);
            List<StarData> data = new List<StarData>(ParseStars(catalog));
            Parent parent = new Parent { Value = entity };

            // spawn prefabs
            NativeArray<Entity> entities = new NativeArray<Entity>(data.Count, Allocator.TempJob);
            this.EntityManager.Instantiate(config.Prefab, entities);

            // update each star
            for (int i=0; i<data.Count; i++) {
                // set parent
                this.EntityManager.AddComponentData<Parent>(entities[i], parent);
                // set size/color/position
                SetupStar(entities[i], data[i], config.Distance);
            }
            
            // clean up
            entities.Dispose();    
            // only run this system once
            this.Enabled = false;
        }

        private void SetupStar(Entity entity, StarData star, float dist) {
            // find stellar magnitude
            float mag = math.pow(2.512f / 2.0f, -(star.mag - 1f));
            // find scale
            float scale = dist / 50f * mag;
            // find rotation
            quaternion rot = quaternion.EulerZXY(
                math.radians(-star.dec), math.radians(-star.ra * DEGREES_PER_HOUR), 0f);
            // find position
            float3 pos = math.mul(rot, math.forward() * dist);
            // update local transform
            var transform = UniformScaleTransform
                .FromPositionRotationScale(pos, math.inverse(rot), scale);
            var ltpt = new LocalToParentTransform { Value = transform };
            this.EntityManager.AddComponentData<LocalToParentTransform>(entity, ltpt);
            // find star color
            // scale the temperature half closer to "white"
            float temp = star.temp + (5800f - star.temp) / 2f;
            Color color = Mathf.CorrelatedColorTemperatureToRGB(temp);
            color.a = math.min(1f, mag);
            // update sprite
            SpriteRenderer sprite = this.EntityManager
                .GetComponentObject<SpriteRenderer>(entity);
            sprite.color = color;
        }

        private IEnumerable<StarData> ParseStars(string filename) {
            string[] lines = System.IO.File.ReadAllLines(filename);
            foreach (string line in lines) {
                StarData star = new StarData();
                star.name = line.Substring(14, 11).Trim();
                if (star.name == "") {
                    continue;
                }
                // right ascension (hours, minutes, seconds)
                float rah = float.Parse(line.Substring(75, 2));
                float ram = float.Parse(line.Substring(77, 2));
                float ras = float.Parse(line.Substring(79, 4));
                // convert to hours
                star.ra = (ras / 60f + ram) / 60f + rah;
                // declination (sign, degrees, minutes, seconds)
                string sign = line.Substring(83, 1);
                float decd = float.Parse(line.Substring(84, 2));
                float decm = float.Parse(line.Substring(86, 2));
                float decs = float.Parse(line.Substring(88, 2));
                // convert to degrees
                star.dec = (decs / 60f + decm) / 60f + decd;
                star.dec *= (sign == "-") ? -1f : 1f;
                // magnitude
                star.mag = float.Parse(line.Substring(102, 5));
                // color temperature
                float bv;
                float.TryParse(line.Substring(109, 5), out bv);
                // bv is now either set or 0
                // https://en.wikipedia.org/wiki/Color_index
                star.temp = 4600f * ((1 / (0.92f * bv + 1.7f)) +
                                     (1 / (0.92f * bv + 0.62f)));
                yield return star;
            }
        }
    }
}
