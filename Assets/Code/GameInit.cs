using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInit : MonoBehaviour {
    void Awake() {
#if !UNITY_EDITOR
        Debug.Log("===== Icarus init =====");
#endif
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
        Application.targetFrameRate = 60;
   }
}
