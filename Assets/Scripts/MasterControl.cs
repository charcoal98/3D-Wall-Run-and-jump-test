using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using StarterAssets;
using Cinemachine;

public class MasterControl : MonoBehaviour
{
    public TMP_Text velometor;
    public float velo;
    public float accel;
    public FirstPersonController playerCont;
    public GameObject player;
    public CinemachineVirtualCamera cam;

    public static MasterControl instance;

    AudioSource audioSource;
    public AudioClip wind;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        playerCont = player.transform.GetChild(2).GetComponent<FirstPersonController>();
        cam = player.transform.GetChild(1).GetComponent<CinemachineVirtualCamera>();
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(wind);
    }

    // Update is called once per frame
    void Update()
    {
        windFX();
        velo = playerCont.horVelo.magnitude;
        accel = playerCont.acceleration.magnitude;
        velo *= 100;
        velo = Mathf.Floor(velo);
        velo /= 100;
        velometor.text = velo + "m/s";
        speedometorColor();
        adjustFov();
    }

    void speedometorColor()
    {
        //float colorVal = 0.0f;
        if (velo == 0)
            velometor.color = Color.black;
        else if (velo < 5)
            velometor.color = Color.green;
        else if (velo < 10)
            velometor.color = new Color(100, 100, 0);
        else if (velo < 15)
            velometor.color = Color.magenta;/*new Color(255, 150, 0);*/
        else
            velometor.color = Color.red;

        //print(velometor.color);
    }

    void adjustFov()
    {
        float Fov = 2f * accel + 60;
        cam.m_Lens.FieldOfView = Fov;//Mathf.Lerp(cam.m_Lens.FieldOfView, Fov,  0.1f * Time.deltaTime);
    }

    void windFX()
    {
        float volume = 0;
        volume = accel/15;
        if (accel >= 15)
        {
            volume = 1;
        }

        audioSource.volume = volume;
    }
}
