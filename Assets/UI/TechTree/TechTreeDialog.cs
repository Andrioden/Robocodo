using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TechTreeDialog : MonoBehaviour
{

    public Button closeButton;
    public GameObject panel;
    public GameObject techColumn;
    public GameObject techPrefab;

    private PlayerController player;
    private List<TechGUI> techGUICacheList = new List<TechGUI>();

    private Color researcedTechButtonTextColor = Utils.HexToColor("#CF8B31FF");
    private Color normalTechButtonTextColor;

    private Color activeResearchTextColor = Utils.HexToColor("#00D9E3FF");
    private Color normalResearchTextColor;

    private DateTime _lastUpdateWhileHidden;

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

    public void Start()
    {
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(Hide);

        normalTechButtonTextColor = techPrefab.GetComponent<TechGUI>().techButtonText.color;
        normalResearchTextColor = techPrefab.GetComponent<TechGUI>().techDescription.color;
    }

    public void Show(PlayerController player)
    {
        this.player = player;
        GenerateTechButtons();

        RefreshTechnologyUI();
        player.TechTree.OnTechnologyUpdated += RefreshTechnologyUI;

        panel.SetActive(true);
    }

    public void Hide()
    {
        player.TechTree.OnTechnologyUpdated -= RefreshTechnologyUI;
        panel.SetActive(false);
    }

    public bool IsOpen()
    {
        return panel.activeSelf;
    }

    private void RefreshTechnologyUI()
    {
        if (!ShouldUpdatePanel())
            return;

        foreach (Technology tech in player.TechTree.Technologies)
        {
            TechGUI techGUI = techGUICacheList.Find(t => t.techId == tech.id);
            UpdateTechButton(tech, techGUI);
        }
    }

    private bool ShouldUpdatePanel()
    {
        if (IsOpen())
            return true;

        else
        {
            //Update every 5 seconds even when hidden to avoid a lag effect on progress when panel reopens.
            if ((DateTime.Now - _lastUpdateWhileHidden).TotalSeconds < 5)
                return false;
            else
            {
                _lastUpdateWhileHidden = DateTime.Now;
                return true;
            }
        }
    }

    private void UpdateTechButton(Technology tech, TechGUI techGUI)
    {
        string winConditionPart = tech is Technology_Victory ? "(victory)" : "";
        techGUI.techButtonText.text = string.Format("{0} {1}/{2} ({3}%) {4}", tech.name, tech.Progress, tech.cost, tech.GetProgressPercent(), winConditionPart);

        if (tech.Progress >= tech.cost)
        {
            techGUI.techButton.interactable = false;
            techGUI.techButtonText.color = researcedTechButtonTextColor;
            techGUI.GetComponent<Image>().enabled = false;
            techGUI.techDescription.color = normalResearchTextColor;
        }
        else if (IsActiveResearch(tech))
        {
            techGUI.techButton.interactable = true;
            techGUI.techButtonText.color = normalTechButtonTextColor;
            techGUI.GetComponent<Image>().enabled = true;
            techGUI.techDescription.color = activeResearchTextColor;
        }
        else
        {
            techGUI.techButton.interactable = true;
            techGUI.techButtonText.color = normalTechButtonTextColor;
            techGUI.GetComponent<Image>().enabled = false;
            techGUI.techDescription.color = normalResearchTextColor;
        }
    }

    private bool IsActiveResearch(Technology tech)
    {
        return player.TechTree.activeResearch == tech;
    }

    private void GenerateTechButtons()
    {
        foreach (Technology tech in player.TechTree.Technologies)
        {
            if (techGUICacheList.Exists(x => x.techId == tech.id))
                continue;

            GameObject techGO = Instantiate(techPrefab);
            techGO.transform.SetParent(techColumn.transform, false);
            TechGUI techGUI = techGO.GetComponent<TechGUI>();
            techGUI.techId = tech.id;

            techGUI.techButton.onClick.AddListener(delegate { SetActiveResearchClick(tech); });
            techGUI.techButton.interactable = tech.Progress < tech.cost;

            UpdateTechButton(tech, techGUI);

            techGUI.techDescription.text = tech.description;
            techGUICacheList.Add(techGUI);
        }
    }

    private void SetActiveResearchClick(Technology tech)
    {
        player.TechTree.SetOrPauseActiveResearch(tech);
        EventSystem.current.SetSelectedGameObject(null);
        RefreshTechnologyUI();
    }

}