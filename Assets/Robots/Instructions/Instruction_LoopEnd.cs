using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_LoopEnd : Instruction
{

    public static readonly string Format = "LOOP END";

    private RobotController robot;

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;
        SetInstructionToMatchingLoopStart();
        return true;
    }

    public override bool CanBePreviewed()
    {
        return true;
    }

    public override string Serialize()
    {
        return Format;
    }

    private void SetInstructionToMatchingLoopStart()
    {
        int skippingLoopStarts = 0;
        for (int i = robot.nextInstructionIndex - 1; i >= 0; i--)
        {
            if (robot.Instructions[i].GetType() == typeof(Instruction_LoopEnd))
                skippingLoopStarts++;
            else if (robot.Instructions[i].GetType() == typeof(Instruction_LoopStart))
            {
                if (skippingLoopStarts == 0)
                {
                    Instruction_LoopStart loopStartInstruction = (Instruction_LoopStart)robot.Instructions[i];
                    if (loopStartInstruction.IsIterationsCompleted())
                        return;
                    else
                    {
                        robot.nextInstructionIndex = i - 1;
                        return;
                    }
                }
                else
                    skippingLoopStarts--;
            }
        }

        robot.SetFeedbackIfNotPreview("COULD NOT FIND MATCHING LOOP START");
    }
}