using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Instruction_Attack : Instruction
{

    public override int Setting_EnergyCost() { return 1; }
    public override bool Setting_Still() { return false; }
    public override bool Setting_ConsumesTick() { return true; }
    public override bool CanBePreviewed() { return false; }

    public static readonly string Format = "ATTACK [DIRECTION]";

    private RobotController robot;
    public AttackType direction;

    public Instruction_Attack(AttackType direction)
    {
        this.direction = direction;
    }

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;
        AttackInDirection(direction);
        return true;
    }

    public override string Serialize()
    {
        return Format.Replace("[DIRECTION]", direction.ToString().ToUpper());
    }

    public static Instruction Deserialize(string instruction)
    {
        string directionString = instruction.Replace("ATTACK ", "");
        AttackType direction = Utils.ParseEnum<AttackType>(directionString);
        return new Instruction_Attack(direction);
    }

    public static bool IsValid(string instruction)
    {
        string direction = instruction.Replace("ATTACK ", "");
        return Utils.IsStringValueInEnum<AttackType>(direction);
    }

    private void AttackInDirection(AttackType type)
    {
        if (type == AttackType.Nearby3)
            AttackNearby(3);
        else
            throw new Exception("AttackType not supported " + type);
    }

    //private void AttackPosition(float x, float z)
    //{
    //    IAttackable attackable = FindAttackableEnemy((int)x, (int)z);
    //    if (attackable != null)
    //        attackable.TakeDamage(robot.Settings_Damage());
    //    else
    //        robot.SetFeedbackIfNotPreview("NO TARGET TO ATTACK", false, true);
    //}

    private void AttackNearby(int maxDistance)
    {
        IAttackable attackable = FindNearbyAttackableEnemy(maxDistance);
        if (attackable != null)
        {
            robot.lastAttackedTargetWasAnHit = true;
            robot.lastAttackedTargetX = attackable.GetX();
            robot.lastAttackedTargetZ = attackable.GetZ();
            attackable.TakeDamage(robot.Settings_Damage());
        }
        else
        {
            robot.lastAttackedTargetWasAnHit = false;
            robot.lastAttackedTargetX = -9999;
            robot.lastAttackedTargetZ = -9999;
            robot.SetFeedback("NO TARGET TO ATTACK", false, true);
        }
    }

    //private IAttackable FindAttackableEnemy(int x, int z)
    //{
    //    foreach (IAttackable potentialTarget in robot.FindNearbyAttackableTargets())
    //    {
    //        if (potentialTarget.X() == x && potentialTarget.Z() == z)
    //        {
    //            if (potentialTarget.GetOwner() != robot.GetOwner())
    //                return potentialTarget;
    //        }
    //    }

    //    //Debug.Log("Did not find attackable");
    //    return null;
    //}

    private IAttackable FindNearbyAttackableEnemy(int maxDistance)
    {
        foreach (IAttackable potentialTarget in robot.FindNearbyAttackableTargets())
        {
            if (MathUtils.Distance(robot.GetX(), robot.GetZ(), potentialTarget.GetX(), potentialTarget.GetZ()) <= maxDistance)
            {
                if (potentialTarget.GetOwner() != robot.GetOwner())
                    return potentialTarget;
            }
        }

        Debug.Log("Did not find attackable");
        return null;
    }

}

public enum AttackType
{
    Nearby3
}