using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class Instructions
{

    public static string MoveUp { get { return "MOVE UP"; } }
    public static string MoveDown { get { return "MOVE DOWN"; } }
    public static string MoveLeft { get { return "MOVE LEFT"; } }
    public static string MoveRight { get { return "MOVE RIGHT"; } }
    public static string MoveHome { get { return "MOVE HOME"; } }
    public static string Harvest { get { return "HARVEST"; } }
    public static string DropInventory { get { return "DROP INVENTORY"; } }
    public static string LoopStart { get { return "LOOP START (NUMBER)"; } }
    public static string LoopStartPlain { get { return "LOOP START"; } }
    public static string LoopEnd { get { return "LOOP END"; } }

    public static List<string> AllInstructions = new List<string>
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        MoveHome,
        Harvest,
        DropInventory,
        LoopStartPlain,
        LoopStart,
        LoopEnd
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

    public static string LoopStartNumbered(int current, int total)
    {
        return LoopStart.Replace("NUMBER", string.Format("{0}/{1}", current, total));
    }

    public static bool IsLoopStartCompleted(string instruction)
    {
        if (instruction == LoopStartPlain)
            return false;

        string paraContent = GetParenthesesContent(instruction);
        string[] paraContentSlashSplit = paraContent.Split('/');

        return paraContentSlashSplit[0] == paraContentSlashSplit[1];
    }

    public static string LoopStartReset(string instruction)
    {
        if (instruction == LoopStartPlain)
            return instruction;

        string paraContent = GetParenthesesContent(instruction);
        string[] paraContentSlashSplit = paraContent.Split('/');

        if (paraContentSlashSplit.Length == 1)
            return instruction;

        return LoopStartNumbered(0, Convert.ToInt32(paraContentSlashSplit[1]));
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