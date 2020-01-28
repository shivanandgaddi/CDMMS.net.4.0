using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CenturyLink.Network.Engineering.Material.Editor.Models.Template;

namespace UnitTestMaterialEditor
{
    [TestClass]
    public class ScaleTest
    {
        [TestMethod]
        public void TestScale()
        {
            Scale scale = new Scale(84M, 25M, "in");

            Assert.IsTrue(scale.Left > 0);
        }
    }
}
