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

        Cost cost = module.Settings_Cost();
        nameLabel.text = string.Format("{0} ({1}c {2}i)", module.Settings_Name(), cost.Copper, cost.Iron);
        descriptionField.text = module.Settings_Description();
        installButton.onClick.RemoveAllListeners();
        installButton.onClick.AddListener(BuyModule);
    }

    private void BuyModule()
    {
        if (robot.Owner.City.CanAffordFlashIfNot(module.Settings_Cost()))
        {
            robot.CmdAddModule(module.Serialize());
            moduleMenuController.Setup(robot, module);
        }
    }
}
