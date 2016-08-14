using UnityEngine;
using System.Collections;
using System;

public class PlayerCityPanel : MonoBehaviour
{

    public BuildMenu buildMenu;
    public Sprite harvesterRobotSprite;
    public Sprite combatRobotSprite;

    private Animator animator;
    private PlayerCityController playerCityController;

    public static PlayerCityPanel instance;
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
        AddBuildableItemsToBuildMenu();
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

    private void AddBuildableItemsToBuildMenu()
    {
        buildMenu.AddBuildableItem(HarvesterRobotController.Settings_Name, BuyHarvesterRobot, HarvesterRobotController.Settings_copperCost, HarvesterRobotController.Settings_ironCost, harvesterRobotSprite);
        buildMenu.AddBuildableItem(CombatRobotController.Settings_Name, BuyCombatRobot, CombatRobotController.Settings_copperCost, CombatRobotController.Settings_ironCost, combatRobotSprite);
    }

    public void BuyHarvesterRobot()
    {
        instance.playerCityController.CmdBuyHarvesterRobot();
    }

    public void BuyCombatRobot()
    {
        instance.playerCityController.CmdBuyCombatRobot();
    }
}
