﻿using UnityEngine;
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
    public Sprite batteryRobotSprite;

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
            if (MouseManager.instance.CurrentlySelectedObject == null || MouseManager.instance.CurrentlySelectedObject != city.gameObject)
            {
                Close();
                return;
            }
        }
    }

    public void Show(CityController city)
    {
        if (this.city == city)
            return;

        this.city = city;
        KeyboardManager.KeyboardLockOff();
        UpdateBuildMenuWithBuildableItems();
        tabController.SetFirstTabActive();
        garage.Show(city);
        animator.Play("RobotMenuSlideIn");

        city.Owner.TechTree.OnNewRobotResearched += UpdateBuildMenuWithBuildableItems;
    }

    private void Close()
    {
        garage.Close();
        city.Owner.TechTree.OnNewRobotResearched -= UpdateBuildMenuWithBuildableItems;
        city = null;
        animator.Play("RobotMenuSlideOut");
    }

    private void UpdateBuildMenuWithBuildableItems()
    {
        if (city.Owner.TechTree.IsRobotTechResearched(typeof(HarvesterRobotController)))
            buildMenu.AddBuildItemIfNotExists(HarvesterRobotController.Settings_name, BuyHarvesterRobot, HarvesterRobotController.Settings_cost(), harvesterRobotSprite);

        if (city.Owner.TechTree.IsRobotTechResearched(typeof(CombatRobotController)))
            buildMenu.AddBuildItemIfNotExists(CombatRobotController.Settings_name, BuyCombatRobot, CombatRobotController.Settings_cost(), combatRobotSprite);

        if (city.Owner.TechTree.IsRobotTechResearched(typeof(TransporterRobotController)))
            buildMenu.AddBuildItemIfNotExists(TransporterRobotController.Settings_name, BuyTransporterRobot, TransporterRobotController.Settings_cost(), transporterRobotSprite);

        if (city.Owner.TechTree.IsRobotTechResearched(typeof(StorageRobotController)))
            buildMenu.AddBuildItemIfNotExists(StorageRobotController.Settings_name, BuyStorageRobot, StorageRobotController.Settings_cost(), storageRobotSprite);

        if (city.Owner.TechTree.IsRobotTechResearched(typeof(PurgeRobotController)))
            buildMenu.AddBuildItemIfNotExists(PurgeRobotController.Settings_name, BuyPurgeRobot, PurgeRobotController.Settings_cost(), purgeRobotSprite);

        if (city.Owner.TechTree.IsRobotTechResearched(typeof(BatteryRobotController)))
            buildMenu.AddBuildItemIfNotExists(BatteryRobotController.Settings_name, BuyBatteryRobot, BatteryRobotController.Settings_cost(), batteryRobotSprite);
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

    private void BuyBatteryRobot()
    {
        if (city.CanAffordFlashIfNot(BatteryRobotController.Settings_cost()))
        {
            city.CmdBuyBatteryRobot();
            instance.buildMenu.IndicateSuccessfulPurchase(BatteryRobotController.Settings_name);
        }
    }

}