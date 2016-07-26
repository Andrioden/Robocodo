using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class PossibleCommandOnClick : MonoBehaviour
{
    InputField codeInputField;
    string instruction;

    public void SetupPossibleCommand(InputField codeInputField, string instruction)
    {
        this.codeInputField = codeInputField;
        this.instruction = instruction;
    }

    public void StoreCaretPosition()
    {
        RobotPanel.instance.SaveCaretPositions();
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