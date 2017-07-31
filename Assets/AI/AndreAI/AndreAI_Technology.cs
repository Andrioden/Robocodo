using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Robocodo.AndreAI
{
    public partial class AndreAI : AI
    {

        private bool Seek_VictoryByTech()
        {
            if (!Condition_NoCurrentActiveTeching())
                return false;

            return Seek_Tech(player.TechTree.Technologies.Where(t => t.name == "DX Vaccine").First());
        }

        private bool Condition_NoCurrentActiveTeching()
        {
            return player.TechTree.activeResearch == null;
        }

        private bool Seek_RobotTech<T>() where T : RobotController
        {
            Technology robotTech = player.TechTree.Technologies
                .Where(t => t is Technology_Robot)
                .Select(t => (Technology_Robot)t)
                .First(t => t.robotType == typeof(T));

            return Seek_Tech(robotTech);
        }

        private bool Seek_Tech(Technology tech)
        {
            if (Has_Tech(tech))
                return true;

            return Do_Tech(tech);
        }

        private bool Has_Tech(Technology tech)
        {
            return tech.IsResearched();
        }

        private bool Do_Tech(Technology tech)
        {
            player.TechTree.SetOrPauseActiveResearch(tech);

            return tech.IsResearched();
        }

    }
}
