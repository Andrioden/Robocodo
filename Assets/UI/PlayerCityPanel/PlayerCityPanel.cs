using UnityEngine;
using System.Collections;
using System;

public class PlayerCityPanel : MonoBehaviour
{

    public static PlayerCityPanel instance;
    public BuildMenu buildMenu;
    public Sprite harvesterRobotSprite;

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

        /* THIS IS WHERE WE ENABLE BUILDING NEW ITEMS */
        //buildMenu.AddBuildableItem(BuyHarvesterRobot, HarvesterRobotController.Settings_CopperCost, HarvesterRobotController.Settings_IronCost, harvesterRobotSprite);
        //buildMenu.AddBuildableItem(BuyCombatRobot, CombatRobotController.CopperCost, CombatRobotController.IronCost, combatRobotSprite);
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

    public void BuyHarvesterRobot()
    {
        instance.playerCityController.CmdBuyHarvesterRobot();
    }

    public void BuyCombatRobot()
    {
        //instance.playerCityController.CmdBuyCombatRobot();
    }
}
