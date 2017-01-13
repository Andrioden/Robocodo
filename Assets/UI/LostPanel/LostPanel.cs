using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class LostPanel : MonoBehaviour
{

    public GameObject panel;
    public Text textLabel;

    public static LostPanel instance;
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
        WinLoseChecker.instance.OnLost += Lost;
    }

    // Update is called once per frame
    private void Update()
    {
        //if (Input.GetKeyDown("escape") && panel.activeSelf)
        //    panel.SetActive(false);
    }

    private void Lost(LossType type)
    {
        if (type == LossType.Infection)
            Show("The infection got to high near your city, your people got infected and everyone died, good job...");
        else if (type == LossType.CityDestroyed)
            Show("You lost! City destroyed!");
        else if (type == LossType.StarvedToDeath)
            Show("You lost! Everyone died of starvation.");
        else
            throw new Exception("Loss type not supported " + type);
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
}
