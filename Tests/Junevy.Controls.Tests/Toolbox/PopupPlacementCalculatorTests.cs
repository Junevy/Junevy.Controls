using System.Windows;
using System.Windows.Controls;
using Junevy.Controls.Common;
using Junevy.Controls.Controls.Toolbox;
using NUnit.Framework;

namespace Junevy.Controls.Tests.Toolbox;

[TestFixture]
public sealed class PopupPlacementCalculatorTests
{
    [Test]
    public void AutoPlacement_UsesOrientationSpecificPriority()
    {
        ToolboxPopupPlacementCandidate[] vertical = Calculate(Orientation.Vertical);
        ToolboxPopupPlacementCandidate[] horizontal = Calculate(Orientation.Horizontal);

        Assert.Multiple(() =>
        {
            Assert.That(vertical.Select(candidate => candidate.Direction), Is.EqualTo(new[]
            {
                ToolboxPopupPlacement.Right,
                ToolboxPopupPlacement.Left,
                ToolboxPopupPlacement.Bottom,
                ToolboxPopupPlacement.Top
            }));
            Assert.That(horizontal.Select(candidate => candidate.Direction), Is.EqualTo(new[]
            {
                ToolboxPopupPlacement.Bottom,
                ToolboxPopupPlacement.Top,
                ToolboxPopupPlacement.Right,
                ToolboxPopupPlacement.Left
            }));
        });
    }

    [Test]
    public void PlacementCoordinates_UseTargetAndPopupDipSizes()
    {
        ToolboxPopupPlacementCandidate[] placements = Calculate(Orientation.Vertical);

        Assert.Multiple(() =>
        {
            Assert.That(placements[0].Placement.Point, Is.EqualTo(new Point(40d, 0d)));
            Assert.That(placements[1].Placement.Point, Is.EqualTo(new Point(-300d, 0d)));
            Assert.That(placements[2].Placement.Point, Is.EqualTo(new Point(0d, 40d)));
            Assert.That(placements[3].Placement.Point, Is.EqualTo(new Point(0d, -200d)));
        });
    }

    [Test]
    public void ExplicitPlacement_IsFirstAndRetainsEveryFallbackOnce()
    {
        ToolboxPopupPlacementCandidate[] placements = ToolboxPopupPlacementCalculator.GetPlacements(
            new Size(300d, 200d),
            new Size(40d, 40d),
            new Point(),
            Orientation.Vertical,
            ToolboxPopupPlacement.Left);

        Assert.Multiple(() =>
        {
            Assert.That(placements[0].Direction, Is.EqualTo(ToolboxPopupPlacement.Left));
            Assert.That(placements.Select(candidate => candidate.Direction), Is.EquivalentTo(new[]
            {
                ToolboxPopupPlacement.Right,
                ToolboxPopupPlacement.Left,
                ToolboxPopupPlacement.Bottom,
                ToolboxPopupPlacement.Top
            }));
        });
    }

    [Test]
    public void PlacementCoordinates_IncludeRequestedDipOffset()
    {
        ToolboxPopupPlacementCandidate[] placements = ToolboxPopupPlacementCalculator.GetPlacements(
            new Size(300d, 200d),
            new Size(40d, 40d),
            new Point(3d, 5d),
            Orientation.Horizontal,
            ToolboxPopupPlacement.Auto);

        Assert.Multiple(() =>
        {
            Assert.That(placements[0].Placement.Point, Is.EqualTo(new Point(3d, 45d)));
            Assert.That(placements[1].Placement.Point, Is.EqualTo(new Point(3d, -195d)));
            Assert.That(placements[2].Placement.Point, Is.EqualTo(new Point(43d, 5d)));
            Assert.That(placements[3].Placement.Point, Is.EqualTo(new Point(-297d, 5d)));
        });
    }

    private static ToolboxPopupPlacementCandidate[] Calculate(Orientation orientation)
    {
        return ToolboxPopupPlacementCalculator.GetPlacements(
            new Size(300d, 200d),
            new Size(40d, 40d),
            new Point(),
            orientation,
            ToolboxPopupPlacement.Auto);
    }
}
