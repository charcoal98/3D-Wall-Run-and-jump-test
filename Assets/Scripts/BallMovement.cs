using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
public class BallMovement : MonoBehaviour
{
    Vector2 movement;
    Rigidbody rb;
    public float moveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        move(movement);
    }

    void OnMove(InputValue value)
    {
        
        movement = value.Get<Vector2>();
        Debug.Log(movement);
    }

    public void move(Vector2 dir)
    {
        rb.AddForce(new Vector3(dir.x * moveSpeed * Time.deltaTime, 0, dir.y * moveSpeed * Time.deltaTime), ForceMode.Impulse);
    }

}
