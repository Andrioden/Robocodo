using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_Unknown : Instruction
{

    public override int Setting_EnergyCost() { return 1; }
    public override bool Setting_Still() { return false; }
    public override bool Setting_ConsumesTick() { return true; }
    public override bool Setting_AllowStacking() { return false; }
    public override PreviewImage Setting_PreviewImage() { return null; }
    public override bool CanBeExecutedForPreviewRobot() { return false; }

    private string instruction;

    public Instruction_Unknown(string instruction)
    {
        this.instruction = instruction;
    }

    public override bool Execute(RobotController robot)
    {
        throw new Exception("ATTEMPTED TO RUN UNKNOWN INSTRUCTION: '{0}'. Please check for unknown instruction before running it.");
    }

    public override string Serialize()
    {
        return instruction;
    }

}