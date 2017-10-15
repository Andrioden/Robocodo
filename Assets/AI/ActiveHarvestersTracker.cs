using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Robocodo.AI
{

    public class ActiveHarvestersTracker
    {

        List<ActiveHarvester> activeHarvesters = new List<ActiveHarvester>();

        public void Add(HarvesterRobotController harvester, ResourceController resource)
        {
            activeHarvesters.Add(new ActiveHarvester(harvester, resource));
        }

        public int OfResourceTypeCount<T>()
        {
            CleanupList();

            return activeHarvesters.Count(h => h.Resource is T);
        }

        public void ReprogramCompletedHarvesters()
        {
            CleanupList();

            foreach (var ah in activeHarvesters)
                if ((ah.Resource == null || ah.Resource.RemainingItems == 0) && ah.Harvester.Inventory.Count == 0)
                    if (!ah.Harvester.WillReprogramWhenHome)
                        ah.Harvester.CmdToggleReprogramWhenHome();
        }

        private void CleanupList()
        {
            activeHarvesters.RemoveAll(h => h.Harvester == null || !h.Harvester.IsStarted);
        }

    }

    public class ActiveHarvester
    {
        public HarvesterRobotController Harvester;
        public ResourceController Resource;

        public ActiveHarvester(HarvesterRobotController harvester, ResourceController resource)
        {
            Harvester = harvester;
            Resource = resource;
        }
    }

}