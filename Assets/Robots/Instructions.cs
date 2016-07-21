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
    public const string LoopStartNumbered = "LOOP START [NUMBER]";
    public const string LoopEnd = "LOOP END";

    public const string Harvest = "HARVEST";
    public const string DropInventory = "DROP INVENTORY";

    public const string AttackMelee = "ATTACK MELEE";
    public const string AttackUp = "ATTACK UP";
    public const string AttackDown = "ATTACK DOWN";
    public const string AttackLeft = "ATTACK LEFT";
    public const string AttackRight = "ATTACK RIGHT";

    public static bool IsValidLoopStart(string instruction)
    {
        if (instruction == "LOOP START")
            return true;
        if (Regex.Match(instruction, @"^LOOP START \d+$").Success) // Understand this better: https://regex101.com/r/lY9pP4/2
            return true;
        if (Regex.Match(instruction, @"^LOOP START \d+\/\d+$").Success) // Understand this better: https://regex101.com/r/bR3kG5/2
            return true;
        return false;
    }

    public static string LoopStartNumberedSet(int current, int total)
    {
        return LoopStartNumbered.Replace("[NUMBER]", string.Format("{0}/{1}", current, total));
    }

    public static bool IsLoopStartCompleted(string instruction)
    {
        if (instruction == LoopStart)
            return false;

        string loopNumber = instruction.Replace(LoopStart, "").Trim();
        string[] loopNumberSplit = loopNumber.Split('/');

        return loopNumberSplit[0] == loopNumberSplit[1];
    }

    public static string LoopStartReset(string instruction)
    {
        if (instruction == LoopStart)
            return instruction;

        string loopNumber = instruction.Replace(LoopStart, "").Trim();
        string[] loopNumberSplit = loopNumber.Split('/');

        if (loopNumberSplit.Length == 1)
            return instruction;

        return LoopStartNumberedSet(0, Convert.ToInt32(loopNumberSplit[1]));
    }

}