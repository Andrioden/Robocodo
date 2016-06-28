﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class RobotPanel : MonoBehaviour
{
    public Text titleText;

    public Text memoryText;
    public Text feedbackText;

    public Button runButton;
    public Button closeButton;

    public Text codeInputLabel;
    public InputField codeInputField;
    public GameObject codeInput;

    public Text codeOutputLabel;
    public Text codeOutputField;
    public GameObject codeOutput;

    public Text helpTextLabel;
    public Text helpTextText;

    public Text inventoryLabel;
    public GameObject inventoryContainer;
    public GameObject ironPrefab;
    public GameObject copperPrefab;

    public static RobotPanel instance;

    private Animator animator;
    private HarvesterRobotController harvesterRobotController;
    private List<string> instructionListCopy;
    private IEnumerator feedbackClearCoroutine;

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

    private void Start()
    {
        animator = GetComponent<Animator>();

        HarvesterRobotController.OnInventoryChanged += InventoryUpdated;
    }

    private void Update()
    {
        if (harvesterRobotController != null)
        {
            if (MouseManager.currentlySelected == null || MouseManager.currentlySelected != harvesterRobotController.gameObject) { 
                ClosePanel();
                return;
            }

            instructionListCopy = harvesterRobotController.GetInstructions().Select(instruction => instruction.ToString()).ToList();

            if (harvesterRobotController.IsStarted)
            {
                if (harvesterRobotController.InstructionBeingExecutedIsValid)
                    instructionListCopy[harvesterRobotController.InstructionBeingExecuted] = "<color=#D5A042FF>" + instructionListCopy[harvesterRobotController.InstructionBeingExecuted] + "</color>";
                else {
                    instructionListCopy[harvesterRobotController.InstructionBeingExecuted] = "<color=red>" + instructionListCopy[harvesterRobotController.InstructionBeingExecuted] + "</color>";
                    SetFeedbackText("<color=red>" + harvesterRobotController.Feedback + "</color>", 0);
                }
                codeOutputField.text = string.Join("\n", instructionListCopy.ToArray());

                inventoryLabel.text = harvesterRobotController.Inventory.Count > 0 ? "INVENTORY" + harvesterRobotController.Inventory.Count + "/" + harvesterRobotController.InventoryCapacity : "INVENTORY";
            }

            var instructions = instructionListCopy.Count > 0 ? instructionListCopy : codeInputField.text.Split('\n').ToList();
            bool memoryExceeded = instructions.Count > harvesterRobotController.Memory;
            string colorPrefix = memoryExceeded ? "<color=red>" : "";
            string colorSuffix = memoryExceeded ? "</color>" : "";
            memoryText.text = colorPrefix + "MEMORY: " + instructions.Count + "/" + harvesterRobotController.Memory + colorSuffix;
        }
    }

    public void ShowPanel(HarvesterRobotController harvesterRobotController)
    {
        this.harvesterRobotController = harvesterRobotController;
        KeyboardManager.KeyboardLockOff();

        if (harvesterRobotController.IsStarted)
        {
            titleText.text = "HARVESTER";
            helpTextLabel.text = "";
            helpTextText.text = "";

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);

            codeOutput.SetActive(true);
            codeInput.SetActive(false);            
        }
        else {
            titleText.text = "HARVESTER SETUP";
            helpTextLabel.text = "POSSIBLE COMMANDS";
            helpTextText.text = string.Join("\n", Instructions.AllInstructions.ToArray());

            runButton.onClick.RemoveAllListeners();
            runButton.onClick.AddListener(RunCode);

            codeOutput.SetActive(false);
            codeInput.SetActive(true);
            codeInputField.onValueChanged.AddListener(KeyboardManager.KeyboardLockOn);
            codeInputField.onValueChanged.AddListener(CodeInputToUpper);
            codeInputField.onEndEdit.AddListener(KeyboardManager.KeyboardLockOff);
            codeInputField.text = harvesterRobotController.GetDemoInstructions();  /* Pre filled demo data */
        }

        animator.Play("RobotMenuSlideIn");
    }

    private void RunCode()
    {
        KeyboardManager.KeyboardLockOff();
        List<string> instructions = codeInputField.text.Split('\n').ToList();

        if (harvesterRobotController.RunCode(instructions))
            ClosePanel();
        else
        {
            SetFeedbackText("<color=red>" + harvesterRobotController.Feedback + "</color>", 1);
        }
    }

    private void ClosePanel()
    {
        KeyboardManager.KeyboardLockOff();
        harvesterRobotController = null;
        animator.Play("RobotMenuSlideOut");
    }

    private void CodeInputToUpper(string arg0 = "")
    {
        codeInputField.text = codeInputField.text.ToUpper();
    }

    private void SetFeedbackText(string feedback, float durationSeconds)
    {
        feedbackText.text = feedback;

        if (feedbackClearCoroutine != null)
            StopCoroutine(feedbackClearCoroutine);

        feedbackClearCoroutine = RemoveFeedbackAfterSeconds(durationSeconds);
        StartCoroutine(feedbackClearCoroutine);
    }

    IEnumerator RemoveFeedbackAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        feedbackText.text = "";
    }

    private void InventoryUpdated(HarvesterRobotController robot)
    {
        if(harvesterRobotController != null && robot == harvesterRobotController)
        {
            Debug.Log("InventoryUpdated: " + harvesterRobotController.Inventory);

            foreach (Transform child in inventoryContainer.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            var currentInventory = harvesterRobotController.Inventory;
            currentInventory.ForEach(item => AddInventoryItem(item));
        }
    }

    private void AddInventoryItem(InventoryItem item)
    {
        GameObject prefab = null;
        if (item is IronItem)
            prefab = ironPrefab;
        else if (item is CopperItem)
            prefab = copperPrefab;

        var inventoryImage = Instantiate(prefab) as GameObject;
        inventoryImage.transform.SetParent(inventoryContainer.transform, false);
    }

    //private void InventoryDropped(HarvesterRobotController robot)
    //{
    //    if (harvesterRobotController != null && robot == harvesterRobotController)
    //    {
    //        foreach (Transform child in inventoryContainer.transform)
    //        {
    //            GameObject.Destroy(child.gameObject);
    //        }
    //    }
    //}
}

