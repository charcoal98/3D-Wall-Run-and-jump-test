using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
public class BallMovement : MonoBehaviour
{
    Vector2 movement;
    Vector2 lookDirection;

    Camera cam;

    Rigidbody rb;
    public float jumpVelo;
    public float moveSpeed;
    public float lookSens;

    //grounded check
    bool grounded;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!Input.GetKey(KeyCode.J)){
            GlobalController.getMouse();
        }

        
        transform.GetChild(0).rotation = Quaternion.Euler(GlobalController.direction.y * lookSens, GlobalController.direction.x * lookSens, 0);

        lookDirection = getCameraLook2D();
        
        Debug.DrawLine(Vector3.zero, new Vector3(lookDirection.x * 5, 0, lookDirection.y * 5), Color.black);

        if(movement != Vector2.zero){
            move(Vector2.Lerp(lookDirection, movement, 0));
        }
        
    }

    void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    void OnJump()
    {
        rb.AddForce(new Vector3(0,jumpVelo,0), ForceMode.Impulse);
    }

    public void move(Vector2 dir)
    {
        //Debug.Log("look+Dir: " + dir.x * lookDirection.x + ", " + dir.y * lookDirection.y);
        rb.AddForce(new Vector3(dir.x * moveSpeed * Time.deltaTime, 0, dir.y * moveSpeed * Time.deltaTime), ForceMode.Impulse);
    }

    public void pointArrow()
    {
        //transform.GetChild.rotation = Quaternion.Euler(0,GlobalController.direction,0);
    }

    public Vector2 getCameraLook2D()
    {
        Vector3 direction = transform.GetChild(0).forward;
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

        // ground_accelerate and max_velocity_ground are server-defined movement variables
        return Accelerate(accelDir, prevVelocity, ground_accelerate, max_velocity_ground);
    }

    private Vector3 MoveAir(Vector3 accelDir, Vector3 prevVelocity)
    {
        // air_accelerate and max_velocity_air are server-defined movement variables
        return Accelerate(accelDir, prevVelocity, air_accelerate, max_velocity_air);
    }
    */
}
