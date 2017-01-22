using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechTreeDialog : MonoBehaviour
{

    public Button closeButton;
    public GameObject panel;
    public GameObject techColumn;
    public GameObject techButtonPrefab;

    private PlayerController player;

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
    }

    public void Show(PlayerController player)
    {
        this.player = player;
        GenerateTechButtons();

        player.TechTree.OnTechnologyUpdated += GenerateTechButtons;

        panel.SetActive(true);
    }

    public void Hide()
    {
        player.TechTree.OnTechnologyUpdated -= GenerateTechButtons;

        panel.SetActive(false);
    }

    private void GenerateTechButtons()
    {
        techColumn.transform.DestroyChildren();

        foreach (Technology tech in player.TechTree.technologies)
        {
            GameObject techButtonGO = Instantiate(techButtonPrefab);
            Button techButton = techButtonGO.GetComponent<Button>();
            Text techButtonText = techButtonGO.GetComponentInChildren<Text>();
            techButton.transform.SetParent(techColumn.transform, false);

            techButton.onClick.AddListener(delegate { SetActiveResearchClick(tech); });

            string activeReseachTextPart = player.TechTree.activeResearch == tech ? "[X]" : "";
            techButtonText.text = string.Format("{0} {1} {2}/{3}", activeReseachTextPart, tech.name, tech.Progress, tech.cost);
        }
    }

    private void SetActiveResearchClick(Technology tech)
    {
        player.TechTree.SetActiveResearch(tech);

        GenerateTechButtons();
    }

}