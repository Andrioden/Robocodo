using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Robocodo.AndreAI
{
    public partial class AndreAI : AI
    {

        private ActiveHarvestersTracker activeHarvestersTracker;

        private bool Seek_ActiveHarvesters<T>(int count, float searchRadius) where T : ResourceController
        {
            LogFormat("Seek_ActiveHarvesters<{0}>({1}): ", typeof(T), count);

            if (Has_ActiveHarvesters<T>(count))
                return true;

            if (!Seek_IdleRobot<HarvesterRobotController>(HarvesterRobotController.Settings_cost()))
                return false;

            if (!Do_HarvestNearby<T>(searchRadius))
                return false;

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

            HarvesterRobotController harvester = GetOwnedRobot<HarvesterRobotController>(false).FirstOrDefault();

            if (harvester == null)
                return false;

            ResourceController nearByResource = harvester.FindNearbyCollidingGameObjectsOfType<T>(searchRadius).Take(4).TakeRandom();

            if (nearByResource == null)
                return false;

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

    }
}
