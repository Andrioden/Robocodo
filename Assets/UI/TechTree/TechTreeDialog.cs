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

    private Color researchCompletedTechButtonTextColor = Utils.HexToColor("#CF8B31FF");
    private Color defaultTechButtonTextColor;

    private Color activeTechDescriptionTextColor = Utils.HexToColor("#00D9E3FF");
    private Color defaultTechDescriptionTextColor;

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

        defaultTechButtonTextColor = techPrefab.GetComponent<TechGUI>().techButtonText.color;
        defaultTechDescriptionTextColor = techPrefab.GetComponent<TechGUI>().techDescriptionText.color;
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
        foreach (Technology tech in player.TechTree.Technologies)
        {
            TechGUI techGUI = techGUICacheList.Find(t => t.techId == tech.id);
            UpdateTechButton(tech, techGUI);
        }
    }

    private void UpdateTechButton(Technology tech, TechGUI techGUI)
    {
        string winConditionPart = tech is Technology_Victory ? "(victory)" : "";
        techGUI.techButtonText.text = string.Format("{0} {1}/{2} ({3}%) {4}", tech.name, tech.Progress, tech.cost, tech.GetProgressPercent(), winConditionPart);

        if (tech.Progress >= tech.cost)
        {
            techGUI.techButton.interactable = false;
            techGUI.techButtonText.color = researchCompletedTechButtonTextColor;
            techGUI.GetComponent<Image>().enabled = false;
            techGUI.techDescriptionText.color = defaultTechDescriptionTextColor;
        }
        else if (IsActiveResearch(tech))
        {
            techGUI.techButton.interactable = true;
            techGUI.techButtonText.color = defaultTechButtonTextColor;
            techGUI.GetComponent<Image>().enabled = true;
            techGUI.techDescriptionText.color = activeTechDescriptionTextColor;
        }
        else
        {
            techGUI.techButton.interactable = true;
            techGUI.techButtonText.color = defaultTechButtonTextColor;
            techGUI.GetComponent<Image>().enabled = false;
            techGUI.techDescriptionText.color = defaultTechDescriptionTextColor;
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

            techGUI.techDescriptionText.text = tech.description;
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