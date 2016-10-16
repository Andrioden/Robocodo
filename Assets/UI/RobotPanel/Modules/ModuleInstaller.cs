using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

public class ModuleInstaller : MonoBehaviour {

    public Text nameLabel;
    public Text descriptionField;
    public Button installButton;

    private ModuleMenuController moduleMenuController;
    private RobotController robot;
    private Module module;

    internal void SetupModuleInstaller(ModuleMenuController moduleMenuController, RobotController robot, Module module)
    {
        this.moduleMenuController = moduleMenuController;
        this.robot = robot;
        this.module = module;

        nameLabel.text = module.Settings_Name();
        descriptionField.text = module.Settings_Description();
        installButton.onClick.RemoveAllListeners();
        installButton.onClick.AddListener(BuyModule);
        installButton.onClick.AddListener(RefreshModuleMenu);
    }

    private void RefreshModuleMenu()
    {
        moduleMenuController.Setup(robot);
    }

    private void BuyModule()
    {
        robot.AddModule(module);
    }
}
