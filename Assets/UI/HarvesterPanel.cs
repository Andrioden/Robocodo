using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class HarvesterPanel : MonoBehaviour
{
    public int TEST = 0;
    public Text title;

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

    public static HarvesterPanel instance;

    private Animator animator;
    private HarvesterRobotController harvesterRobotController;
    private List<string> listCopy;

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
    }

    private void Update()
    {
        if (harvesterRobotController != null)
        {
            if (harvesterRobotController.IsStarted && harvesterRobotController.GetInstructions().Count > 0)
            {
                listCopy = harvesterRobotController.GetInstructions().Select(instruction => instruction.ToString()).ToList();
                if(harvesterRobotController.InstructionBeingExecutedIsValid)
                    listCopy[harvesterRobotController.InstructionBeingExecuted] = "<color=#D5A042FF>" + listCopy[harvesterRobotController.InstructionBeingExecuted] + "</color>";
                else
                    listCopy[harvesterRobotController.InstructionBeingExecuted] = "<color=red>" + listCopy[harvesterRobotController.InstructionBeingExecuted] + "</color>";
                codeOutputField.text = string.Join("\n", listCopy.ToArray());
            }
        }
    }

    public void ShowPanel(HarvesterRobotController harvesterRobotController)
    {
        this.harvesterRobotController = harvesterRobotController;
        KeyboardManager.KeyboardLockOff();

        if (harvesterRobotController.IsStarted)
        {
            title.text = "HARVESTER";
            helpTextLabel.text = "";
            helpTextText.text = "";

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);

            codeOutput.SetActive(true);
            codeInput.SetActive(false);
        }
        else {
            title.text = "HARVESTER SETUP";
            helpTextLabel.text = "POSSIBLE COMMANDS";
            helpTextText.text = string.Join("\n", Instructions.AllInstructions.ToArray());

            runButton.onClick.RemoveAllListeners();
            runButton.onClick.AddListener(RunCode);
            runButton.onClick.AddListener(ClosePanel);

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
        harvesterRobotController.RunCode(instructions);
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
}

