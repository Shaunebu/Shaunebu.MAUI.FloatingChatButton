using FloatingChatButton.Internal;
using FloatingChatButton.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xunit;

namespace FloatingChatButton.Tests;

public sealed class MessageCollectionObserverTests
{
    [Fact]
    public void NullCollectionCreatesSafePerInstanceCollection()
    {
        var first = new MessageCollectionObserver(null);
        var second = new MessageCollectionObserver(null);

        Assert.NotNull(first.Current);
        Assert.NotNull(second.Current);
        Assert.NotSame(first.Current, second.Current);
    }

    [Fact]
    public void MessagesAddedToOneCollectionDoNotAppearInAnother()
    {
        var first = new MessageCollectionObserver(null);
        var second = new MessageCollectionObserver(null);

        first.Current.Add(new ChatMessage { Text = "Only first" });

        Assert.Single(first.Current);
        Assert.Empty(second.Current);
    }

    [Fact]
    public void ReplacingCollectionUnsubscribesOldAndSubscribesNew()
    {
        var oldMessages = new ObservableCollection<ChatMessage>();
        var newMessages = new ObservableCollection<ChatMessage>();
        var observer = new MessageCollectionObserver(oldMessages);
        var events = 0;
        observer.CollectionChanged += (_, _) => events++;
        observer.SetActive(true);

        observer.Replace(newMessages);
        oldMessages.Add(new ChatMessage { Text = "Old" });
        newMessages.Add(new ChatMessage { Text = "New" });

        Assert.Equal(1, events);
        Assert.Same(newMessages, observer.Current);
        Assert.True(observer.IsSubscribed);
    }

    [Fact]
    public void CollectionAddRemoveReplaceAndResetDoNotThrowWhenActive()
    {
        var messages = new ObservableCollection<ChatMessage>
        {
            new() { Text = "One" },
            new() { Text = "Two" }
        };
        var observer = new MessageCollectionObserver(messages);
        var actions = new List<NotifyCollectionChangedAction>();
        observer.CollectionChanged += (_, e) => actions.Add(e.Action);
        observer.SetActive(true);

        messages.Add(new ChatMessage { Text = "Three" });
        messages.RemoveAt(0);
        messages[0] = new ChatMessage { Text = "Replacement" };
        messages.Clear();

        Assert.Equal(
            [NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Remove, NotifyCollectionChangedAction.Replace, NotifyCollectionChangedAction.Reset],
            actions);
    }

    [Fact]
    public void RepeatedLoadUnloadCyclesDoNotDuplicateSubscriptions()
    {
        var messages = new ObservableCollection<ChatMessage>();
        var observer = new MessageCollectionObserver(messages);
        var events = 0;
        observer.CollectionChanged += (_, _) => events++;

        observer.SetActive(true);
        observer.SetActive(true);
        observer.SetActive(false);
        observer.SetActive(false);
        observer.SetActive(true);
        messages.Add(new ChatMessage { Text = "Once" });

        Assert.Equal(1, events);
        Assert.True(observer.IsSubscribed);
    }

    [Fact]
    public void UnloadedObserverDoesNotForwardCollectionChanges()
    {
        var messages = new ObservableCollection<ChatMessage>();
        var observer = new MessageCollectionObserver(messages);
        var events = 0;
        observer.CollectionChanged += (_, _) => events++;
        observer.SetActive(true);

        observer.SetActive(false);
        messages.Add(new ChatMessage { Text = "No scroll while unloaded" });

        Assert.Equal(0, events);
        Assert.False(observer.IsSubscribed);
    }

    [Fact]
    public void DisposeUnsubscribesAndIsIdempotent()
    {
        var messages = new ObservableCollection<ChatMessage>();
        var observer = new MessageCollectionObserver(messages);
        var events = 0;
        observer.CollectionChanged += (_, _) => events++;
        observer.SetActive(true);

        observer.Dispose();
        observer.Dispose();
        messages.Add(new ChatMessage { Text = "After dispose" });

        Assert.Equal(0, events);
        Assert.False(observer.IsSubscribed);
    }
}
