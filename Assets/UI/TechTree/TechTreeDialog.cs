using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechTreeDialog : MonoBehaviour
{

    private PlayerController localPlayer;

    public static TechTreeDialog instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.LogError("Tried to create another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    public void RegisterLocalPlayer(PlayerController player)
    {
        localPlayer = player;
    }

}