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

    private Color activeResearchTextColor = Utils.HexToColor("#00D9E3FF");
    private Color normalResearchTextColor;

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

        normalResearchTextColor = techPrefab.GetComponent<TechGUI>().techDescription.color;
    }

    public void Show(PlayerController player)
    {
        this.player = player;

        GenerateTechButtons();

        player.TechTree.OnTechnologyUpdated += RefreshTechnologyUI;

        panel.SetActive(true);
    }

    private void RefreshTechnologyUI()
    {
        foreach (Technology tech in player.TechTree.Technologies)
        {
            foreach (TechGUI techGUI in techGUICacheList)
            {
                if (techGUI.techId == tech.id)
                {
                    UpdateTechButton(tech, techGUI);
                    break;
                }
            }
        }
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

    private void UpdateTechButton(Technology tech, TechGUI techGUI)
    {
        string activeReseachTextPart = IsActiveResearch(tech) ? "[X]" : "";
        techGUI.techButtonText.text = string.Format("{0} {1} {2}/{3}", activeReseachTextPart, tech.name, tech.Progress, tech.cost);

        techGUI.techButton.interactable = tech.Progress < tech.cost;

        if (IsActiveResearch(tech))
        {
            techGUI.GetComponent<Image>().enabled = true;
            techGUI.techDescription.color = activeResearchTextColor;
        }
        else
        {
            techGUI.GetComponent<Image>().enabled = false;
            techGUI.techDescription.color = normalResearchTextColor;
        }
    }

    private bool IsActiveResearch(Technology tech)
    {
        return player.TechTree.activeResearch == tech;
    }

    private void SetActiveResearchClick(Technology tech)
    {
        player.TechTree.SetOrPauseActiveResearch(tech);
        EventSystem.current.SetSelectedGameObject(null);
        RefreshTechnologyUI();
    }

}