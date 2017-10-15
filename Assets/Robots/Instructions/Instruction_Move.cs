using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Instruction_Move : Instruction
{

    public override int Setting_EnergyCost() { return 1; }
    public override bool Setting_Still() { return false; }
    public override bool Setting_ConsumesTick() { return true; }
    public override bool Setting_AllowStacking() { return false; }
    public override PreviewImage Setting_PreviewImage() { return new PreviewImage { Name = "ArrowRight", VerticalAlign = VerticalAlign.Bottom }; }

    public static readonly string Format = "MOVE [DIRECTION]";

    private RobotController robot;
    public MoveDirection direction;

    public Instruction_Move(MoveDirection direction)
    {
        this.direction = direction;
    }

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;
        return Move(direction);
    }

    public override bool CanBeExecutedForPreviewRobot()
    {
        if (direction == MoveDirection.Random)
            return false;
        else
            return true;
    }

    public override string Serialize()
    {
        return Format.Replace("[DIRECTION]", direction.ToString().ToUpper());
    }

    public static Instruction Deserialize(string instruction)
    {
        string directionString = instruction.Replace("MOVE ", "");
        MoveDirection direction = Utils.ParseEnum<MoveDirection>(directionString);
        return new Instruction_Move(direction);
    }

    public static bool IsValid(string instruction)
    {
        string direction = instruction.Replace("MOVE ", "");
        return Utils.IsStringValueInEnum<MoveDirection>(direction);
    }

    private bool Move(MoveDirection direction)
    {
        if (IsBlockedByOtherRobots())
        {
            robot.SetFeedback("WAITING FOR OTHER ROBOT", true, false);
            return false;
        }
        else
        {
            SetRobotMovementOnCurrentCordIfNotPreview();

            bool instructionCompleted = false;

            if (direction == MoveDirection.Home)
                instructionCompleted = MoveHome();
            else
            {
                if (!robot.Move(direction))
                    robot.SetFeedback("CAN NOT MOVE THERE", true, false);
                instructionCompleted = true;
            }

            SetRobotMovementOnCurrentCordIfNotPreview();

            return instructionCompleted;
        }
    }

    private bool MoveHome()
    {
        if (robot.Owner == null)
            throw new Exception("Robot has no owner.");
        else if (!robot.IsAtPlayerCity())
        {
            SetRobotMovementOnCurrentCordIfNotPreview();
            robot.MoveTowards(robot.Owner.City.X, robot.Owner.City.Z);
            SetRobotMovementOnCurrentCordIfNotPreview();

            return false;
        }
        else
            return true;
    }

    private bool IsBlockedByOtherRobots()
    {
        if (robot.isPreviewRobot || robot.IsAtPlayerCity())
            return false;

        RobotTick robotMovement = WorldController.instance.robotMovements[robot.X, robot.Z];
        return robotMovement != null && robotMovement.Robot != robot && robotMovement.Tick == WorldTickController.instance.Tick;
    }

    private void SetRobotMovementOnCurrentCordIfNotPreview()
    {
        if (!robot.isPreviewRobot)
            WorldController.instance.robotMovements[robot.X, robot.Z] = new RobotTick(robot, WorldTickController.instance.Tick);
    }

}