using UnityEngine;
using System.Collections.Generic;
using Assets.GameLogic.ClassExtensions;
using UnityEngine.UI;

public class ModuleMenuController : MonoBehaviour {

    public GameObject moduleInstallerPrefab;
    public GameObject noModulesAvailableTextPrefab;

    private RobotController robot;

    public void Setup(RobotController robot)
    {
        this.robot = robot;
        transform.DestroyChildren();

        var modules = GetAvailableModules();

        if (modules.Count > 0)
            modules.ForEach(module => AddModuleInstaller(this.robot, module));
        else
            Instantiate(noModulesAvailableTextPrefab, transform, false);
    }

    public List<Module> GetAvailableModules()
    {
        List<Module> modules = new List<Module>();

        AddModuleToListIfNotInstalled(modules, robot, new SolarPanelModule());

        return modules;
    }

    private void AddModuleInstaller(RobotController robot, Module module)
    {
        GameObject moduleInstallerGO = Instantiate(moduleInstallerPrefab);
        moduleInstallerGO.transform.SetParent(transform, false);

        ModuleInstaller moduleInstaller = moduleInstallerGO.GetComponent<ModuleInstaller>();
        moduleInstaller.SetupModuleInstaller(this, robot, module);
    }

    public void AddModuleToListIfNotInstalled(List<Module> modules, RobotController robot, Module module)
    {
        if (!robot.Modules.Exists(m => m.Serialize() == module.Serialize()))
            modules.Add(module);
    }
}
