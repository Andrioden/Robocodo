﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Robocodo.AndreAI
{
    /// <summary>
    /// This AI class follows a SEEK-DO pattern:
    /// 
    ///  ---------- SUMMARY ----------
    /// SEEK
    ///     HAS
    ///     
    ///     CONDITION CHECK 1
    ///     CONDITION CHECK 2...
    ///     
    ///     SEEK 1 (subseek)
    ///     SEEK 2 (subseek)...
    ///     
    ///     DO
    ///     
    ///     SEEK (recursive)
    ///  
    /// 
    ///  ---------- DETAILS ----------
    /// Has:
    ///     - First we always check if the AI does not already have what it seeks
    /// 
    /// 
    /// Seek: 
    ///     - Something the AI wants to gain
    ///     - Returns true if it managed to Seek it
    ///     - Can be trigger within a Seek (recursively)
    ///     
    /// Condition: 
    ///     - Something that stops the robot from doing the action to get what the AI Seeks.
    ///     - Returns true if the condition is fulfilled
    /// 
    /// Do:
    ///     - An actual real world changing action that the AI does to get what he Seeks.
    ///         
    /// </summary>
    public partial class AndreAI : AI
    {
        public static int Setting_DelayedStart_Min = 2; // Should not be lower than 2, since the game need to initialize a bit before the AI starts. Typical setting Owner on objects.
        public static int Setting_DelayedStart_Max = 3;
        public static int Setting_ThinkingInterval = 2;

        private ActiveHarvestersTracker activeHarvestersTracker;


        protected override void StartAI()
        {
            activeHarvestersTracker = new ActiveHarvestersTracker();

            float delayedStart = Utils.RandomFloat(Setting_DelayedStart_Min, Setting_DelayedStart_Max);
            StartCoroutine(ThinkCoroutine(delayedStart));
        }

        private IEnumerator ThinkCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            while (true)
            {
                Think();
                yield return new WaitForSeconds(Setting_ThinkingInterval);
            }
        }

        private void Think()
        {
            if (Settings.Debug_EnableAiLogging)
                HumanCommunicator.ShowPopupForAllHumans("Thinking... " + Icons.Heart, player.transform.position, TextPopup.ColorType.DEFAULT);

            Seek_ActiveHarvesters<FoodController>(4, 30);
            Seek_ActiveHarvesters<IronController>(2, 30);
            Seek_ActiveHarvesters<CopperController>(2, 30);

            Do_ReprogramCompletedHarvesters();
        }

        private bool Do_ReprogramCompletedHarvesters()
        {
            activeHarvestersTracker.ReprogramCompletedHarvesters();
            return true;
        }

        private bool Seek_ActiveHarvesters<T>(int count, float searchRadius) where T : ResourceController
        {
            LogFormat("Seek_ActiveHarvesters<{0}>({1}): ", typeof(T), count);

            if (Has_ActiveHarvesters<T>(count))
                return true;

            if (!Seek_IdleRobot<HarvesterRobotController>(HarvesterRobotController.Settings_cost()))
                return false;

            Do_HarvestNearby<T>(searchRadius);

            return Seek_ActiveHarvesters<T>(count, searchRadius);
        }

        private bool Has_ActiveHarvesters<T>(int count) where T : ResourceController
        {
            bool b = activeHarvestersTracker.OfResourceTypeCount<T>() >= count;

            LogFormat("Has_ActiveHarvesters<{0}>({1}): {2}", typeof(T), count, b);

            return b;
        }

        private bool Seek_IdleRobot<T>(Cost cost) where T : RobotController
        {
            LogFormat("Seek_IdleRobot<{0}>({1}): {2}", typeof(T), cost, typeof(T));

            if (Has_IdleRobot<T>())
                return true;

            if (!Condition_CanAfford(cost))
                return false;

            if (!Seek_RobotTech<T>())
                return false;

            return Do_BuyRobot<T>();
        }

        private bool Has_IdleRobot<T>() where T : RobotController
        {
            bool b = GetOwnedRobot<T>(false).Count > 0;

            LogFormat("Has_IdleRobot<{0}>(): {1}", typeof(T), b);

            return b;
        }

        private bool Condition_CanAfford(Cost cost)
        {
            bool b = player.City.CanAfford(cost);

            LogFormat("Condition_CanAfford({0}): {1}", cost, b);

            return b;
        }

        private bool Seek_RobotTech<T>() where T : RobotController
        {
            if (Has_RobotTech<T>())
                return true;

            return Do_RobotTech<T>();
        }

        private bool Has_RobotTech<T>() where T : RobotController
        {
            return player.TechTree.HasRobotTech(typeof(T));
        }

        private bool Do_RobotTech<T>() where T : RobotController
        {
            Technology_Robot robotTech = player.TechTree.technologies
                .Where(t => t is Technology_Robot)
                .Select(t => (Technology_Robot)t)
                .First(t => t.robotType == typeof(T));

            player.TechTree.SetActiveResearch(robotTech);

            return robotTech.IsResearched();
        }

        private bool Do_BuyRobot<T>() where T : RobotController
        {
            LogFormat("Do_BuyRobot<{0}>()", typeof(T));

            if (typeof(T) == typeof(HarvesterRobotController))
                player.City.CmdBuyHarvesterRobot();
            else if (typeof(T) == typeof(CombatRobotController))
                player.City.CmdBuyCombatRobot();
            else if (typeof(T) == typeof(TransporterRobotController))
                player.City.CmdBuyTransporterRobot();
            else if (typeof(T) == typeof(StorageRobotController))
                player.City.CmdBuyStorageRobot();
            else if (typeof(T) == typeof(PurgeRobotController))
                player.City.CmdBuyPurgeRobot();
            else
                throw new Exception("Not added support to AI buy " + typeof(T));

            return true;
        }

        private bool Do_HarvestNearby<T>(float searchRadius) where T : ResourceController
        {
            LogFormat("Do_HarvestNearby<{0}>()", typeof(T));

            //Log("   Looking for idle harvester...");
            HarvesterRobotController harvester = GetOwnedRobot<HarvesterRobotController>(false).FirstOrDefault();

            if (harvester != null)
            {
                //Log("   ... Harvester found!");

                //Log("   Looking for nearby resource...");
                ResourceController nearByResource = harvester.FindNearbyCollidingGameObjectsOfType<T>(searchRadius).Take(4).TakeRandom();

                if (nearByResource != null)
                {
                    //Log("   ... Resource found!");

                    //Log("   Programming robot and starting it...");
                    DateTime timeStart = DateTime.Now;
                    List<Instruction> instructions = FindPathInstructions(harvester.gameObject, nearByResource.gameObject);
                    instructions.Add(new Instruction_Harvest());
                    instructions.Add(new Instruction_Harvest());
                    instructions.Add(new Instruction_Move(MoveDirection.Home));
                    instructions.Add(new Instruction_DropInventory());
                    harvester.SetInstructions(instructions);
                    harvester.CmdStartRobot();
                    DateTime timeEnd = DateTime.Now;
                    int msUsed = (timeEnd - timeStart).Milliseconds;
                    LogFormat("   ... Robot pathfound and programmed in in {0}ms!", msUsed);

                    activeHarvestersTracker.Add(harvester, nearByResource);

                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

    }

}