using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public Transform mainCamera;
    public GravitySource gravity;

    public float speed = 3f;
    public float jumpHeight = 3f;
    public float mouseSensitivity = 200f;
    public float runMultiplier = 3f;
    public float airMultiplier = 0.05f;
    public float groundDrag = 10f;
    public float airDrag = 0f;

    bool isGrounded;
    float xRotation = 0f;
    Rigidbody rb;

    void Start() {
        this.rb = this.GetComponent<Rigidbody>();
        this.rb.freezeRotation = true;
    }

    void Update() {
        if (!Application.isFocused) return;
        CheckInputs();
        MouseLook();
        PerformMovement();
        DebugWidgets();
    }

    void FixedUpdate() {
        this.rb.drag = this.isGrounded ? this.groundDrag : this.airDrag;
    }

    void DebugWidgets() {
        float length = 0.1f;
        DebugRay(0.75f, 0.75f, length * this.transform.forward, Color.blue, true);
        DebugRay(0.75f, 0.75f, length * this.transform.right, Color.red, true);
        DebugRay(0.75f, 0.75f, length * this.transform.up, Color.green, true);
    }
    
    void DebugRay(float x, float y, Vector3 ray, Color color) {
        DebugRay(x, y, ray, color, false);
    }
    
    void DebugRay(float x, float y, Vector3 ray, Color color, bool relative) {
        Camera cam = this.mainCamera.GetComponent<Camera>();
        Ray look = cam.ViewportPointToRay(new Vector3(x, y, 0f));
        Vector3 origin = this.mainCamera.transform.position + look.direction * 0.5f;
        if (relative) {
            ray += origin;
        }
        Debug.DrawLine(origin, ray, color);
    }

    void MouseLook() {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        Vector3 realUp = -this.gravity.ForceVector(this.transform.position).normalized;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        mainCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // https://gamedev.stackexchange.com/questions/87176/lookrotation-of-a-gameobject-in-just-one-axis/100887#100887
        Vector3 forward = this.rb.rotation
            * Quaternion.AngleAxis(mouseX, Vector3.up)
            * Vector3.forward;
        this.rb.rotation = Quaternion.LookRotation(realUp, -forward)
            * Quaternion.AngleAxis(90f, Vector3.right);
        
        DebugRay(0.5f, 0.5f, this.rb.rotation * Vector3.up, Color.red, true);
        DebugRay(0.5f, 0.5f, realUp, Color.blue, true);
        // Debug.Log(Vector3.Dot(this.rb.rotation * Vector3.up, realUp));
    }

    void PerformMovement() {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 velocity = (Vector3.right * x) + (Vector3.forward * z);
        velocity = velocity.normalized * speed * this.rb.drag * Time.deltaTime;

        if (!this.isGrounded) {
            velocity *= this.airMultiplier;
        } else if (Input.GetButton("Run")) {
            velocity *= this.runMultiplier;
        }

        if (Input.GetButtonDown("Jump") && this.isGrounded) {
            // Debug.Log("jump");
            float g = this.gravity.ForceVector(this.transform.position).magnitude;
            // jump v = sqrt(h * -2g)
            velocity += Vector3.up * Mathf.Sqrt(jumpHeight * 2f * g);
        }
        
        this.rb.AddRelativeForce(velocity, ForceMode.VelocityChange);
    }

    void DisplayGravity() {
        Vector3 g = this.gravity.ForceVector(this.transform.position);
        float force = g.magnitude;
        float gs = force / 9.81f;
        Debug.LogFormat("g={0} rot={1}", gs, this.gravity.RotationRate);
    }

    void CheckInputs() {
        if (Input.GetKeyDown("1")) {
            // Debug.Log("key one");
            this.gravity.RotationRate -= 0.05f;
            DisplayGravity();
        }
        if (Input.GetKeyDown("2")) {
            // Debug.Log("key two");
            this.gravity.RotationRate += 0.05f;
            DisplayGravity();
        }
    }

    void OnCollisionEnter(Collision collision) {
        this.isGrounded = true;
    }
    
    void OnCollisionExit(Collision collision) {
        this.isGrounded = false;
    }
}
