using UnityEngine;
using System.Collections;
using System;

public class PlayerCityPanel : MonoBehaviour
{
    public TabController tabController;
    public BuildMenu buildMenu;
    public GarageTabController garage;
    public Sprite harvesterRobotSprite;
    public Sprite combatRobotSprite;
    public Sprite transporterRobotSprite;
    public Sprite purgeRobotSprite;

    private Animator animator;
    private CityController playerCityController;

    public static PlayerCityPanel instance;
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
                Close();
                return;
            }
        }
    }

    public void Show(CityController playerCityController)
    {
        this.playerCityController = playerCityController;
        KeyboardManager.KeyboardLockOff();
        tabController.SetFirstTabActive();
        garage.Show(playerCityController);
        animator.Play("RobotMenuSlideIn");
    }

    private void Close()
    {
        garage.Close();
        playerCityController = null;
        animator.Play("RobotMenuSlideOut");
    }

    private void AddBuildableItemsToBuildMenu()
    {
        buildMenu.AddBuildableItem(HarvesterRobotController.Settings_name, BuyHarvesterRobot, HarvesterRobotController.Settings_cost(), harvesterRobotSprite);
        buildMenu.AddBuildableItem(CombatRobotController.Settings_name, BuyCombatRobot, CombatRobotController.Settings_cost(), combatRobotSprite);
        buildMenu.AddBuildableItem(TransporterRobotController.Settings_name, BuyTransporterRobot, TransporterRobotController.Settings_cost(), transporterRobotSprite);
        buildMenu.AddBuildableItem(PurgeRobotController.Settings_name, BuyPurgeRobot, PurgeRobotController.Settings_cost(), purgeRobotSprite);
    }

    private void BuyHarvesterRobot()
    {
        if (playerCityController.CanAffordFlashIfNot(HarvesterRobotController.Settings_cost()))
            playerCityController.CmdBuyHarvesterRobot();
    }

    private void BuyCombatRobot()
    {
        if (playerCityController.CanAffordFlashIfNot(CombatRobotController.Settings_cost()))
            playerCityController.CmdBuyCombatRobot();
    }

    private void BuyTransporterRobot()
    {
        if (playerCityController.CanAffordFlashIfNot(TransporterRobotController.Settings_cost()))
            playerCityController.CmdBuyTransporterRobot();
    }

    private void BuyPurgeRobot()
    {
        if (playerCityController.CanAffordFlashIfNot(PurgeRobotController.Settings_cost()))
            playerCityController.CmdBuyPurgeRobot();
    }
}
