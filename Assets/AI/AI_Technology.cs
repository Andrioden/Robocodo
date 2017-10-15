using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Robocodo.AI
{


    public abstract partial class AI : MonoBehaviour
    {
        protected bool Seek_VictoryByTech()
        {
            if (!Condition_NoCurrentActiveTeching())
                return false;

            return Seek_Tech(player.TechTree.Technologies.Where(t => t.name == "DX Vaccine").First());
        }

        protected bool Condition_NoCurrentActiveTeching()
        {
            return player.TechTree.activeResearch == null;
        }

        protected bool Seek_RobotTech<T>() where T : RobotController
        {
            Technology robotTech = player.TechTree.Technologies
                .Where(t => t is Technology_Robot)
                .Select(t => (Technology_Robot)t)
                .First(t => t.robotType == typeof(T));

            return Seek_Tech(robotTech);
        }

        protected bool Seek_Tech(Technology tech)
        {
            if (Has_Tech(tech))
                return true;

            return Do_Tech(tech);
        }

        protected bool Has_Tech(Technology tech)
        {
            return tech.IsResearched();
        }

        protected bool Do_Tech(Technology tech)
        {
            player.TechTree.SetOrPauseActiveResearch(tech);

            return tech.IsResearched();
        }
    }

}