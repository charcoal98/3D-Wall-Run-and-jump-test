using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using StarterAssets;

public class MasterControl : MonoBehaviour
{
    public TMP_Text velometor;
    public float velo;
    public FirstPersonController playerCont;
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        playerCont = player.transform.GetChild(2).GetComponent<FirstPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        
        velo = playerCont.horVelo.magnitude;
        velo *= 100;
        velo = Mathf.Floor(velo);
        velo /= 100;
        velometor.text = velo + "m/s";
    }
}
