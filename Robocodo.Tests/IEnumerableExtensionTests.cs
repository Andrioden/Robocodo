using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robocodo.Tests
{
    [TestClass]
    public class IEnumerableExtensionTests
    {
        [TestMethod]
        public void FindAllIndexesTest()
        {
            List<int> numbers = new List<int>() { 5, 6, 5, 7, 8, 5 };

            CollectionAssert.AreEqual(new List<int>() { 0, 2, 5 }, numbers.FindAllIndexes(n => n == 5).ToList());
        }
    }
}