using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class Instruction
{
    /// <summary>
    /// Execute instruction for a robot
    /// </summary>
    /// <returns>True - if the Instruction is fully completed</returns>
    public abstract bool Execute(RobotController robot);
    public abstract bool CanBePreviewed();

    public abstract string Serialize();
}