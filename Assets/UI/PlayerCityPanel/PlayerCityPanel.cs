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
    public Sprite storageRobotSprite;
    public Sprite purgeRobotSprite;

    private Animator animator;
    private CityController city;

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
    }

    void Update()
    {
        if (city != null)
        {
            if (MouseManager.currentlySelected == null || MouseManager.currentlySelected != city.gameObject)
            {
                Close();
                return;
            }
        }
    }

    public void Show(CityController city)
    {
        this.city = city;
        KeyboardManager.KeyboardLockOff();
        SetBuildableItemsToBuildMenu();
        tabController.SetFirstTabActive();
        garage.Show(city);
        animator.Play("RobotMenuSlideIn");
    }

    private void Close()
    {
        garage.Close();
        city = null;
        animator.Play("RobotMenuSlideOut");
    }

    private void SetBuildableItemsToBuildMenu()
    {
        buildMenu.ClearBuildables();

        if (city.Owner.TechTree.HasRobotTech(typeof(HarvesterRobotController)))
            buildMenu.AddBuildableItem(HarvesterRobotController.Settings_name, BuyHarvesterRobot, HarvesterRobotController.Settings_cost(), harvesterRobotSprite);

        if (city.Owner.TechTree.HasRobotTech(typeof(CombatRobotController)))
            buildMenu.AddBuildableItem(CombatRobotController.Settings_name, BuyCombatRobot, CombatRobotController.Settings_cost(), combatRobotSprite);

        if (city.Owner.TechTree.HasRobotTech(typeof(TransporterRobotController)))
            buildMenu.AddBuildableItem(TransporterRobotController.Settings_name, BuyTransporterRobot, TransporterRobotController.Settings_cost(), transporterRobotSprite);

        if (city.Owner.TechTree.HasRobotTech(typeof(StorageRobotController)))
            buildMenu.AddBuildableItem(StorageRobotController.Settings_name, BuyStorageRobot, StorageRobotController.Settings_cost(), storageRobotSprite);

        if (city.Owner.TechTree.HasRobotTech(typeof(PurgeRobotController)))
            buildMenu.AddBuildableItem(PurgeRobotController.Settings_name, BuyPurgeRobot, PurgeRobotController.Settings_cost(), purgeRobotSprite);
    }

    private void BuyHarvesterRobot()
    {
        if (city.CanAffordFlashIfNot(HarvesterRobotController.Settings_cost()))
        {
            city.CmdBuyHarvesterRobot();
            instance.buildMenu.IndicateSuccessfulPurchase(HarvesterRobotController.Settings_name);
        }
    }

    private void BuyCombatRobot()
    {
        if (city.CanAffordFlashIfNot(CombatRobotController.Settings_cost()))
        {
            city.CmdBuyCombatRobot();
            instance.buildMenu.IndicateSuccessfulPurchase(CombatRobotController.Settings_name);
        }
    }

    private void BuyTransporterRobot()
    {
        if (city.CanAffordFlashIfNot(TransporterRobotController.Settings_cost()))
        {
            city.CmdBuyTransporterRobot();
            instance.buildMenu.IndicateSuccessfulPurchase(TransporterRobotController.Settings_name);
        }
    }

    private void BuyStorageRobot()
    {
        if (city.CanAffordFlashIfNot(StorageRobotController.Settings_cost()))
        {
            city.CmdBuyStorageRobot();
            instance.buildMenu.IndicateSuccessfulPurchase(StorageRobotController.Settings_name);
        }
    }

    private void BuyPurgeRobot()
    {
        if (city.CanAffordFlashIfNot(PurgeRobotController.Settings_cost()))
        {
            city.CmdBuyPurgeRobot();
            instance.buildMenu.IndicateSuccessfulPurchase(PurgeRobotController.Settings_name);
        }
    }
}
