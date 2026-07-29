using FloatingChatButton.Extensions;
using FloatingChatButton.Internal;
using FloatingChatButton.Models;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace FloatingChatButton.Controls;

/// <summary>
/// Displays a draggable floating entry point that expands into an embedded chat panel for .NET MAUI applications.
/// </summary>
/// <remarks>
/// The control is intended to be placed over page content, usually in an <see cref="AbsoluteLayout"/> or similar overlay container.
/// It manages its own drag, snap, expand, collapse, and message-list presentation behavior.
/// </remarks>
public partial class FloatingChatButton : ContentView, IDisposable
{
    private const uint OverlayFadeDuration = 200;
    private const uint ResizeExpandDuration = 300;
    private const uint ResizeCollapseDuration = 250;
    private const uint SnapDuration = 300;
    private const uint ThrowDuration = 500;

    private bool _disposed;
    private bool _hasCollapsedBounds;
    private bool _isDragging;
    private bool _isInitialPositionSet;
    private DateTime _lastUpdateTime;
    private double _velocityX;
    private Point _lastPosition;
    private Point _previousPosition;
    private Rect _collapsedBounds;
    private readonly MessageCollectionObserver _messageObserver;
    private readonly TransitionCoordinator _transitionCoordinator = new();

    /// <summary>
    /// Provides the conversation items rendered by the expanded chat panel.
    /// </summary>
    /// <remarks>
    /// This bindable property defaults to a non-null empty <see cref="ObservableCollection{T}"/> of <see cref="ChatMessage"/>.
    /// Assigning <see langword="null"/> is allowed and is coerced to a new empty collection. Replacing the collection takes
    /// effect immediately: the control detaches from the previous collection, observes the replacement while loaded, and scrolls
    /// to the newest message when the panel is expanded. Mutate the collection on the UI thread or through a dispatcher.
    /// </remarks>
    public ObservableCollection<ChatMessage> Messages
    {
        get => (ObservableCollection<ChatMessage>)GetValue(MessagesProperty);
        set => SetValue(MessagesProperty, value);
    }

    /// <summary>
    /// Identifies the bindable backing store for <see cref="Messages"/>.
    /// </summary>
    /// <remarks>
    /// The default value is a new empty message collection per control instance. The field coerces <see langword="null"/>
    /// assignments to an empty collection so bindings always expose a usable collection reference.
    /// </remarks>
    public static readonly BindableProperty MessagesProperty =
        BindableProperty.Create(
            nameof(Messages),
            typeof(ObservableCollection<ChatMessage>),
            typeof(FloatingChatButton),
            defaultValueCreator: _ => new ObservableCollection<ChatMessage>(),
            coerceValue: (_, value) => value ?? new ObservableCollection<ChatMessage>(),
            propertyChanged: OnMessagesChanged);

    /// <summary>
    /// Specifies the primary accent color used by the collapsed floating button and by the expanded panel when no secondary
    /// resource color is available.
    /// </summary>
    /// <remarks>
    /// This bindable property defaults to <see cref="Colors.Blue"/>. Assign a non-null <see cref="Color"/>; invalid values are
    /// rejected by the bindable-property validator. Changes are reflected by the collapsed button immediately, and by expanded
    /// state transitions the next time the control applies its panel background.
    /// </remarks>
    public Color PrimaryColor
    {
        get => (Color)GetValue(PrimaryColorProperty);
        set => SetValue(PrimaryColorProperty, value);
    }

    /// <summary>
    /// Identifies the bindable backing store for <see cref="PrimaryColor"/>.
    /// </summary>
    /// <remarks>
    /// The default color is <see cref="Colors.Blue"/>. The field validates that values supplied through bindings are
    /// <see cref="Color"/> instances.
    /// </remarks>
    public static readonly BindableProperty PrimaryColorProperty =
        BindableProperty.Create(
            nameof(PrimaryColor),
            typeof(Color),
            typeof(FloatingChatButton),
            Colors.Blue,
            validateValue: (_, value) => value is Color);

    /// <summary>
    /// Controls whether the floating button is collapsed or expanded into the chat panel.
    /// </summary>
    /// <remarks>
    /// This two-way bindable property defaults to <see langword="false"/>. Setting it to <see langword="true"/> starts the
    /// expand transition; setting it to <see langword="false"/> starts the collapse transition. The property is non-nullable
    /// and expects a Boolean value from bindings.
    /// </remarks>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    /// Identifies the bindable backing store for <see cref="IsExpanded"/>.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="false"/> and the binding mode is <see cref="BindingMode.TwoWay"/>, allowing view
    /// models to both request and observe expansion-state changes.
    /// </remarks>
    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(
            nameof(IsExpanded),
            typeof(bool),
            typeof(FloatingChatButton),
            false,
            BindingMode.TwoWay,
            propertyChanged: OnIsExpandedChanged);

    /// <summary>
    /// Identifies the bindable backing store for <see cref="BotIcon"/>.
    /// </summary>
    /// <remarks>
    /// The default value is the image source created from the bundled <c>dotnet_bot</c> asset name. The binding mode is
    /// <see cref="BindingMode.OneWay"/>, so changes flow from the binding source into the collapsed button icon.
    /// </remarks>
    public static readonly BindableProperty BotIconProperty =
        BindableProperty.Create(
            nameof(BotIcon),
            typeof(ImageSource),
            typeof(FloatingChatButton),
            ImageSource.FromFile("dotnet_bot"),
            BindingMode.OneWay);

    /// <summary>
    /// Supplies the image displayed inside the collapsed floating button.
    /// </summary>
    /// <remarks>
    /// This bindable property defaults to <c>dotnet_bot</c>. Updating the value changes the collapsed icon on the next binding
    /// refresh or property assignment. A <see langword="null"/> value is accepted by the image pipeline and leaves the icon
    /// without an image source.
    /// </remarks>
    public ImageSource BotIcon
    {
        get => (ImageSource)GetValue(BotIconProperty);
        set => SetValue(BotIconProperty, value);
    }

    /// <summary>
    /// Creates a floating chat button with the default message collection, colors, icon, gestures, and accessibility metadata.
    /// </summary>
    public FloatingChatButton()
    {
        InitializeComponent();
        _messageObserver = new MessageCollectionObserver(Messages);
        _messageObserver.CollectionChanged += OnMessagesCollectionChanged;
        InitializeAccessibility();
        InitializeGestures();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <summary>
    /// Releases event subscriptions, stops transient animations, and prevents further gesture or transition work for this control.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Clears transient animation and gesture state before the platform handler attached to this control changes.
    /// </summary>
    /// <param name="args">Describes the old and new handler involved in the handler-change operation.</param>
    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        CleanupTransientState();
        base.OnHandlerChanging(args);
    }

    /// <summary>
    /// Positions or clamps the collapsed button and expanded panel after the containing layout assigns a size.
    /// </summary>
    /// <param name="width">The available width assigned by the parent layout, in device-independent units.</param>
    /// <param name="height">The available height assigned by the parent layout, in device-independent units.</param>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (!FloatingChatButtonLayout.IsUsableSize(width, height))
        {
            return;
        }

        if (!_isInitialPositionSet)
        {
            AbsoluteLayout.SetLayoutBounds(
                chatBubble,
                ToRect(FloatingChatButtonLayout.GetInitialCollapsedBounds(width, height)));
            _isInitialPositionSet = true;
        }

        if (IsExpanded)
        {
            var bounds = AbsoluteLayout.GetLayoutBounds(chatBubble);

            AbsoluteLayout.SetLayoutBounds(
                chatBubble,
                ToRect(FloatingChatButtonLayout.ClampExpandedBounds(bounds.X, bounds.Y, width, height)));
        }
        else
        {
            var bounds = AbsoluteLayout.GetLayoutBounds(chatBubble);
            AbsoluteLayout.SetLayoutBounds(
                chatBubble,
                ToRect(FloatingChatButtonLayout.ClampCollapsedBounds(bounds.X, bounds.Y, width, height)));
        }
    }

    /// <summary>
    /// Performs the managed cleanup used by <see cref="Dispose()"/>.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when cleanup was requested by application code; <see langword="false"/> when called by a finalizer.
    /// </param>
    /// <remarks>
    /// Subclasses overriding this method should call the base implementation so message observation, page lifecycle handlers,
    /// and active animations are released consistently.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (disposing)
        {
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
            _messageObserver.CollectionChanged -= OnMessagesCollectionChanged;
            _messageObserver.Dispose();
            CleanupTransientState();
        }
    }

    private static void OnIsExpandedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (Equals(oldValue, newValue))
        {
            return;
        }

        var control = (FloatingChatButton)bindable;
        control.StartTransition((bool)newValue);
    }

    private static void OnMessagesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FloatingChatButton)bindable;
        var coercedMessages = control._messageObserver.Replace(newValue as ObservableCollection<ChatMessage>);
        if (!ReferenceEquals(newValue, coercedMessages))
        {
            control.SetValue(MessagesProperty, coercedMessages);
        }

        control.ScrollToLatestMessage();
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        _messageObserver.SetActive(true);
        UpdateAccessibilityState();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        _messageObserver.SetActive(false);
        CleanupTransientState();
    }

    private void StartTransition(bool expand)
    {
        if (_disposed)
        {
            return;
        }

        var version = _transitionCoordinator.Begin();
        AbortActiveAnimations();

        if (Dispatcher?.IsDispatchRequired == true)
        {
            Dispatcher.Dispatch(() => _ = RunTransitionAsync(expand, version));
        }
        else if (MainThread.IsMainThread)
        {
            _ = RunTransitionAsync(expand, version);
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() => _ = RunTransitionAsync(expand, version));
        }
    }

    private async Task RunTransitionAsync(bool expand, int version)
    {
        try
        {
            if (_disposed || !FloatingChatButtonLayout.IsUsableSize(Width, Height))
            {
                UpdateAccessibilityState();
                return;
            }

            if (expand)
            {
                await ExpandBubbleInternalAsync(version).ConfigureAwait(true);
            }
            else
            {
                await CollapseBubbleInternalAsync(version).ConfigureAwait(true);
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            Debug.WriteLine($"FloatingChatButton transition failed: {ex.GetType().Name}");
        }
        finally
        {
            if (_transitionCoordinator.CompleteIfCurrent(version))
            {
                UpdateAccessibilityState();
            }
        }
    }

    private async Task ExpandBubbleInternalAsync(int version)
    {
        chatBubble.AnchorX = 0;
        chatBubble.AnchorY = 0;

        _collapsedBounds = AbsoluteLayout.GetLayoutBounds(chatBubble);
        _hasCollapsedBounds = true;

        var expandedBounds = FloatingChatButtonLayout.GetExpandedBounds(_collapsedBounds.X, _collapsedBounds.Y, Width, Height);

        overlay.Opacity = 0;
        overlay.IsVisible = true;
        await overlay.FadeTo(0.4, OverlayFadeDuration).ConfigureAwait(true);

        if (_disposed || !_transitionCoordinator.IsCurrent(version))
        {
            return;
        }

        AbsoluteLayout.SetLayoutBounds(chatBubble, ToRect(expandedBounds));
        await chatBubble.ResizeTo(expandedBounds.Width, expandedBounds.Height, ResizeExpandDuration, Easing.SpringOut).ConfigureAwait(true);
        chatBubble.BackgroundColor = GetResourceColor("Secondary", PrimaryColor);
        ScrollToLatestMessage();
    }

    private async Task CollapseBubbleInternalAsync(int version)
    {
        chatBubble.AnchorX = 0;
        chatBubble.AnchorY = 0;

        if (_hasCollapsedBounds)
        {
            AbsoluteLayout.SetLayoutBounds(
                chatBubble,
                ToRect(FloatingChatButtonLayout.ClampCollapsedBounds(_collapsedBounds.X, _collapsedBounds.Y, Width, Height)));

            await Task.WhenAll(
                overlay.FadeTo(0, OverlayFadeDuration),
                chatBubble.ResizeTo(
                    FloatingChatButtonLayout.CollapsedSize,
                    FloatingChatButtonLayout.CollapsedSize,
                    ResizeCollapseDuration,
                    Easing.SpringOut)).ConfigureAwait(true);
        }

        if (_disposed || !_transitionCoordinator.IsCurrent(version))
        {
            return;
        }

        overlay.IsVisible = false;
        chatBubble.BackgroundColor = PrimaryColor;
    }

    private void ToggleBubble()
    {
        if (!_transitionCoordinator.IsTransitioning && !_isDragging)
        {
            IsExpanded = !IsExpanded;
        }
    }

    private void InitializeGestures()
    {
        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnBubblePanned;

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (_, _) => ToggleBubble();

        chatBubble.GestureRecognizers.Add(panGesture);
        chatBubble.GestureRecognizers.Add(tapGesture);

        var overlayTapGesture = new TapGestureRecognizer();
        overlayTapGesture.Tapped += (_, _) =>
        {
            if (IsExpanded)
            {
                IsExpanded = false;
            }
        };
        overlay.GestureRecognizers.Add(overlayTapGesture);

        chatBubble.AnchorX = 0;
        chatBubble.AnchorY = 0;
    }

    private void InitializeAccessibility()
    {
        SemanticProperties.SetDescription(chatBubble, "Open chat");
        SemanticProperties.SetHint(chatBubble, "Double tap to expand or collapse the chat panel.");
        SemanticProperties.SetDescription(MessageEntry, "Chat message");
        SemanticProperties.SetHint(MessageEntry, "Enter a chat message.");
        SemanticProperties.SetDescription(SendButton, "Send message");
        SemanticProperties.SetHint(SendButton, "Adds the typed message to the chat.");
    }

    private void UpdateAccessibilityState()
    {
        SemanticProperties.SetDescription(chatBubble, IsExpanded ? "Chat panel expanded" : "Open chat");
        SemanticProperties.SetHint(chatBubble, IsExpanded ? "Double tap to collapse the chat panel." : "Double tap to expand the chat panel.");
    }

    private void OnBubblePanned(object? sender, PanUpdatedEventArgs e)
    {
        if (_disposed || IsExpanded || _transitionCoordinator.IsTransitioning || chatBubble.Width <= 0 || chatBubble.Height <= 0 || !FloatingChatButtonLayout.IsUsableSize(Width, Height))
        {
            return;
        }

        AbortActiveAnimations();

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _isDragging = true;
                _lastUpdateTime = default;
                _velocityX = 0;
                var currentBounds = AbsoluteLayout.GetLayoutBounds(chatBubble);
                _lastPosition = new Point(currentBounds.X, currentBounds.Y);
                _previousPosition = _lastPosition;
                break;

            case GestureStatus.Running:
                if (_isDragging)
                {
                    MoveBubble(e.TotalX, e.TotalY);
                }
                break;

            case GestureStatus.Canceled:
                _isDragging = false;
                SnapToEdge();
                break;

            case GestureStatus.Completed:
                _isDragging = false;
                CompleteDrag();
                break;
        }
    }

    private void MoveBubble(double totalX, double totalY)
    {
        var targetX = FloatingChatButtonLayout.Clamp(_lastPosition.X + totalX, 0, Math.Max(0, Width - chatBubble.Width));
        var targetY = FloatingChatButtonLayout.Clamp(_lastPosition.Y + totalY, 0, Math.Max(0, Height - chatBubble.Height));
        var currentBounds = AbsoluteLayout.GetLayoutBounds(chatBubble);
        var smoothX = currentBounds.X + (targetX - currentBounds.X) * 0.5;
        var smoothY = currentBounds.Y + (targetY - currentBounds.Y) * 0.5;

        AbsoluteLayout.SetLayoutBounds(chatBubble, new Rect(smoothX, smoothY, currentBounds.Width, currentBounds.Height));

        var now = DateTime.UtcNow;
        if (_lastUpdateTime != default)
        {
            var elapsed = (now - _lastUpdateTime).TotalSeconds;
            if (elapsed > 0)
            {
                _velocityX = (smoothX - _previousPosition.X) / elapsed;
            }
        }

        _previousPosition = new Point(smoothX, smoothY);
        _lastUpdateTime = now;
    }

    private void CompleteDrag()
    {
        var bounds = AbsoluteLayout.GetLayoutBounds(chatBubble);
        var finalX = FloatingChatButtonLayout.Clamp(bounds.X + (_velocityX * 0.1), 0, Math.Max(0, Width - chatBubble.Width));
        var shouldSnapToEdge = Math.Abs(_velocityX) < 50 || finalX < 50 || finalX > Math.Max(0, Width - chatBubble.Width) - 50;

        if (shouldSnapToEdge)
        {
            SnapToEdge();
        }
        else
        {
            AnimateThrow(finalX, bounds.Y);
        }

        UpdateAnchorPoints(bounds.X, bounds.Y);
    }

    private void AnimateThrow(double finalX, double y)
    {
        var startX = AbsoluteLayout.GetLayoutBounds(chatBubble).X;

        new Animation(v =>
        {
            var currentX = startX + (finalX - startX) * v;
            AbsoluteLayout.SetLayoutBounds(chatBubble, new Rect(currentX, y, chatBubble.Width, chatBubble.Height));
        })
        .Commit(this, "ThrowAnimation", 16, ThrowDuration, Easing.CubicOut, finished: (_, _) => SnapToEdge());
    }

    private void SnapToEdge()
    {
        if (_disposed || !FloatingChatButtonLayout.IsUsableSize(Width, Height))
        {
            return;
        }

        this.AbortAnimation("SnapAnimation");

        var bounds = AbsoluteLayout.GetLayoutBounds(chatBubble);
        var snapTargetX = FloatingChatButtonLayout.GetSnapTargetX(bounds.X, chatBubble.Width, Width);
        var targetY = FloatingChatButtonLayout.GetClampedY(bounds.Y, chatBubble.Height, Height);

        new Animation(
            callback: v =>
            {
                var currentX = bounds.X + (snapTargetX - bounds.X) * v;
                AbsoluteLayout.SetLayoutBounds(chatBubble, new Rect(currentX, targetY, chatBubble.Width, chatBubble.Height));
            },
            start: 0,
            end: 1)
        .Commit(
            owner: this,
            name: "SnapAnimation",
            length: SnapDuration,
            easing: Easing.SpringOut,
            finished: (_, _) => UpdateAnchorPoints(snapTargetX, targetY),
            repeat: () => false);
    }

    private void OnSendButtonClicked(object? sender, EventArgs e)
    {
        var text = FloatingChatButtonLayout.NormalizeOutgoingMessageText(MessageEntry.Text);
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        Messages.Add(new ChatMessage { Text = text, IsIncoming = false });
        MessageEntry.Text = string.Empty;
    }

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Dispatcher?.IsDispatchRequired == true)
        {
            Dispatcher.Dispatch(ScrollToLatestMessage);
        }
        else
        {
            ScrollToLatestMessage();
        }
    }

    private void ScrollToLatestMessage()
    {
        if (_disposed || !_messageObserver.IsActive || !IsExpanded || Messages.Count == 0)
        {
            return;
        }

        MessagesCollectionView.ScrollTo(Messages[^1], position: ScrollToPosition.End, animate: true);
    }

    private void CleanupTransientState()
    {
        _isDragging = false;
        _transitionCoordinator.Cancel();
        AbortActiveAnimations();
    }

    private void AbortActiveAnimations()
    {
        this.AbortAnimation("SnapAnimation");
        this.AbortAnimation("ThrowAnimation");
        chatBubble.AbortAnimation("Resize");
        chatBubble.AbortAnimation("FadeTo");
        overlay.AbortAnimation("FadeTo");
    }

    private Color GetResourceColor(string key, Color fallback)
    {
        return Application.Current?.Resources.TryGetValue(key, out var resource) == true && resource is Color color
            ? color
            : fallback;
    }

    private void UpdateAnchorPoints(double x, double y)
    {
        chatBubble.AnchorX = x > Width / 2 ? 1 : 0;
        chatBubble.AnchorY = y > Height / 2 ? 1 : 0;
    }

    private static Rect ToRect(FloatingChatButtonBounds bounds)
    {
        return new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }
}
