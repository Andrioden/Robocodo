using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_Move : Instruction
{

    public static readonly string SerializedType = "IDLE";

    private RobotController robot;
    public MoveDirection direction;

    public Instruction_Move(MoveDirection direction)
    {
        this.direction = direction;
    }

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;
        return MoveInDirection(direction);
    }

    public override bool CanBePreviewed()
    {
        if (direction == MoveDirection.Random)
            return false;
        else
            return true;
    }

    public override string Serialize()
    {
        return SerializedType + " " + GetDirectionString();
    }

    private bool MoveInDirection(MoveDirection direction)
    {
        if (direction == MoveDirection.Up)
            ChangePosition(robot.x, robot.z + 1);
        else if (direction == MoveDirection.Down)
            ChangePosition(robot.x, robot.z - 1);
        else if (direction == MoveDirection.Right)
            ChangePosition(robot.x + 1, robot.z);
        else if (direction == MoveDirection.Left)
            ChangePosition(robot.x - 1, robot.z);
        else if (direction == MoveDirection.Home)
            return MoveHome();
        else if (direction == MoveDirection.Random)
        {
            MoveDirection randomDirection = Utils.Random(new List<MoveDirection>
            {
                MoveDirection.Up,
                MoveDirection.Down,
                MoveDirection.Right,
                MoveDirection.Left
            });
            MoveInDirection(randomDirection);
        }

        return true;
    }

    private bool MoveHome()
    {
        if (robot.PlayerCityController == null)
            throw new Exception("Robot has no playerCityController.");

        SanityCheckIfPositionNumbersAreWhole();

        float difX = Math.Abs(robot.x - robot.PlayerCityController.X);
        float difZ = Math.Abs(robot.z - robot.PlayerCityController.Z);

        if (difX >= difZ && !robot.IsAtPlayerCity())
            robot.x += GetIncremementOrDecrementToGetCloser(robot.x, robot.PlayerCityController.X);
        else if (difX < difZ)
            robot.z += GetIncremementOrDecrementToGetCloser(robot.z, robot.PlayerCityController.Z);

        if (!robot.IsAtPlayerCity())
            return false;

        return true;
    }

    private void ChangePosition(float newPosX, float newPosZ)
    {
        if (newPosX >= WorldController.instance.Width || newPosX < 0 || newPosZ >= WorldController.instance.Height || newPosZ < 0)
            robot.SetFeedbackIfNotPreview("CAN NOT MOVE THERE");
        else
        {
            robot.x = newPosX;
            robot.z = newPosZ;
        }
    }

    private int GetIncremementOrDecrementToGetCloser(float posValue, float homeValue)
    {
        if (posValue > homeValue)
            return -1;
        else if (posValue < homeValue)
            return 1;
        else
            throw new Exception("Should not call this method withot a value difference");
    }

    private void SanityCheckIfPositionNumbersAreWhole()
    {
        SanityCheckIsWholeNumber("position X", robot.x);
        SanityCheckIsWholeNumber("position Z", robot.z);
        SanityCheckIsWholeNumber("home X", robot.PlayerCityController.X);
        SanityCheckIsWholeNumber("home Z", robot.PlayerCityController.Z);
    }

    private void SanityCheckIsWholeNumber(string friendlyName, float number)
    {
        if ((number % 1) != 0)
            throw new Exception("Robot " + friendlyName + " is not a whole number");
    }

    private string GetDirectionString()
    {
        if (direction == MoveDirection.Up)
            return "UP";
        else if (direction == MoveDirection.Down)
            return "DOWN";
        else if (direction == MoveDirection.Left)
            return "LEFT";
        else if (direction == MoveDirection.Right)
            return "RIGHT";
        else if (direction == MoveDirection.Home)
            return "HOME";
        else if (direction == MoveDirection.Random)
            return "RANDOM";
        else
            throw new Exception("Unsupported MoveDirection: " + direction);
    }
}

public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right,
    Home,
    Random
}