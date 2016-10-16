using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_IdleUntilThen : Instruction
{

    public static readonly string SerializedType = "IDLE UNTIL THEN";

    public override bool Execute(RobotController robot)
    {
        throw new NotImplementedException();

        //string detectionSource = instruction.Split(' ')[2];
        //if (detectionSource == "FULL" && !IsInventoryFull())
        //    return false;

        //string detectInstruction = Instructions.GetStringAfterSpace(instruction, 4);
        //return ExecuteInstruction(detectInstruction);
    }

    public override bool CanBePreviewed()
    {
        return false;
    }

    public override string Serialize()
    {
        return SerializedType;
    }

}