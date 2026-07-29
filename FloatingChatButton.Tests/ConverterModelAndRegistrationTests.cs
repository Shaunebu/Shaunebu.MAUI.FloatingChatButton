using FloatingChatButton.Converters;
using FloatingChatButton.Internal;
using FloatingChatButton.Models;
using Microsoft.Maui.Controls;
using System.Globalization;
using Xunit;

namespace FloatingChatButton.Tests;

public sealed class ConverterModelAndRegistrationTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void InvertedBoolConverterConvertsBooleanValues(bool input, bool expected)
    {
        var converter = new InvertedBoolConverter();

        Assert.Equal(expected, converter.Convert(input, typeof(bool), null, CultureInfo.InvariantCulture));
        Assert.Equal(expected, converter.ConvertBack(input, typeof(bool), null, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not a bool")]
    [InlineData(1)]
    public void InvertedBoolConverterHandlesUnexpectedValues(object? input)
    {
        var converter = new InvertedBoolConverter();

        Assert.False((bool)converter.Convert(input, typeof(bool), null, CultureInfo.InvariantCulture));
        Assert.False((bool)converter.ConvertBack(input, typeof(bool), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ChatMessageDefaultsAreSafe()
    {
        var message = new ChatMessage();

        Assert.Equal(string.Empty, message.Text);
        Assert.False(message.IsIncoming);
    }

    [Fact]
    public void ChatMessagePropertiesRoundTripAndDoNotNotify()
    {
        var message = new ChatMessage
        {
            Text = "Hello",
            IsIncoming = true
        };

        Assert.Equal("Hello", message.Text);
        Assert.True(message.IsIncoming);
        Assert.False(message is System.ComponentModel.INotifyPropertyChanged);
    }

    [Fact]
    public void ChatMessageAllowsNullAssignmentAsCurrentPublicBehavior()
    {
        var message = new ChatMessage
        {
            Text = null!
        };

        Assert.Null(message.Text);
    }

    [Fact]
    public void RegistrationRejectsNullBuilder()
    {
        Assert.Throws<ArgumentNullException>("builder", () => MauiExtensions.UseFloatingChatButton(null!));
    }

    [Fact]
    public void RepeatedRegistrationReturnsSameBuilder()
    {
        var builder = MauiApp.CreateBuilder();

        var first = builder.UseFloatingChatButton();
        var second = builder.UseFloatingChatButton();

        Assert.Same(builder, first);
        Assert.Same(builder, second);
    }

    [Fact]
    public void TransitionCoordinatorLatestVersionWins()
    {
        var coordinator = new TransitionCoordinator();

        var first = coordinator.Begin();
        var second = coordinator.Begin();

        Assert.False(coordinator.IsCurrent(first));
        Assert.True(coordinator.IsCurrent(second));
        Assert.False(coordinator.CompleteIfCurrent(first));
        Assert.True(coordinator.IsTransitioning);
        Assert.True(coordinator.CompleteIfCurrent(second));
        Assert.False(coordinator.IsTransitioning);
    }

    [Fact]
    public void TransitionCoordinatorCancelInvalidatesPreviousWork()
    {
        var coordinator = new TransitionCoordinator();
        var version = coordinator.Begin();

        coordinator.Cancel();

        Assert.False(coordinator.IsCurrent(version));
        Assert.False(coordinator.IsTransitioning);
    }
}
