using UnityEngine;
using System.Collections;

public class SelectionIndicatorController : MonoBehaviour
{
    public GameObject quad;

    // Update is called once per frame
    void Update()
    {
        if (MouseManager.currentlySelected != null)
        {
            quad.SetActive(true);
            transform.position = MouseManager.currentlySelected.transform.position;
        }
        else
            quad.SetActive(false);
    }
}