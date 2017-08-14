using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CityButton : MonoBehaviour {

    private Button cityButton;
    private CityController city;

	private void Start () {
        cityButton = GetComponent<Button>();

        cityButton.onClick.RemoveAllListeners();
        cityButton.onClick.AddListener(CenterCameraAndSelectCity);
    }

    private void CenterCameraAndSelectCity()
    {
        if(city == null)
            city = WorldController.instance.FindClientsOwnPlayer().City;

        RTSCamera.instance.PositionRelativeTo(city.transform);
        MouseManager.instance.ClickGameObject(city.gameObject);
        EventSystem.current.SetSelectedGameObject(null);
    }
}
