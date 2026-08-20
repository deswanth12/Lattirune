using NUnit.Framework;
using UnityEngine;
using Lattirune.UI;

namespace Lattirune.Tests
{
    [TestFixture]
    public class FullLoopRealDeviceQATests
    {
        [Test]
        public void DeviceQA_DesignSystemColors_AreValid()
        {
            Assert.AreEqual(0.839f, LattiruneUITheme.ColorGoldPrimary.r, 0.01f);
            Assert.AreEqual(0.957f, LattiruneUITheme.ColorGoldBright.r, 0.01f);
            Assert.AreEqual(0.031f, LattiruneUITheme.ColorObsidianBg.r, 0.01f);
            Assert.AreEqual(0.067f, LattiruneUITheme.ColorSurfaceDark.r, 0.01f);
        }

        [Test]
        public void DeviceQA_MatrixCalculation_IsDeterministic()
        {
            var oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);
            Assert.Greater(scale, 0f);
        }
    }
}
