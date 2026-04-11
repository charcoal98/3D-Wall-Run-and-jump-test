using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    public string nextlevel;

    void OnTriggerEnter(Collider other)
    {
        print("blarg");
        if(other.GetComponent<CharacterController>())
        {
            print("blarg2");
            SceneManager.LoadScene(nextlevel);
        }
    }
}
