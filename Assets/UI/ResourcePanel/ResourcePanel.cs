﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class ResourcePanel : MonoBehaviour
{

    public Text nickLabel;
    public Text copperLabel;
    public Text ironLabel;

    private PlayerCityController localPlayerCity;

    public static ResourcePanel instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to created another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    private void Start()
    {
        InvokeRepeating("UpdateResourceLabels", 0, 0.3f); // Dont update it to often, so we use a slow updater
    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void RegisterLocalPlayerCity(PlayerCityController playerCity)
    {
        localPlayerCity = playerCity;
    }

    private void UpdateResourceLabels()
    {
        if (localPlayerCity != null)
        {
            nickLabel.text = localPlayerCity.Nick;
            copperLabel.text = "Copper: " + localPlayerCity.GetCopperCount();
            ironLabel.text = "Iron: " + localPlayerCity.GetIronCount();
        }
        else
        {
            nickLabel.text = "";
            copperLabel.text = "Copper: " + 0;
            ironLabel.text = "Iron: " + 0;
        }
    }

}