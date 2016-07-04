using UnityEngine;
using System.Collections;
using System;

public class PlayerCityPanel : MonoBehaviour
{

    public static PlayerCityPanel instance;

    private Animator animator;
    private PlayerCityController playerCityController;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to created another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerCityController != null)
        {
            if (MouseManager.currentlySelected == null || MouseManager.currentlySelected != playerCityController.gameObject)
            {
                ClosePanel();
                return;
            }
        }
    }

    public void ShowPanel(PlayerCityController playerCityController)
    {
        this.playerCityController = playerCityController;
        KeyboardManager.KeyboardLockOff();

        animator.Play("RobotMenuSlideIn");
    }

    private void ClosePanel()
    {
        playerCityController = null;
        animator.Play("RobotMenuSlideOut");
    }

    public PlayerCityController GetCurrentlySelectedPlayerCity()
    {
        return playerCityController;
    }
}
