using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.UIElements;

public enum CrosshairType : ushort {
    Closed = 0,
    Open,
    CW,
    CCW,
    Up,
    Down
}

public class Crosshair : MonoBehaviour {
    public float interactionDistance = 2.0f;
    public Sprite[] crosshairs = new Sprite[6];

    private CrosshairType crosshair;
    private Image image;
    
    void Start() {
        this.image = GetComponent<Image>();
        this.crosshair = CrosshairType.Up;
        SetCrosshair(CrosshairType.Closed);
    }

    void Update() {
        CrosshairType wanted = CrosshairType.Open;
        int layerMask = 1 << 6;      // interaction layer
        RaycastHit hit;
        bool valid = Physics.Raycast(transform.position,
                                     transform.TransformDirection(Vector3.forward),
                                     out hit,
                                     this.interactionDistance,
                                     layerMask);
        float scrolling = Input.mouseScrollDelta.y;
        if (valid) {
            GameObject go = hit.transform.gameObject;
            IInteractable obj = go.GetComponent<IInteractable>();
            if (Input.GetButtonDown("Interact")) {
                obj.Interact();
            } else if (scrolling < 0f) {
                obj.ScrollDown();
            } else if (scrolling > 0f) {
                obj.ScrollUp();
            }
            
            wanted = obj.GetCrosshair();
        }
        SetCrosshair(wanted);
    }

    bool SetCrosshair(CrosshairType wanted) {
        if (wanted != this.crosshair) {
            // Debug.LogFormat("changing crosshair to {0}", wanted);
            this.image.sprite = this.crosshairs[(int)wanted];
            this.crosshair = wanted;
            return true;
        }
        return false;
    }
}
