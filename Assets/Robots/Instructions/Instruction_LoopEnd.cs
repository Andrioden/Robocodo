using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_LoopEnd : Instruction
{

    public static readonly string SerializedType = "LOOP END";

    public override bool Execute(RobotController robot)
    {
        SetInstructionToMatchingLoopStart();

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

    private void SetInstructionToMatchingLoopStart()
    {
        //int skippingLoopStarts = 0;
        //for (int i = nextInstructionIndex - 1; i >= 0; i--)
        //{
        //    if (instructions[i] == Instructions.LoopEnd)
        //        skippingLoopStarts++;
        //    else if (Instructions.IsValidLoopStart(instructions[i]))
        //    {
        //        if (skippingLoopStarts == 0)
        //        {
        //            if (Instructions.IsLoopStartCompleted(instructions[i]))
        //                return;
        //            else
        //            {
        //                nextInstructionIndex = i - 1;
        //                return;
        //            }
        //        }
        //        else
        //            skippingLoopStarts--;
        //    }
        //}

        //SetFeedbackIfNotPreview("COULD NOT FIND MATCHING LOOP START");
    }
}