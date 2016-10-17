using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;


public static class InstructionsHelper
{

    public static string[] SerializeList(List<Instruction> instructions)
    {
        return instructions.Select(i => i.Serialize()).ToArray();
    }

    public static Instruction Deserialize(string instruction)
    {
        if (instruction == Instruction_Idle.Format)
            return new Instruction_Idle();
        else if (Instruction_Move.IsValid(instruction))
            return Instruction_Move.Deserialize(instruction);
        else if (Instruction_LoopStart.IsValid(instruction))
            return Instruction_LoopStart.Deserialize(instruction);
        else if (instruction == Instruction_LoopEnd.Format)
            return new Instruction_LoopEnd();
        else if (instruction == Instruction_Harvest.Format)
            return new Instruction_Harvest();
        else if (instruction == Instruction_DropInventory.Format)
            return new Instruction_DropInventory();
        else if (Instruction_Attack.IsValid(instruction))
            return Instruction_Attack.Deserialize(instruction);
        else if (Instruction_DetectThen.IsValid(instruction))
            return Instruction_DetectThen.Deserialize(instruction);
        else if (Instruction_IdleUntilThen.IsValid(instruction))
            return Instruction_IdleUntilThen.Deserialize(instruction);
        else
            return new Instruction_Unknown(instruction);
    }

    public static List<Instruction> DeserializeList(List<string> instruction)
    {
        return instruction.Select(i => Deserialize(i)).ToList();
    }
    
    public static bool IsValidConditionaledInstruction(Instruction instruction)
    {
        List<Type> validTypes = new List<Type>()
        {
            typeof(Instruction_Idle),
            typeof(Instruction_Move),
            typeof(Instruction_Harvest),
            typeof(Instruction_DropInventory),
            typeof(Instruction_Attack)
        };

        return validTypes.Contains(instruction.GetType());
    }

    public static string GetStringAfterSpace(string str, int spaceNumber)
    {
        if (spaceNumber == 0)
            throw new Exception("Tried to find the string after spaceNumber 0 which obviously dont exist");

        string[] stringSplit = str.Split(' ');

        if (stringSplit.Length <= spaceNumber)
            throw new Exception(string.Format("Tried to find a string after a space that dont exists, string: '{0}', spaceNumber: {1}", str, spaceNumber));

        return string.Join(" ", stringSplit, spaceNumber, stringSplit.Length - spaceNumber);
    }

}