using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class Instruction_DetectThen : Instruction
{

    public override int Setting_EnergyCost() { return 1; }
    public override bool Setting_Still() { return false; }
    public override bool Setting_ConsumesTick() { return true; }
    public override bool Setting_AllowStacking() { return false; }
    public override bool CanBePreviewed() { return false; }

    public static readonly string Format = "DETECT [WHAT] THEN [INSTRUCTION]";

    private RobotController robot;
    private DetectSource detectSource;
    private Instruction thenInstruction;

    public Instruction_DetectThen(DetectSource detectSource, Instruction thenInstruction)
    {
        this.detectSource = detectSource;
        this.thenInstruction = thenInstruction;
    }

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;

        if (detectSource == DetectSource.Enemy && FindNearbyEnemy(robot.X, robot.Z, 3.0) == null)
            return true;
        else if (detectSource == DetectSource.Full && !robot.IsInventoryFull())
            return true;
        else if (detectSource == DetectSource.Iron || detectSource == DetectSource.Copper)
        {
            robot.SetFeedback("CAN NOT DETECT IRON OR COPPER YET. NOT IMPLEMENTED", true, false);
            return true;
        }

        return thenInstruction.Execute(robot);
    }

    public override string Serialize()
    {
        string serialized = Format.Replace("[WHAT]", detectSource.ToString().ToUpper());
        if (thenInstruction != null)
            serialized = serialized.Replace("[INSTRUCTION]", thenInstruction.Serialize());
        return serialized;
    }

    public static Instruction Deserialize(string instruction)
    {
        string thenInstructionString = InstructionsHelper.GetStringAfterSpace(instruction, 3);
        Instruction thenInstruction = InstructionsHelper.Deserialize(thenInstructionString);

        string detectSourceString = instruction.Split(' ')[1];
        DetectSource detectSource = Utils.ParseEnum<DetectSource>(detectSourceString);

        return new Instruction_DetectThen(detectSource, thenInstruction);
    }

    public static bool IsValid(string instruction)
    {
        if (Regex.Match(instruction, @"^DETECT \b(ENEMY|COPPER|IRON|FULL)\b THEN .+$").Success) // Understand regex better: https://regex101.com/r/aK2aM2/1
        {
            string thenInstructionString = InstructionsHelper.GetStringAfterSpace(instruction, 3);
            Instruction thenInstruction = InstructionsHelper.Deserialize(thenInstructionString);

            return InstructionsHelper.IsValidConditionaledInstruction(thenInstruction);
        }
        else
            return false;
    }

    private IAttackable FindNearbyEnemy(int x, int z, double maxDistance)
    {
        foreach (IAttackable potentialTarget in robot.FindNearbyAttackableTargets())
        {
            if (MathUtils.Distance(x, z, potentialTarget.GetX(), potentialTarget.GetZ()) <= maxDistance)
            {
                if (potentialTarget.GetOwner() != robot.GetOwner())
                    return potentialTarget;
            }
        }

        Debug.Log("Nothing nearby");
        return null;
    }

}

public enum DetectSource
{
    Enemy,
    Copper,
    Iron,
    Full
}