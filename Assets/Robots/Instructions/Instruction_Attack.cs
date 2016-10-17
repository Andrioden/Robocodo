using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Instruction_Attack : Instruction
{

    public static readonly string Format = "ATTACK [DIRECTION]";

    private RobotController robot;
    public AttackDirection direction;

    public Instruction_Attack(AttackDirection direction)
    {
        this.direction = direction;
    }

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;
        AttackInDirection(direction);
        return true;
    }

    public override bool CanBePreviewed()
    {
        return false;
    }

    public override string Serialize()
    {
        return Format.Replace("[DIRECTION]", direction.ToString().ToUpper());
    }

    public static Instruction Deserialize(string instruction)
    {
        string directionString = instruction.Replace("ATTACK ", "");
        AttackDirection direction = Utils.ParseEnum<AttackDirection>(directionString);
        return new Instruction_Attack(direction);
    }

    public static bool IsValid(string instruction)
    {
        string direction = instruction.Replace("ATTACK ", "");
        return Utils.IsStringValueInEnum<AttackDirection>(direction);
    }

    private void AttackInDirection(AttackDirection direction)
    {
        if (direction == AttackDirection.Melee)
            AttackPosition(robot.x, robot.z);
        else if (direction == AttackDirection.Up)
            AttackPosition(robot.x, robot.z + 1);
        else if (direction == AttackDirection.Down)
            AttackPosition(robot.x, robot.z - 1);
        else if (direction == AttackDirection.Right)
            AttackPosition(robot.x + 1, robot.z);
        else if (direction == AttackDirection.Left)
            AttackPosition(robot.x - 1, robot.z);
        else if (direction == AttackDirection.Random)
        {
            AttackDirection randomDirection = Utils.Random(new List<AttackDirection>
            {
                AttackDirection.Melee,
                AttackDirection.Up,
                AttackDirection.Down,
                AttackDirection.Right,
                AttackDirection.Left
            });
            AttackInDirection(randomDirection);
        }
    }

    private void AttackPosition(float x, float z)
    {
        IAttackable attackable = FindAttackableEnemy((int)x, (int)z);
        if (attackable != null)
            attackable.TakeDamage(robot.Settings_Damage());
        else
            robot.SetFeedbackIfNotPreview("NO TARGET TO ATTACK");
    }

    private IAttackable FindAttackableEnemy(int x, int z)
    {
        foreach (GameObject potentialGO in robot.FindNearbyCollidingGameObjects())
        {
            IAttackable attackable = potentialGO.transform.root.GetComponent<IAttackable>();

            if (attackable != null && potentialGO.transform.position.x == x && potentialGO.transform.position.z == z)
            {
                if (attackable.GetOwner() != robot.GetOwner())
                    return attackable;
            }
        }

        //Debug.Log("Did not find attackable");
        return null;
    }

}

public enum AttackDirection
{
    Melee,
    Up,
    Down,
    Left,
    Right,
    Random
}