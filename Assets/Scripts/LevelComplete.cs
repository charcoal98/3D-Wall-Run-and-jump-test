using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    public string nextlevel;

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<CharacterController>())
        {
            SceneManager.LoadScene(nextlevel);
        }
    }
}
