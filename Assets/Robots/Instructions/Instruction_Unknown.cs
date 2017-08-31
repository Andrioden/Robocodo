using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_Unknown : Instruction
{

    public override int Setting_EnergyCost() { return 1; }
    public override bool IsStill() { return false; }

    private string instruction;

    public Instruction_Unknown(string instruction)
    {
        this.instruction = instruction;
    }

    public override bool Execute(RobotController robot)
    {
        throw new Exception("ATTEMPTED TO RUN UNKNOWN INSTRUCTION: '{0}'. Please check for unknown instruction before running it.");
    }

    public override bool CanBePreviewed()
    {
        return false;
    }

    public override string Serialize()
    {
        return instruction;
    }

}