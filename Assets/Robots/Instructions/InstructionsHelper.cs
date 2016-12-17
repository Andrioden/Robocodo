using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;

public static class InstructionsHelper
{

    private static List<Type> _cachedInstructionTypes = null;

    public static string[] SerializeList(List<Instruction> instructions)
    {
        return instructions.Select(i => i.Serialize()).ToArray();
    }

    public static Instruction Deserialize(string instruction)
    {
        foreach(Type type in GetInstructionTypes())
        {
            bool isValid = (bool)TypeUtils.RunPublicStaticMethod(type, "IsValid", instruction);
            if (isValid)
                return (Instruction)TypeUtils.RunPublicStaticMethod(type, "Deserialize", instruction);
        }

        return new Instruction_Unknown(instruction);
    }

    public static List<Instruction> Deserialize(List<string> instruction)
    {
        return instruction.Select(i => Deserialize(i)).ToList();
    }

    private static List<Type> GetInstructionTypes()
    {
        if (_cachedInstructionTypes == null)
        {
            var targetAssembly = Assembly.GetExecutingAssembly(); // or whichever
            _cachedInstructionTypes = targetAssembly.GetTypes()
                .Where(t =>
                    t.IsSubclassOf(typeof(Instruction))
                    && TypeUtils.HasPublicStaticMethod(t, "Deserialize")
                    && TypeUtils.HasPublicStaticMethod(t, "IsValid")
                ).ToList();
        }

        return _cachedInstructionTypes;
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