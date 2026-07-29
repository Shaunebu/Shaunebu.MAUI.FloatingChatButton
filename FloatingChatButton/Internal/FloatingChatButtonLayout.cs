namespace FloatingChatButton.Internal;

internal readonly record struct FloatingChatButtonBounds(double X, double Y, double Width, double Height);

internal static class FloatingChatButtonLayout
{
    internal const double CollapsedSize = 60;
    internal const double ExpandedWidthPercentage = 0.8;
    internal const double ExpandedHeightPercentage = 0.7;
    internal const double MaxExpandedWidth = 400;
    internal const double MaxExpandedHeight = 600;
    internal const double EdgePadding = 20;

    internal static bool IsUsableSize(double width, double height)
    {
        return double.IsFinite(width) && double.IsFinite(height) && width > 0 && height > 0;
    }

    internal static double Clamp(double value, double min, double max)
    {
        if (!double.IsFinite(value))
        {
            return min;
        }

        if (!double.IsFinite(min))
        {
            min = 0;
        }

        if (!double.IsFinite(max) || max < min)
        {
            max = min;
        }

        return Math.Clamp(value, min, max);
    }

    internal static double GetExpandedWidth(double containerWidth)
    {
        return GetExpandedDimension(containerWidth, ExpandedWidthPercentage, MaxExpandedWidth);
    }

    internal static double GetExpandedHeight(double containerHeight)
    {
        return GetExpandedDimension(containerHeight, ExpandedHeightPercentage, MaxExpandedHeight);
    }

    internal static FloatingChatButtonBounds GetInitialCollapsedBounds(double containerWidth, double containerHeight)
    {
        if (!IsUsableSize(containerWidth, containerHeight))
        {
            return new FloatingChatButtonBounds(0, 0, CollapsedSize, CollapsedSize);
        }

        return new FloatingChatButtonBounds(
            Math.Max(0, containerWidth - CollapsedSize - EdgePadding),
            Math.Max(0, containerHeight - CollapsedSize - EdgePadding),
            CollapsedSize,
            CollapsedSize);
    }

    internal static FloatingChatButtonBounds ClampCollapsedBounds(double x, double y, double containerWidth, double containerHeight)
    {
        return new FloatingChatButtonBounds(
            Clamp(x, 0, Math.Max(0, containerWidth - CollapsedSize)),
            Clamp(y, 0, Math.Max(0, containerHeight - CollapsedSize)),
            CollapsedSize,
            CollapsedSize);
    }

    internal static FloatingChatButtonBounds GetExpandedBounds(double collapsedX, double collapsedY, double containerWidth, double containerHeight)
    {
        var targetWidth = GetExpandedWidth(containerWidth);
        var targetHeight = GetExpandedHeight(containerHeight);
        var isRightHalf = collapsedX > containerWidth / 2;
        var maxX = Math.Max(EdgePadding, containerWidth - targetWidth - EdgePadding);
        var maxY = Math.Max(EdgePadding, containerHeight - targetHeight - EdgePadding);

        return new FloatingChatButtonBounds(
            Clamp(isRightHalf ? maxX : EdgePadding, EdgePadding, maxX),
            Clamp(collapsedY, EdgePadding, maxY),
            targetWidth,
            targetHeight);
    }

    internal static FloatingChatButtonBounds ClampExpandedBounds(double x, double y, double containerWidth, double containerHeight)
    {
        var targetWidth = GetExpandedWidth(containerWidth);
        var targetHeight = GetExpandedHeight(containerHeight);

        return new FloatingChatButtonBounds(
            Clamp(x, EdgePadding, Math.Max(EdgePadding, containerWidth - targetWidth - EdgePadding)),
            Clamp(y, EdgePadding, Math.Max(EdgePadding, containerHeight - targetHeight - EdgePadding)),
            targetWidth,
            targetHeight);
    }

    internal static double GetSnapTargetX(double currentX, double controlWidth, double containerWidth)
    {
        var maxX = Math.Max(0, containerWidth - Math.Max(0, controlWidth));
        var normalizedX = Clamp(currentX, 0, maxX);
        var snapRight = normalizedX + Math.Max(0, controlWidth) / 2 > containerWidth / 2;
        return snapRight ? Math.Max(0, maxX - EdgePadding) : Math.Min(EdgePadding, maxX);
    }

    internal static double GetClampedY(double currentY, double controlHeight, double containerHeight)
    {
        return Clamp(currentY, 0, Math.Max(0, containerHeight - Math.Max(0, controlHeight)));
    }

    internal static string NormalizeOutgoingMessageText(string? text)
    {
        return text?.Trim() ?? string.Empty;
    }

    private static double GetExpandedDimension(double containerDimension, double ratio, double maxDimension)
    {
        if (!double.IsFinite(containerDimension) || containerDimension <= 0)
        {
            return 0;
        }

        return Math.Min(containerDimension * ratio, Math.Min(maxDimension, containerDimension));
    }
}
