using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
public class BallinputDirection : MonoBehaviour
{
    Vector2 inputDirection;
    Vector3 moveDirection;

    //Velocity Limits for the BHOP
    public float max_velocity_ground;
    public float max_velocity_air;

    //accelerate
    public float air_accelerate;
    public float ground_accelerate;

    //friction for surface
    public float friction;
    Camera cam;

    //Auto Jump bool
    public bool isJump;

    Rigidbody rb;
    public float jumpVelo;
    public float moveSpeed;
    public float lookSense;

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
        //Debug.DrawLine(Vector3.zero, new Vector3(getCameraLookFoward().normalized.x * 5, 0, getCameraLookFoward().normalized.z * 5), Color.black);
        moveDirection = inputDirection.x * getCameraLookRight().normalized + inputDirection.y * getCameraLookFoward().normalized;
        if(inputDirection != Vector2.zero){
            move(moveDirection);
        }
        if (isJump)
        {
            Jump();
            // isJump = false;
        }
    }

    public void rotateCamera()
    {
        transform.GetChild(0).rotation = Quaternion.Euler(GlobalController.direction.y * lookSense, GlobalController.direction.x * lookSense, 0);
    }

    void OnMove(InputValue value)
    {
        inputDirection = value.Get<Vector2>();
    }

    /*void OnJump()
    {
        
        Jump();
    }*/

    void OnJump(InputValue value)
    {
        Debug.Log(value.isPressed);
        isJump = value.isPressed;
    }

    void Jump()
    {
        if (grounded){
            rb.AddForce(new Vector3(0,jumpVelo,0), ForceMode.Impulse);
        }
    }

    public void move(Vector3 dir)
    {       
        rb.AddForce(new Vector3(dir.x * moveSpeed * Time.deltaTime, 0, dir.z * moveSpeed * Time.deltaTime), ForceMode.Impulse);
    }

    public Vector3 getCameraLookFoward()
    {
        Vector3 direction = transform.GetChild(0).forward;
        return direction;
    }

    public Vector3 getCameraLookRight()
    {
        Vector3 direction = transform.GetChild(0).right;
        return direction;
    }

    //thanks to Adrian Biagioli for the code  https://adrianb.io/2015/02/14/bunnyhop.html
    
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


    
}
