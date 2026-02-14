using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalController : MonoBehaviour
{
    public static Vector2 turn;
    public static Vector2 direction;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static void getMouse()
    {// Mixing old and new input system just to get something that works
    //Nothing more permant than a temporary solution
        turn.x = Input.GetAxis("Mouse X");
        turn.y = -1 * Input.GetAxis("Mouse Y");
        direction += turn;
        //Debug.Log(direction);
    }

    

}
