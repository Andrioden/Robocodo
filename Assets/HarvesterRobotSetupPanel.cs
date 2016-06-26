using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class HarvesterRobotSetupPanel : MonoBehaviour
{

    public Text title;

    public Text runButtonLabel;
    public Button runButton;

    public Text codeInputLabel;
    public InputField codeInputField;

    public Text helpTextLabel;
    public Text helpTextText;

    public HarvesterRobotController harvesterRobotController;

    public static HarvesterRobotSetupPanel instance;

    private Animator animator;

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

    public void ShowPanel(HarvesterRobotController harvesterRobotController)
    {
        this.harvesterRobotController = harvesterRobotController;

        if (harvesterRobotController.IsStarted)
        {
            title.text = "ROBOT";
            helpTextLabel.text = "";
            helpTextText.text = "";
            codeInputLabel.text = "RUNNING CODE";

            runButtonLabel.text = "Close";
            runButtonLabel.color = Color.white;
            runButton.GetComponent<Image>().color = Color.gray;
            runButton.onClick.RemoveAllListeners();
            runButton.onClick.AddListener(ClosePanel);

            codeInputField.enabled = false;
            string instructionsConcat = string.Join("\n", harvesterRobotController.GetInstructions().ToArray());
            codeInputField.text = instructionsConcat;
        }
        else {
            title.text = "ROBOT SETUP";
            runButtonLabel.text = "Run";
            helpTextLabel.text = "POSSIBLE COMMANDS";
            helpTextText.text = string.Join("\n", Instructions.AllInstructions.ToArray());
            codeInputLabel.text = "CODE INPUT";

            runButtonLabel.color = Color.black;
            runButton.GetComponent<Image>().color = new Color(0f, 236f, 226f, 255f);
            runButton.onClick.RemoveAllListeners();
            runButton.onClick.AddListener(RunCode);
            runButton.onClick.AddListener(ClosePanel);

            codeInputField.enabled = true;
            codeInputField.onValueChanged.AddListener(KeyboardManager.KeyboardLockOn);
            codeInputField.onEndEdit.AddListener(KeyboardManager.KeyboardLockOff);

            /* Pre filled demo data */
            codeInputField.text =
                Instructions.MoveUp + "\n" +
                Instructions.MoveUp + "\n" +
                Instructions.MoveUp + "\n" +
                Instructions.MoveRight + "\n" +
                Instructions.MoveRight + "\n" +
                Instructions.MoveUp + "\n" +
                Instructions.MoveUp + "\n" +
                Instructions.MoveHome;
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
}

