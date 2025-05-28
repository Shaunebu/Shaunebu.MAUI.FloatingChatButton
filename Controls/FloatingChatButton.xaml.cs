using FloatingChatButton.Extensions;
using FloatingChatButton.Models;
using System.Collections.ObjectModel;

namespace FloatingChatButton.Controls;

public partial class FloatingChatButton : ContentView
{
    private bool _isDragging;
    private Point _lastPosition;
    private bool _isExpanded = false;
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

    public static readonly BindableProperty MessagesProperty =
        BindableProperty.Create(nameof(Messages), typeof(ObservableCollection<ChatMessage>), typeof(FloatingChatButton), new ObservableCollection<ChatMessage>());

    public static readonly BindableProperty PrimaryColorProperty =
        BindableProperty.Create(nameof(PrimaryColor), typeof(Color), typeof(FloatingChatButton), Colors.Blue);

    public ObservableCollection<ChatMessage> Messages
    {
        get => (ObservableCollection<ChatMessage>)GetValue(MessagesProperty);
        set => SetValue(MessagesProperty, value);
    }

    public Color PrimaryColor
    {
        get => (Color)GetValue(PrimaryColorProperty);
        set => SetValue(PrimaryColorProperty, value);
    }

    public FloatingChatButton()
    {
        InitializeComponent();
        InitializeGestures();
    }

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
    }

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

    private void ToggleBubble()
    {
        if (_isExpanded)
        {
            CollapseBubble();
        }
        else
        {
            ExpandBubble();
        }
    }

    private async void ExpandBubble()
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

        _isExpanded = true;
        bubbleContent.IsVisible = true;
    }

    private async void CollapseBubble()
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

        _isExpanded = false;
        bubbleContent.IsVisible = false;
    }

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

    private void UpdateAnchorPoints(double x, double y)
    {
        chatBubble.AnchorX = (x > Width / 2) ? 1 : 0;
        chatBubble.AnchorY = (y > Height / 2) ? 1 : 0;
    }

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
}