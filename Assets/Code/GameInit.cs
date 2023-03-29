using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInit : MonoBehaviour {
    void Awake() {
        // Application.runInBackground = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

#if !UNITY_EDITOR
        // Debug.Log($"loading sub scenes");
        // SceneManager.LoadSceneAsync("Assets/Scenes/Sub Scenes/Space Sub Scene/Space Sub Scene.unity", LoadSceneMode.Additive);
        // SceneManager.LoadSceneAsync("Assets/Scenes/Sub Scenes/Player Ship Sub Scene/Player Ship Sub Scene.unity", LoadSceneMode.Additive);
        // SceneManager.LoadScene(2, LoadSceneMode.Additive);
#endif
    }
}
