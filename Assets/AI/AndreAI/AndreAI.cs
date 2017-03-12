using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Robocodo.AndreAI
{
    public class AndreAI : AI
    {

        //private static float PlanningInterval()

        protected override void StartAI()
        {
            Log("Looking for harvester...");
            HarvesterRobotController harvester = GetOwned<HarvesterRobotController>().FirstOrDefault();

            if (harvester != null)
            {
                Log("... Harvester found!");

                Log("Looking for nearby food...");
                FoodController nearByFood = harvester.FindNearbyCollidingGameObjectsOfType<FoodController>(40).FirstOrDefault();

                if (nearByFood != null)
                {
                    Log("... Food found!");
                    List<Instruction> instructions = FindPathInstructions(harvester.gameObject, nearByFood.gameObject);
                    instructions.Add(new Instruction_Harvest());
                    instructions.Add(new Instruction_Harvest());
                    instructions.Add(new Instruction_Move(MoveDirection.Home));
                    instructions.Add(new Instruction_DropInventory());
                    harvester.SetInstructions(instructions);
                    harvester.CmdStartRobot();
                }
            }

        }

    }
}