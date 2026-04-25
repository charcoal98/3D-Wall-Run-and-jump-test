using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using StarterAssets;
using Cinemachine;

public class MasterControl : MonoBehaviour
{
    public TMP_Text velometor;
    public float contVelo; //is the horizontal velocity of the character controler (how the player is actually moving in engine)
    public float calcVelo; //is the horizontal velocity of the movement script (how the player is moving in the movement calculations)
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
        contVelo = playerCont.horVelo.magnitude;
        calcVelo = playerCont.acceleration.magnitude;
        contVelo *= 100;
        contVelo = Mathf.Floor(contVelo);
        contVelo /= 100;
        velometor.text = contVelo + "m/s";
        speedometorColor();
        adjustFov();
    }

    void speedometorColor()
    {
        switch (contVelo)
        {
            case 0:
                velometor.color = Color.black;
                break;
            case < 5:
                velometor.color = Color.green;
                break;
            case < 10:
                velometor.color = new Color(100, 100, 0);
                break;
            case < 15:
                velometor.color = Color.magenta;
                break;
            case >= 15:
                velometor.color = Color.red;
                break;
        }
    }

    void adjustFov()
    {
        float Fov = 2f * calcVelo + 60; //linear equation FOV should follow the graph of y = 2x + 60, so that the minimum fov is 60 and fov at max speed of 15 is 90
        cam.m_Lens.FieldOfView = Fov;
    }

    void windFX()
    {
        //WindFX is allways playing but is muted when not moving
        //When you move it increases in volume
        float volume = 0;
        volume = calcVelo/15;

        //audioSource.volume is on a scale of 0 to 1 so if calcVelo is greater than 
        if (calcVelo >= 15)
        {
            volume = 1;
        }

        audioSource.volume = volume;
    }
}
