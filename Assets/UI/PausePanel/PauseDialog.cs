using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseDialog : MonoBehaviour
{

    public GameObject panel;

    // Update is called once per frame
    private void Update()
    {
        InvokeRepeating("ShowOrHidePauseDialog", 0, 0.2f); // Dont update it to often, so we use a slow updater
    }

    private void ShowOrHidePauseDialog()
    {
        if (Time.timeScale == 0)
            panel.SetActive(true);
        else
            panel.SetActive(false);
    }
}
