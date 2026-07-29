using FloatingChatButton.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FloatingChatButton.Internal;

internal sealed class MessageCollectionObserver : IDisposable
{
    private bool _disposed;
    private bool _isActive;
    private bool _isSubscribed;

    internal MessageCollectionObserver(ObservableCollection<ChatMessage>? messages)
    {
        Current = CoerceMessages(messages);
    }

    internal event NotifyCollectionChangedEventHandler? CollectionChanged;

    internal ObservableCollection<ChatMessage> Current { get; private set; }

    internal bool IsActive => _isActive;

    internal bool IsSubscribed => _isSubscribed;

    internal void SetActive(bool isActive)
    {
        ThrowIfDisposed();

        if (_isActive == isActive)
        {
            return;
        }

        _isActive = isActive;

        if (_isActive)
        {
            Subscribe();
        }
        else
        {
            Unsubscribe();
        }
    }

    internal ObservableCollection<ChatMessage> Replace(ObservableCollection<ChatMessage>? messages)
    {
        ThrowIfDisposed();

        var replacement = CoerceMessages(messages);
        if (ReferenceEquals(Current, replacement))
        {
            return Current;
        }

        Unsubscribe();
        Current = replacement;

        if (_isActive)
        {
            Subscribe();
        }

        return Current;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Unsubscribe();
        _disposed = true;
    }

    private static ObservableCollection<ChatMessage> CoerceMessages(ObservableCollection<ChatMessage>? messages)
    {
        return messages ?? new ObservableCollection<ChatMessage>();
    }

    private void Subscribe()
    {
        if (_isSubscribed)
        {
            return;
        }

        Current.CollectionChanged += OnCollectionChanged;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed)
        {
            return;
        }

        Current.CollectionChanged -= OnCollectionChanged;
        _isSubscribed = false;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(sender, e);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
