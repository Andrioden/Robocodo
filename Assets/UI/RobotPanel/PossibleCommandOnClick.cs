using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PossibleCommandOnClick : MonoBehaviour
{
    InputField codeInputField;
    string instruction;
    int lastCaretPosition = 0;

    public void SetupPossibleCommand(InputField codeInputField, string instruction)
    {
        this.codeInputField = codeInputField;
        this.instruction = instruction;
    }

    public void StoreCaretPosition()
    {
        if (codeInputField != null)
            lastCaretPosition = codeInputField.caretPosition;
    }

    public void CopyInstructionToCodeInput()
    {
        if (codeInputField != null)
        {
            if (instruction != string.Empty)
            {
                if (lastCaretPosition > 0)
                    codeInputField.text = codeInputField.text.Insert(lastCaretPosition, "\n" + instruction);
                else
                    codeInputField.text += codeInputField.text == string.Empty ? instruction : "\n" + instruction;
            }
        }
    }
}