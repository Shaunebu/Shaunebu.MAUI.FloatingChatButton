using FloatingChatButton.Internal;
using Xunit;

namespace FloatingChatButton.Tests;

public sealed class FloatingChatButtonLayoutTests
{
    public static TheoryData<double, double> InvalidContainerSizes => new()
    {
        { 0, 100 },
        { 100, 0 },
        { -1, 100 },
        { 100, -1 },
        { double.NaN, 100 },
        { 100, double.NaN },
        { double.PositiveInfinity, 100 },
        { 100, double.NegativeInfinity }
    };

    [Fact]
    public void NormalContainerInitialCollapsedBoundsAreFiniteAndVisible()
    {
        var bounds = FloatingChatButtonLayout.GetInitialCollapsedBounds(360, 640);

        Assert.Equal(280, bounds.X);
        Assert.Equal(560, bounds.Y);
        AssertInside(bounds, 360, 640);
    }

    [Theory]
    [InlineData(30, 30)]
    [InlineData(60, 60)]
    [InlineData(40, 120)]
    public void VerySmallContainersStillProduceFiniteBounds(double width, double height)
    {
        var bounds = FloatingChatButtonLayout.GetInitialCollapsedBounds(width, height);

        AssertFinite(bounds);
        Assert.True(bounds.X >= 0);
        Assert.True(bounds.Y >= 0);
    }

    [Theory]
    [InlineData(-100, -50, 360, 640, 0, 0)]
    [InlineData(999, 999, 360, 640, 300, 580)]
    [InlineData(double.NaN, double.NaN, 360, 640, 0, 0)]
    [InlineData(double.PositiveInfinity, double.NegativeInfinity, 360, 640, 0, 0)]
    [InlineData(40, 50, 30, 30, 0, 0)]
    public void CollapsedBoundsAreClampedInsideVisibleRange(double x, double y, double width, double height, double expectedX, double expectedY)
    {
        var bounds = FloatingChatButtonLayout.ClampCollapsedBounds(x, y, width, height);

        Assert.Equal(expectedX, bounds.X);
        Assert.Equal(expectedY, bounds.Y);
        AssertFinite(bounds);
    }

    [Theory]
    [InlineData(360, 640, 288, 448)]
    [InlineData(1000, 1000, 400, 600)]
    [InlineData(100, 100, 80, 70)]
    [InlineData(0, 100, 0, 70)]
    [InlineData(double.NaN, 100, 0, 70)]
    [InlineData(double.PositiveInfinity, 100, 0, 70)]
    public void ExpandedDimensionsAreClampedToContainerAndMaximums(double width, double height, double expectedWidth, double expectedHeight)
    {
        Assert.Equal(expectedWidth, FloatingChatButtonLayout.GetExpandedWidth(width));
        Assert.Equal(expectedHeight, FloatingChatButtonLayout.GetExpandedHeight(height));
    }

    [Theory]
    [InlineData(10, 60, 360, 20)]
    [InlineData(150, 60, 360, 20)]
    [InlineData(151, 60, 360, 280)]
    [InlineData(999, 60, 360, 280)]
    [InlineData(double.NaN, 60, 360, 20)]
    [InlineData(double.PositiveInfinity, 60, 360, 20)]
    [InlineData(10, 500, 300, 0)]
    public void SnapTargetIsFiniteAndInsideHorizontalRange(double x, double controlWidth, double containerWidth, double expected)
    {
        var target = FloatingChatButtonLayout.GetSnapTargetX(x, controlWidth, containerWidth);

        Assert.Equal(expected, target);
        Assert.True(double.IsFinite(target));
        Assert.InRange(target, 0, Math.Max(0, containerWidth - Math.Max(0, controlWidth)));
    }

    [Theory]
    [MemberData(nameof(InvalidContainerSizes))]
    public void InvalidUsableSizesAreRejected(double width, double height)
    {
        Assert.False(FloatingChatButtonLayout.IsUsableSize(width, height));
    }

    [Fact]
    public void ExpandedBoundsAreFiniteForOrientationSizeChange()
    {
        var portrait = FloatingChatButtonLayout.GetExpandedBounds(280, 560, 360, 640);
        var landscape = FloatingChatButtonLayout.ClampExpandedBounds(portrait.X, portrait.Y, 640, 360);

        AssertFinite(portrait);
        AssertFinite(landscape);
        AssertInside(landscape, 640, 360);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData(" hello ", "hello")]
    public void OutgoingMessageTextIsTrimmedAndEmptyWhenWhitespace(string? input, string expected)
    {
        Assert.Equal(expected, FloatingChatButtonLayout.NormalizeOutgoingMessageText(input));
    }

    private static void AssertInside(FloatingChatButtonBounds bounds, double containerWidth, double containerHeight)
    {
        AssertFinite(bounds);
        Assert.InRange(bounds.X, 0, Math.Max(0, containerWidth - bounds.Width));
        Assert.InRange(bounds.Y, 0, Math.Max(0, containerHeight - bounds.Height));
    }

    private static void AssertFinite(FloatingChatButtonBounds bounds)
    {
        Assert.True(double.IsFinite(bounds.X));
        Assert.True(double.IsFinite(bounds.Y));
        Assert.True(double.IsFinite(bounds.Width));
        Assert.True(double.IsFinite(bounds.Height));
    }
}
