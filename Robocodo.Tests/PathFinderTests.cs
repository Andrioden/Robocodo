using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EpPathFinding.cs;

namespace Robocodo.Tests
{
    [TestClass]
    public class PathFinderTests
    {
        [TestMethod]
        public void FindPathTestPlaygroundTest()
        {
            int width = 20;
            int height = 20;

            int fromX = 10;
            int fromZ = 10;
            int toX = 11;
            int toZ = 12;

            BaseGrid searchGrid = new StaticGrid(width, height);

            for (int x = 0; x < width; x++)
                for (int z = 0; z < height; z++)
                    searchGrid.SetWalkableAt(x, z, true);

            JumpPointParam jpParam = new JumpPointParam(searchGrid, false, false, false);

            GridPos from = new GridPos(fromX, fromZ);
            GridPos to = new GridPos(toX, toZ);
            jpParam.Reset(from, to);

            List<GridPos> routeFound = JumpPointFinder.FindPath(jpParam);
            var fullPath = JumpPointFinder.GetFullPath(routeFound);
        }

    }
}