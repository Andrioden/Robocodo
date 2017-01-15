﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;

public class WonOrLostPanel : MonoBehaviour
{

    public GameObject panel;
    public Text textLabel;

    public static WonOrLostPanel instance;
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

    // Use this for initialization
    private void Start()
    {
        WinLoseChecker.instance.OnWon += Won;
        WinLoseChecker.instance.OnLost += Lost;
    }

    // Update is called once per frame
    private void Update()
    {
        //if (Input.GetKeyDown("escape") && panel.activeSelf)
        //    panel.SetActive(false);
    }

    public void Show(string text)
    {
        textLabel.text = text;
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private void Won(WinType type)
    {
        if (type == WinType.Infection_TopContributor)
            ShowInfectionWinLoss("All infection was cleared from the world, life may now slowly return to normal. The people of the world recognize you as the top contributor. Grats! You are the winner!");
        else
            throw new Exception("Win type not supported " + type);
    }

    private void Lost(LossType type)
    {
        if (type == LossType.Infection)
            Show("The infection got to high near your city, your people got infected and everyone died, good job...");
        else if (type == LossType.Infection_NotTopContributor)
            ShowInfectionWinLoss("All infection was cleared from the world, life may now slowly return to normal. However, the people of the world took note that you did not contribute enough and considers you a loser");
        else if (type == LossType.CityDestroyed)
            Show("You lost! City destroyed!");
        else if (type == LossType.StarvedToDeath)
            Show("You lost! Everyone died of starvation.");
        else
            throw new Exception("Loss type not supported " + type);
    }

    private void ShowInfectionWinLoss(string baseText)
    {
        string text = baseText;
        text += "\n";
        text += "\n";
        text += GetInfectionContributionListString();

        Show(text);
    }

    private string GetInfectionContributionListString()
    {
        var orderedPlayerContribution = WorldController.instance.FindPlayerControllers().OrderByDescending(p => p.infectionContribution).ToList();
        string text = "";

        for (int i = 0; i < orderedPlayerContribution.Count; i++)
            text += string.Format("{0}. {1}: {2}\n", i + 1, orderedPlayerContribution[i].Nick, Math.Round(orderedPlayerContribution[i].infectionContribution, 1));

        return text;
    }
}
