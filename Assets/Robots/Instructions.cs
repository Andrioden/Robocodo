using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class Instructions
{
    public const string MoveUp = "MOVE UP";
    public const string MoveDown = "MOVE DOWN";
    public const string MoveLeft = "MOVE LEFT";
    public const string MoveRight = "MOVE RIGHT";
    public const string MoveHome = "MOVE HOME";
    public const string LoopStart = "LOOP START";
    public const string LoopStartNumbered = "LOOP START (NUMBER)";
    public const string LoopEnd = "LOOP END";

    public const string Harvest = "HARVEST";
    public const string DropInventory = "DROP INVENTORY";

    public const string MeleeAttack = "MELEE ATTACK";

    // TODO: Remove when we get the list from the Robot object
    public static List<string> AllInstructions = new List<string>
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        MoveHome,
        Harvest,
        DropInventory,
        LoopStart,
        LoopStartNumbered,
        LoopEnd,
        MeleeAttack
    };

    public static bool IsValidLoopStart(string instruction)
    {
        if (instruction == "LOOP START")
            return true;
        if (Regex.Match(instruction, @"^LOOP START \(\d+\)$").Success) // Understand this better: https://regex101.com/r/lY9pP4/1
            return true;
        if (Regex.Match(instruction, @"^LOOP START \(\d+\/\d+\)$").Success) // Understand this better: https://regex101.com/r/bR3kG5/1
            return true;
        return false;
    }

    public static string LoopStartNumberedSet(int current, int total)
    {
        return LoopStartNumbered.Replace("NUMBER", string.Format("{0}/{1}", current, total));
    }

    public static bool IsLoopStartCompleted(string instruction)
    {
        if (instruction == LoopStart)
            return false;

        string paraContent = GetParenthesesContent(instruction);
        string[] paraContentSlashSplit = paraContent.Split('/');

        return paraContentSlashSplit[0] == paraContentSlashSplit[1];
    }

    public static string LoopStartReset(string instruction)
    {
        if (instruction == LoopStart)
            return instruction;

        string paraContent = GetParenthesesContent(instruction);
        string[] paraContentSlashSplit = paraContent.Split('/');

        if (paraContentSlashSplit.Length == 1)
            return instruction;

        return LoopStartNumberedSet(0, Convert.ToInt32(paraContentSlashSplit[1]));
    }

    public static string RemoveFirstParenthesesPart(string str)
    {
        int indexOfParenthesesStart = str.IndexOf('(');
        int indexOfParenthesesEnd = str.IndexOf(')');

        if (indexOfParenthesesStart == -1 || indexOfParenthesesEnd == -1)
            return str;

        return str.Remove(indexOfParenthesesStart, indexOfParenthesesEnd - indexOfParenthesesStart + 1).Trim();
    }

    public static string GetParenthesesContent(string str)
    {
        int indexOfParenthesesStart = str.IndexOf('(');
        int indexOfParenthesesEnd = str.IndexOf(')');

        if (indexOfParenthesesStart == -1 || indexOfParenthesesEnd == -1)
            throw new Exception("Missing start or end parentheses in string. Should not happen. String is: " + str);

        return str.Substring(indexOfParenthesesStart + 1, indexOfParenthesesEnd - indexOfParenthesesStart - 1);
    }

    // Outdated after LOOP START with number input was added
    //public static bool IsValidInstruction(string instruction)
    //{

    //    return AllInstructions.Contains(instruction);
    //}

    // Outdated after LOOP START with number input was added
    //public static bool IsValidInstructionList(List<string> instructions)
    //{
    //    foreach (string instructionString in instructions)
    //    {
    //        if (!IsValidInstruction(instructionString))
    //            return false;
    //    }

    //    return true;
    //}

}