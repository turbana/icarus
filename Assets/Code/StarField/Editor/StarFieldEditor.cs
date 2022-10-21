using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;

public struct StarData {
    public string name;
    public float ra;
    public float dec;
    public float mag;
    public float temp;

    public override string ToString() => $"<{name} ({ra}ra {dec}dec) {mag}mag {temp}K>";
}

[CustomEditor(typeof(StarField))]
public class StarFieldEditor : Editor {
    private StarField config;

    private const float DEGREES_PER_HOUR = 360f / 24f;

    private void OnEnable() {
        this.config = target as StarField;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Star Field")) {
            if (config.catalog != null) {
                DeleteStars();
                GenerateStars();
            } else {
                Debug.LogError("catalog must be set");
            }
        }
    }

    private void DeleteStars() {
        while (config.transform.childCount > 0) {
            Object.DestroyImmediate(config.transform.GetChild(0).gameObject);
        }
    }

    private void GenerateStars() {
        string path = AssetDatabase.GetAssetPath(config.catalog);
        foreach (StarData star in ParseStars(path)) {
            GameObject go = new GameObject(star.ToString());
            // set parent
            go.transform.SetParent(config.transform);
            // set position
            Quaternion rot = Quaternion.Euler(-star.dec, -star.ra * DEGREES_PER_HOUR, 0f);
            go.transform.Translate(rot * (Vector3.forward * config.starDistance));
            // face origin
            go.transform.LookAt(config.transform);
            // magnitude
            // change scale from -2.0 <-> ~8.0 into 3.0 <-> 0.15
            float mscale = (star.mag + 2f) / 10f;
            // float mag = Mathf.Lerp(4f,  0.1f, mscale);
            float mag = Mathf.Exp(Mathf.Lerp(2f, 0f, mscale));
            mag /= 2f;
            // set scale
            float scale = config.starDistance / 100f * mag;
            go.transform.localScale = Vector3.one * scale;
            // create material
            Material mat = new Material(config.material);
            float rtemp = 6000f * (Random.value - 0.5f);
            Color temp = Mathf.CorrelatedColorTemperatureToRGB(star.temp + rtemp);
            mat.color = new Color(temp.r, temp.g, temp.b, 1f - mscale);
            // add sprite renderer
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = config.sprite;
            sr.materials = new Material[1]{ mat };
        }
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
            star.temp = 6500f;
            yield return star;
        }
    }
}
