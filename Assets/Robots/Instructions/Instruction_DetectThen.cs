using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_DetectThen : Instruction
{

    public static readonly string SerializedType = "DETECT THEN";

    public override bool Execute(RobotController robot)
    {
        throw new NotImplementedException();

        //string detectionSource = instruction.Split(' ')[1];
        //if (detectionSource == "ENEMY" && FindNearbyEnemy((int)x, (int)z, 3.0) == null)
        //    return true;
        //else if (detectionSource == "FULL" && !IsInventoryFull())
        //    return true;
        //else if (detectionSource == "IRON" || detectionSource == "COPPER")
        //{
        //    SetFeedbackIfNotPreview("Cant detect IRON or COPPER yet. Not implemented");
        //    return true;
        //}

        //string detectInstruction = Instructions.GetStringAfterSpace(instruction, 3);
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