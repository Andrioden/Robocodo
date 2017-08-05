using System;
using System.Collections.Generic;
using System.Linq;

public class TechnologiesDB
{
    public static List<Technology> Technologies(TechnologyTree techTree)
    {
        int techIdIterator = 0;

        return new List<Technology>()
        {
            new Technology_Robot(techTree, techIdIterator++, "Harvester", "Enables production of the Harvester robot which can be used to gather resources such as food, copper and iron.", 100, typeof(HarvesterRobotController)),
            new Technology_Robot(techTree, techIdIterator++, "Predator", "Enables production of the Predator robot, a formidable combat robot used to eliminate our enemies.", 500, typeof(CombatRobotController)),
            new Technology_Robot(techTree, techIdIterator++, "Transporter", "Enables production of the Transporter robot which can transport resources from Harvester robots back to the city, enabling Harvesters greater efficiency when gathering resources far from home.", 1000, typeof(TransporterRobotController)),
            new Technology_Robot(techTree, techIdIterator++, "Storage", "Enables production of remote Storage robots to enable resouce stockpiling away from home.", 1000, typeof(StorageRobotController)),
            new Technology_Robot(techTree, techIdIterator++, "Purger", "Enables production of the vitally imporant Purger robot which has the power to eleminate the virus DX completely from an area. Our greatest tool to fight the spreading.", 1000, typeof(PurgeRobotController)),
            new Technology_Robot(techTree, techIdIterator++, "Battery", "Enables production of the Battery robot which is basically a huge mobile battery for recharging robots out in the field.", 2000, typeof(BatteryRobotController)),
            new Technology_Victory(techTree, techIdIterator++, "DX Vaccine", "This is the cure for the virus DX. Once this is researched we will be able to eliminate DX for good and return to the surface once more (Victory).", 1000000)
        };
    }
}

