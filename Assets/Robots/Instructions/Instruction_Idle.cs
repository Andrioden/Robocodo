using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_Idle : Instruction
{

    public static readonly string SerializedType = "IDLE";

    public override bool Execute(RobotController robot)
    {
        return true;
    }

    public override bool CanBePreviewed()
    {
        return true;
    }

    public override string Serialize()
    {
        return SerializedType;
    }

}