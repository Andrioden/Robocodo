using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System;

public class PossibleInstructionOnClick : MonoBehaviour
{
    private InputField codeInputField;
    private string instruction;

    private Text textComponent;
    private Color originalColor;
    private Color hoverColor = Utils.HexToColor("FDC053FF");

    private void Awake()
    {
        textComponent = GetComponent<Text>();
        originalColor = textComponent.color;
    }

    public void SetupPossibleInstruction(InputField codeInputField, string instruction)
    {
        this.codeInputField = codeInputField;
        this.instruction = instruction;
    }

    public void SaveCaretPosition()
    {
        RobotPanel.instance.SaveCaretPositions();
        ApplyHoverTextEffect();
    }

    private void ApplyHoverTextEffect()
    {
        textComponent.color = hoverColor;
    }

    public void RemoveHoverTextEffect()
    {
        textComponent.color = originalColor;
    }

    public void CopyInstructionToCodeInput()
    {
        if (codeInputField != null)
        {
            if (instruction != string.Empty)
            {
                RobotPanel.instance.AppendNewInstructionAtCaretPosition(instruction);
            }
        }
    }
}