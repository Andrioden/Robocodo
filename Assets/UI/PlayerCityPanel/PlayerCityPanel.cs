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
                Close();
                return;
            }
        }
    }

    public void Show(PlayerCityController playerCityController)
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
        buildMenu.AddBuildableItem(HarvesterRobotController.Settings_name, BuyHarvesterRobot, HarvesterRobotController.Settings_copperCost, HarvesterRobotController.Settings_ironCost, harvesterRobotSprite);
        buildMenu.AddBuildableItem(CombatRobotController.Settings_name, BuyCombatRobot, CombatRobotController.Settings_copperCost, CombatRobotController.Settings_ironCost, combatRobotSprite);
        buildMenu.AddBuildableItem(TransporterRobotController.Settings_name, BuyTransporterRobot, TransporterRobotController.Settings_copperCost, TransporterRobotController.Settings_ironCost, transporterRobotSprite);
    }

    private void BuyHarvesterRobot()
    {
        instance.playerCityController.CmdBuyHarvesterRobot();
    }

    private void BuyCombatRobot()
    {
        instance.playerCityController.CmdBuyCombatRobot();
    }

    private void BuyTransporterRobot()
    {
        instance.playerCityController.CmdBuyTransporterRobot();
    }
}
