using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
public class BallinputDirection : MonoBehaviour
{
    Vector2 inputDirection;
    Vector2 moveDirection;

    Camera cam;

    Rigidbody rb;
    public float jumpVelo;
    public float moveSpeed;
    public static float lookSense;

    //grounded check
    public bool grounded;
    Vector3 origin;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        grounded = false;
        origin = transform.position;
        RaycastHit[] hits = Physics.SphereCastAll(origin, 1f, Vector3.down, .5f);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                grounded = true;
            }
        }

        if(!Input.GetKey(KeyCode.J)){
            GlobalController.getMouse();
        }

        //Rotate Camera
        rotateCamera();




        Debug.DrawLine(Vector3.zero, new Vector3(getCameraLookFoward().normalized.x * 5, 0, getCameraLookFoward().normalized.y * 5), Color.black);

        moveDirection = inputDirection.x * getCameraLookRight().normalized + inputDirection.y * getCameraLookFoward().normalized;
        if(inputDirection != Vector2.zero){
            move(moveDirection);
        }
        
    }

    public void rotateCamera()
    {
        float xRotation = GlobalController.direction.y * lookSense;
        transform.GetChild(0).rotation = Quaternion.Euler(xRotation, GlobalController.direction.x * lookSense, 0);
    }

    void OnMove(InputValue value)
    {
        inputDirection = value.Get<Vector2>();
    }

    void OnJump()
    {
        if (grounded)
            rb.AddForce(new Vector3(0,jumpVelo,0), ForceMode.Impulse);
    }

    public void move(Vector2 dir)
    {       
        rb.AddForce(new Vector3(dir.x * moveSpeed * Time.deltaTime, 0, dir.y * moveSpeed * Time.deltaTime), ForceMode.Impulse);
    }

    public Vector2 getCameraLookFoward()
    {
        Vector3 direction = transform.GetChild(0).forward;
        Vector2 cam = new Vector2(direction.x,direction.z);
        return cam;
    }

    public Vector2 getCameraLookRight()
    {
        Vector3 direction = transform.GetChild(0).right;
        Vector2 cam = new Vector2(direction.x,direction.z);
        return cam;
    }

    //Curtacy of adrianb
    /*
    public Vector3 Accelerate(Vector3 accelDir, Vector3 prevVelocity, float accelerate, float max_velocity)
    {
        float projVel = Vector3.Dot(prevVelocity, accelDir);
        float accelVel = accelerate * Time.fixedDeltaTime;

        if(projVel + accelVel > max_velocity)
            accelVel = max_velocity - projVel;

        return prevVelocity + accelDir * accelVel;
    }

    private Vector3 MoveGround(Vector3 accelDir, Vector3 prevVelocity)
    {
        // Apply Friction
        float speed = prevVelocity.magnitude;
        if (speed != 0) // To avoid divide by zero errors
        {
            float drop = speed * friction * Time.fixedDeltaTime;
            prevVelocity *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
        }

        // ground_accelerate and max_velocity_ground are server-defined inputDirection variables
        return Accelerate(accelDir, prevVelocity, ground_accelerate, max_velocity_ground);
    }

    private Vector3 MoveAir(Vector3 accelDir, Vector3 prevVelocity)
    {
        // air_accelerate and max_velocity_air are server-defined inputDirection variables
        return Accelerate(accelDir, prevVelocity, air_accelerate, max_velocity_air);
    }
    */
}
