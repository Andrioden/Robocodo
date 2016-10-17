using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_Unknown : Instruction
{

    private string instruction;

    public Instruction_Unknown(string instruction)
    {
        this.instruction = instruction;
    }

    public override bool Execute(RobotController robot)
    {
        robot.SetFeedbackIfNotPreview(string.Format("UNKNOWN INSTRUCTION: '{0}'", instruction));
        return true;
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