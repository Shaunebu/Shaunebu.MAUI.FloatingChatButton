using FloatingChatButton.Extensions;
using FloatingChatButton.Models;
using System.Collections.ObjectModel;

namespace FloatingChatButton.Controls;

public partial class FloatingChatButton : Microsoft.Maui.Controls.ContentView
{
    #region Fields
    private bool _isDragging;
    private Point _lastPosition;
    private bool _isExpanded
    {
        get => IsExpanded;
        set => IsExpanded = value;
    }
    private const int CollapsedSize = 60;
    private const double ExpandedWidthPercentage = 0.8;
    private const double ExpandedHeightPercentage = 0.7;
    private const int MaxExpandedWidth = 400;
    private const int MaxExpandedHeight = 600;
    private const int EdgePadding = 20;
    private DateTime _lastUpdateTime;
    private Point _previousPosition;
    private double _velocityX;
    private double _velocityY;
    private Rect _collapsedBounds;
    private bool _hasCollapsedBounds = false;
    private bool _isInitialPositionSet = false;
    #endregion

    #region Properties    
    /// <summary>
    /// Gets or sets the messages.
    /// </summary>
    /// <value>
    /// The messages.
    /// </value>
    public ObservableCollection<ChatMessage> Messages
    {
        get => (ObservableCollection<ChatMessage>)GetValue(MessagesProperty);
        set => SetValue(MessagesProperty, value);
    }
    public static readonly BindableProperty MessagesProperty =
       BindableProperty.Create(nameof(Messages), typeof(ObservableCollection<ChatMessage>), typeof(FloatingChatButton), new ObservableCollection<ChatMessage>());


    /// <summary>
    /// Gets or sets the color of the primary.
    /// </summary>
    /// <value>
    /// The color of the primary.
    /// </value>
    public Color PrimaryColor
    {
        get => (Color)GetValue(PrimaryColorProperty);
        set => SetValue(PrimaryColorProperty, value);
    }
    public static readonly BindableProperty PrimaryColorProperty =
        BindableProperty.Create(nameof(PrimaryColor), typeof(Color), typeof(FloatingChatButton), Colors.Blue);

    /// <summary>
    /// Gets or sets a value indicating whether this instance is expanded.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is expanded; otherwise, <c>false</c>.
    /// </value>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(
            nameof(IsExpanded),
            typeof(bool),
            typeof(FloatingChatButton),
            false,
            BindingMode.TwoWay,
            propertyChanged: OnIsExpandedChanged);

    public static readonly BindableProperty BotIconProperty =
       BindableProperty.Create(
           nameof(BotIcon),
           typeof(ImageSource),
           typeof(FloatingChatButton),
           defaultValue: ImageSource.FromFile("dotnet_bot"), // Default fallback
           defaultBindingMode: BindingMode.OneWay);

    public ImageSource BotIcon
    {
        get => (ImageSource)GetValue(BotIconProperty);
        set => SetValue(BotIconProperty, value);
    }
    #endregion

    #region Constructor    
    /// <summary>
    /// Initializes a new instance of the <see cref="FloatingChatButton"/> class.
    /// </summary>
    public FloatingChatButton()
    {
        InitializeComponent();
        Messages = [];
        InitializeGestures();
    }
    #endregion

    #region Methods    
    /// <summary>
    /// Called when [is expanded changed].
    /// </summary>
    /// <param name="bindable">The bindable.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    private static void OnIsExpandedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FloatingChatButton)bindable;
        var isExpanded = (bool)newValue;

        // Only trigger if the value actually changed
        if (isExpanded != (bool)oldValue)
        {
            if (isExpanded)
            {
                control.ExpandBubbleInternal();
            }
            else
            {
                control.CollapseBubbleInternal();
            }
        }
    }

    /// <summary>
    /// Expands the bubble internal.
    /// </summary>
    private async void ExpandBubbleInternal()
    {
        chatBubble.AnchorX = 0;
        chatBubble.AnchorY = 0;

        _collapsedBounds = AbsoluteLayout.GetLayoutBounds(chatBubble);
        _hasCollapsedBounds = true;

        var targetWidth = Math.Min(Width * ExpandedWidthPercentage, MaxExpandedWidth);
        var targetHeight = Math.Min(Height * ExpandedHeightPercentage, MaxExpandedHeight);

        bool isRightHalf = _collapsedBounds.X > Width / 2;
        bool isBottomHalf = _collapsedBounds.Y > Height / 2;

        double newX = isRightHalf ? Width - targetWidth - EdgePadding : EdgePadding;
        double newY = _collapsedBounds.Y;

        if (newY + targetHeight > Height - EdgePadding)
            newY = Height - targetHeight - EdgePadding;
        if (newY < EdgePadding)
            newY = EdgePadding;

        overlay.Opacity = 0;
        overlay.IsVisible = true;
        await overlay.FadeTo(0.4, 200);

        AbsoluteLayout.SetLayoutBounds(chatBubble, new Rect(newX, newY, targetWidth, targetHeight));
        await chatBubble.ResizeTo(targetWidth, targetHeight, 300, Easing.SpringOut);
        chatBubble.BackgroundColor = (Color)Application.Current!.Resources["Secondary"];

        //bubbleContent.IsVisible = true;
    }

    /// <summary>
    /// Collapses the bubble internal.
    /// </summary>
    private async void CollapseBubbleInternal()
    {
        chatBubble.AnchorX = 0;
        chatBubble.AnchorY = 0;

        if (_hasCollapsedBounds)
        {
            AbsoluteLayout.SetLayoutBounds(chatBubble, new Rect(_collapsedBounds.X, _collapsedBounds.Y, CollapsedSize, CollapsedSize));

            await Task.WhenAll(
                overlay?.FadeTo(0, 200) ?? Task.CompletedTask,
                chatBubble.ResizeTo(CollapsedSize, CollapsedSize, 250, Easing.SpringOut)
            );
        }

        if (overlay != null)
            overlay.IsVisible = false;

        chatBubble.BackgroundColor = (Color)Application.Current!.Resources["Primary"];

        //bubbleContent.IsVisible = false;
    }

    /// <summary>
    /// Toggles the bubble.
    /// </summary>
    private void ToggleBubble()
    {
        IsExpanded = !IsExpanded;
    }

    /// <summary>
    /// Initializes the gestures.
    /// </summary>
    private void InitializeGestures()
    {
        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnBubblePanned;

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => ToggleBubble();

        chatBubble.GestureRecognizers.Add(panGesture);
        chatBubble.GestureRecognizers.Add(tapGesture);

        var overlayTapGesture = new TapGestureRecognizer();
        overlayTapGesture.Tapped += (s, e) => ToggleBubble();
        overlay.GestureRecognizers.Add(overlayTapGesture);

        chatBubble.AnchorX = 0;
        chatBubble.AnchorY = 0;
    }

    /// <summary>
    /// Called when [bubble panned].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="PanUpdatedEventArgs"/> instance containing the event data.</param>
    private void OnBubblePanned(object sender, PanUpdatedEventArgs e)
    {
        this.AbortAnimation("SnapAnimation");
        this.AbortAnimation("ThrowAnimation");

        if (chatBubble.Width <= 0 || chatBubble.Height <= 0) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                this.AbortAnimation("Resize");
                _isDragging = true;
                _lastPosition = new Point(
                    AbsoluteLayout.GetLayoutBounds(chatBubble).X,
                    AbsoluteLayout.GetLayoutBounds(chatBubble).Y);
                break;

            case GestureStatus.Running:
                if (_isDragging && Width > 0 && Height > 0)
                {
                    var targetX = _lastPosition.X + e.TotalX;
                    var targetY = _lastPosition.Y + e.TotalY;

                    targetX = Math.Clamp(targetX, 0, Width - chatBubble.Width);
                    targetY = Math.Clamp(targetY, 0, Height - chatBubble.Height);

                    var smoothFactor = 0.5;
                    var currentBounds = AbsoluteLayout.GetLayoutBounds(chatBubble);
                    var smoothX = currentBounds.X + (targetX - currentBounds.X) * smoothFactor;
                    var smoothY = currentBounds.Y + (targetY - currentBounds.Y) * smoothFactor;

                    AbsoluteLayout.SetLayoutBounds(chatBubble,
                        new Rect(smoothX, smoothY, currentBounds.Width, currentBounds.Height));

                    var now = DateTime.Now;
                    if (_lastUpdateTime != default)
                    {
                        var elapsed = (now - _lastUpdateTime).TotalSeconds;
                        if (elapsed > 0)
                        {
                            _velocityX = (smoothX - _previousPosition.X) / elapsed;
                            _velocityY = (smoothY - _previousPosition.Y) / elapsed;
                        }
                    }
                    _previousPosition = new Point(smoothX, smoothY);
                    _lastUpdateTime = now;
                }
                break;

            case GestureStatus.Completed:
                _isDragging = false;
                var bounds = AbsoluteLayout.GetLayoutBounds(chatBubble);

                var velocityX = e.TotalX / (e.GestureId > 0 ? e.GestureId : 1);
                var throwDistance = velocityX * 0.1;
                var finalX = bounds.X + throwDistance;

                var minX = 0;
                var maxX = Width - chatBubble.Width;

                var shouldSnapToEdge = Math.Abs(velocityX) < 0.5 ||
                                     finalX < minX + 50 ||
                                     finalX > maxX - 50;

                if (shouldSnapToEdge)
                {
                    var snapTargetX = (bounds.X + chatBubble.Width / 2) > Width / 2
                        ? maxX - 20
                        : 20;

                    new Animation(
                        callback: v =>
                        {
                            var currentX = bounds.X + (snapTargetX - bounds.X) * v;
                            AbsoluteLayout.SetLayoutBounds(chatBubble,
                                new Rect(currentX, bounds.Y, chatBubble.Width, chatBubble.Height));
                        },
                        start: 0,
                        end: 1)
                    .Commit(
                        owner: this,
                        name: "SnapAnimation",
                        length: 300,
                        easing: Easing.SpringOut,
                        finished: (v, c) => { },
                        repeat: () => false);
                }
                else
                {
                    finalX = Math.Clamp(finalX, minX, maxX);
                    AnimateThrow(finalX, bounds.Y);
                }

                var newBounds = AbsoluteLayout.GetLayoutBounds(chatBubble);
                UpdateAnchorPoints(newBounds.X, newBounds.Y);
                break;
        }
    }

    /// <summary>
    /// Animates the throw.
    /// </summary>
    /// <param name="finalX">The final x.</param>
    /// <param name="y">The y.</param>
    private void AnimateThrow(double finalX, double y)
    {
        var startX = AbsoluteLayout.GetLayoutBounds(chatBubble).X;

        new Animation(v =>
        {
            var currentX = startX + (finalX - startX) * v;
            AbsoluteLayout.SetLayoutBounds(chatBubble, new Rect(currentX, y, chatBubble.Width, chatBubble.Height));
        })
        .Commit(this, "ThrowAnimation", 16, 500, Easing.CubicOut, finished: (v, c) => SnapToEdge());
    }

    /// <summary>
    /// Snaps to edge.
    /// </summary>
    private void SnapToEdge()
    {
        var bounds = AbsoluteLayout.GetLayoutBounds(chatBubble);
        var minX = 0;
        var maxX = Width - chatBubble.Width;

        var snapTargetX = (bounds.X + chatBubble.Width / 2) > Width / 2
            ? maxX - 10
            : 10;

        new Animation(
            callback: v =>
            {
                var currentX = bounds.X + (snapTargetX - bounds.X) * v;
                AbsoluteLayout.SetLayoutBounds(chatBubble,
                    new Rect(currentX, bounds.Y, chatBubble.Width, chatBubble.Height));
            },
            start: 0,
            end: 1)
        .Commit(
            owner: this,
            name: "SnapAnimation",
            length: 300,
            easing: Easing.SpringOut,
            finished: (v, c) => { },
            repeat: () => false);
    }

    /// <summary>
    /// Updates the anchor points.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    private void UpdateAnchorPoints(double x, double y)
    {
        chatBubble.AnchorX = (x > Width / 2) ? 1 : 0;
        chatBubble.AnchorY = (y > Height / 2) ? 1 : 0;
    }

    /// <summary>
    /// Called when [size allocated].
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0 || height <= 0)
            return;

        if (!_isInitialPositionSet)
        {
            var x = width - CollapsedSize - EdgePadding;
            var y = height - CollapsedSize - EdgePadding;

            AbsoluteLayout.SetLayoutBounds(chatBubble, new Rect(x, y, CollapsedSize, CollapsedSize));
            _isInitialPositionSet = true;
        }

        if (_isExpanded)
        {
            var targetWidth = Math.Min(width * ExpandedWidthPercentage, MaxExpandedWidth);
            var targetHeight = Math.Min(height * ExpandedHeightPercentage, MaxExpandedHeight);

            AbsoluteLayout.SetLayoutBounds(chatBubble,
                new Rect(
                    Math.Max(EdgePadding, Math.Min(
                        AbsoluteLayout.GetLayoutBounds(chatBubble).X,
                        width - targetWidth - EdgePadding)),
                    Math.Max(EdgePadding, Math.Min(
                        AbsoluteLayout.GetLayoutBounds(chatBubble).Y,
                        height - targetHeight - EdgePadding)),
                    targetWidth,
                    targetHeight));
        }
    }
    #endregion
}