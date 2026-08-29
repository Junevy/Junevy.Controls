using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Junevy.Controls.Common;

namespace Junevy.Controls.Controls.Toolbox;

internal readonly struct ToolboxPopupPlacementCandidate
{
    internal ToolboxPopupPlacementCandidate(
        ToolboxPopupPlacement direction,
        CustomPopupPlacement placement)
    {
        Direction = direction;
        Placement = placement;
    }

    internal ToolboxPopupPlacement Direction { get; }

    internal CustomPopupPlacement Placement { get; }
}

internal static class ToolboxPopupPlacementCalculator
{
    internal static ToolboxPopupPlacementCandidate[] GetPlacements(
        Size popupSize,
        Size targetSize,
        Point offset,
        Orientation orientation,
        ToolboxPopupPlacement preference)
    {
        ToolboxPopupPlacement[] directions = GetDirections(orientation, preference);
        var placements = new ToolboxPopupPlacementCandidate[directions.Length];

        for (int index = 0; index < directions.Length; index++)
        {
            ToolboxPopupPlacement direction = directions[index];
            Point point = direction switch
            {
                ToolboxPopupPlacement.Right => new Point(targetSize.Width + offset.X, offset.Y),
                ToolboxPopupPlacement.Left => new Point(-popupSize.Width + offset.X, offset.Y),
                ToolboxPopupPlacement.Bottom => new Point(offset.X, targetSize.Height + offset.Y),
                ToolboxPopupPlacement.Top => new Point(offset.X, -popupSize.Height + offset.Y),
                _ => throw new ArgumentOutOfRangeException(nameof(preference))
            };

            placements[index] = new ToolboxPopupPlacementCandidate(
                direction,
                new CustomPopupPlacement(point, PopupPrimaryAxis.None));
        }

        return placements;
    }

    private static ToolboxPopupPlacement[] GetDirections(
        Orientation orientation,
        ToolboxPopupPlacement preference)
    {
        ToolboxPopupPlacement[] automatic = orientation == Orientation.Vertical
            ? new[]
            {
                ToolboxPopupPlacement.Right,
                ToolboxPopupPlacement.Left,
                ToolboxPopupPlacement.Bottom,
                ToolboxPopupPlacement.Top
            }
            : new[]
            {
                ToolboxPopupPlacement.Bottom,
                ToolboxPopupPlacement.Top,
                ToolboxPopupPlacement.Right,
                ToolboxPopupPlacement.Left
            };

        if (preference == ToolboxPopupPlacement.Auto)
        {
            return automatic;
        }

        var ordered = new ToolboxPopupPlacement[automatic.Length];
        ordered[0] = preference;
        int destinationIndex = 1;
        foreach (ToolboxPopupPlacement direction in automatic)
        {
            if (direction != preference)
            {
                ordered[destinationIndex++] = direction;
            }
        }

        return ordered;
    }
}
