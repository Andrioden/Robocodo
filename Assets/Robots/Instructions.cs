using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Instructions
{

    public static string MoveUp { get { return "MOVE UP"; } }
    public static string MoveDown { get { return "MOVE DOWN"; } }
    public static string MoveLeft { get { return "MOVE LEFT"; } }
    public static string MoveRight { get { return "MOVE RIGHT"; } }
    public static string MoveHome { get { return "MOVE HOME"; } }
    public static string Harvest { get { return "HARVEST"; } }
    public static string DropInventory { get { return "DROP INVENTORY"; } }

    public static List<string> AllInstructions = new List<string>
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        MoveHome,
        Harvest,
        DropInventory
    };

    public static bool IsValidInstruction(string instruction)
    {
        return AllInstructions.Contains(instruction);
    }

    public static bool IsValidInstructionList(List<string> instructions)
    {
        foreach (string instructionString in instructions)
        {
            if (!IsValidInstruction(instructionString))
                return false;
        }

        return true;
    }

}