using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

public class TabController : MonoBehaviour
{
    public GameObject tabButtonsContainer;
    public GameObject[] panels;
    public GameObject tabButtonPrefab;

    private List<TabPanelButtonPair> tabs = new List<TabPanelButtonPair>();

    void Start()
    {
        if (!tabButtonsContainer)
            Debug.LogError("Tab buttons container reference missing.");
        if (!tabButtonPrefab)
            Debug.LogError("Missing tab button prefab");

        foreach (GameObject panel in panels)
        {
            GameObject buttonGO = (GameObject)Instantiate(tabButtonPrefab);
            buttonGO.transform.SetParent(tabButtonsContainer.transform);
            buttonGO.transform.localScale = new Vector3(1, 1, 1);
            Button button = buttonGO.GetComponent<Button>();
            button.onClick.AddListener(delegate { ToggleButtonsTabActive(button); });
            button.GetComponentInChildren<Text>().text = panel.name;
            tabs.Add(new TabPanelButtonPair(panel, button));
        }

        SetFirstTabActive();
    }

    public void SetFirstTabActive()
    {
        ToggleButtonsTabActive(tabs.Select(x => x.button).FirstOrDefault());
    }

    public void ToggleButtonsTabActive(Button button)
    {
        var currentTab = tabs.Where(x => x.button == button).Select(x => x).FirstOrDefault();
        var otherTabs = tabs.Where(x => x.button != button).Select(x => x).ToList();

        otherTabs.ForEach(x =>
        {
            x.text.fontStyle = FontStyle.Normal;
            x.text.fontSize = 30;
            x.panel.SetActive(false);
        });

        currentTab.text.fontStyle = FontStyle.Bold;
        currentTab.text.fontSize = 35;
        currentTab.panel.SetActive(true);
    }

    public class TabPanelButtonPair
    {
        public GameObject panel;
        public Button button;
        public Text text;

        public TabPanelButtonPair(GameObject panel, Button button)
        {
            this.panel = panel;
            this.button = button;
            text = button.GetComponentInChildren<Text>();
        }
    }
}
