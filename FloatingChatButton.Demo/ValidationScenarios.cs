using FloatingChatButton.Models;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;

namespace FloatingChatButton.Demo;

public sealed class BasicExpandCollapsePage : ScenarioPageBase
{
    public BasicExpandCollapsePage()
        : base(
            "Basic Expand and Collapse",
            "Tap the floating button, use these buttons, and tap the overlay while expanded.",
            "The chat opens and closes without a stuck overlay. IsExpanded matches the requested state.")
    {
        AddAction("Request expand", () => RequestExpanded(true));
        AddAction("Request collapse", () => RequestExpanded(false));
        AddAction("Toggle requested state", ToggleExpanded);
        AddAction("Add visible message", () =>
        {
            Messages.Add(new ChatMessage { Text = $"Basic message {Messages.Count + 1}", IsIncoming = Messages.Count % 2 == 0 });
            LastOperation = "Added a message";
        });
    }
}

public sealed class RapidTransitionPage : ScenarioPageBase
{
    private readonly Label _requestedFinalState;

    public RapidTransitionPage()
        : base(
            "Rapid Transition Interruption",
            "Use the rapid request buttons, then compare requested final state with IsExpanded. Rotate the device during a transition for manual validation.",
            "The latest request wins; older animation completions do not restore a stale state.")
    {
        _requestedFinalState = AddStatusLabel("Requested final state: none", "RapidRequestedFinalState");
        AddAction("Ten rapid expand/collapse requests", () =>
        {
            for (var i = 0; i < 10; i++)
            {
                RequestExpanded(i % 2 == 0);
            }

            _requestedFinalState.Text = "Requested final state: collapse";
            RequestExpanded(false);
        });
        AddAction("Expand immediately followed by collapse", () =>
        {
            RequestExpanded(true);
            RequestExpanded(false);
            _requestedFinalState.Text = "Requested final state: collapse";
        });
        AddAction("Collapse immediately followed by expand", () =>
        {
            RequestExpanded(false);
            RequestExpanded(true);
            _requestedFinalState.Text = "Requested final state: expand";
        });
        AddAction("Repeated overlay-tap equivalent", () =>
        {
            for (var i = 0; i < 5; i++)
            {
                RequestExpanded(false);
            }

            _requestedFinalState.Text = "Requested final state: collapse";
        });
        AddAction("Prepare for orientation-change test", () =>
        {
            RequestExpanded(true);
            _requestedFinalState.Text = "Requested final state: expand; rotate now";
        });
    }
}

public class DragBoundsPage : ScenarioPageBase
{
    private readonly Grid _boundedHost = new()
    {
        BackgroundColor = Color.FromArgb("#F2F4F7"),
        HeightRequest = 520,
        WidthRequest = 340,
        HorizontalOptions = LayoutOptions.Start
    };
    private readonly Label _boundsStatus;

    public DragBoundsPage()
        : this(
            "Dragging and Edge Snapping",
            "In the fixed drag surface, drag the collapsed button upward, downward, diagonally, and horizontally. Release near the left and right edges. Scroll the instructions/actions area by swiping outside the drag surface, then navigate away or rotate during a drag and repeat after returning.",
            "Vertical and diagonal drags move the control; horizontal drags still snap to the nearest horizontal edge. Scrolling outside the drag surface continues to work, and interaction recovers after completion, cancellation, rotation, or navigation.")
    {
    }

    protected DragBoundsPage(string title, string action, string expected, double? initialWidth = null, double? initialHeight = null)
        : base(
            title,
            action,
            expected)
    {
        RootGrid.Children.Remove(ChatButton);
        RootGrid.RowDefinitions.Clear();
        RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

        if (RootGrid.Children.FirstOrDefault(child => child is ScrollView) is View scenarioContent)
        {
            Grid.SetRow(scenarioContent, 1);
        }

        _boundedHost.Children.Add(ChatButton);

        var fixedDragSurface = new Border
        {
            Padding = 0,
            Stroke = Colors.DarkGray,
            StrokeThickness = 2,
            Content = _boundedHost
        };
        Grid.SetRow(fixedDragSurface, 0);
        RootGrid.Children.Add(fixedDragSurface);

        _boundsStatus = AddStatusLabel("Bounds status: not checked", "DragBoundsStatus");

        AddSectionTitle("Container presets");
        AddAction("Very small", () => SetContainerSize(120, 120));
        AddAction("Phone portrait", () => SetContainerSize(340, 560));
        AddAction("Phone landscape", () => SetContainerSize(560, 300));
        AddAction("Tablet", () => SetContainerSize(720, 720));
        AddAction("Control larger than host", () => SetContainerSize(48, 48));
        AddAction("Check public bounds diagnostics", CheckBounds);

        if (initialWidth.HasValue && initialHeight.HasValue)
        {
            SetContainerSize(initialWidth.Value, initialHeight.Value);
        }
    }

    protected void SetContainerSize(double width, double height)
    {
        _boundedHost.WidthRequest = width;
        _boundedHost.HeightRequest = height;
        LastOperation = $"Container set to {width:0} x {height:0}";
        CheckBounds();
    }

    protected void CheckBounds()
    {
        var finite = double.IsFinite(ChatButton.TranslationX)
            && double.IsFinite(ChatButton.TranslationY)
            && double.IsFinite(ChatButton.Width)
            && double.IsFinite(ChatButton.Height);
        _boundsStatus.Text = $"Bounds status: public metrics finite={finite}; host={_boundedHost.WidthRequest:0}x{_boundedHost.HeightRequest:0}; inner bubble coordinates are intentionally not public.";
    }
}

public sealed class KeyboardMessagePage : ScenarioPageBase
{
    public KeyboardMessagePage()
        : base(
            "Keyboard and Message Sending",
            "Expand the chat, focus its entry, test normal, whitespace, whitespace-only, very long, and rapid send operations.",
            "Whitespace-only input is ignored. Normal text is trimmed, added as outgoing, and the entry clears.")
    {
        AddAction("Expand for keyboard test", () => RequestExpanded(true));
        AddAction("Add programmatic incoming message", () =>
        {
            Messages.Add(new ChatMessage { Text = $"Incoming {Messages.Count + 1}", IsIncoming = true });
            LastOperation = "Added programmatic incoming message";
        });
        AddAction("Add very long message", () =>
        {
            Messages.Add(new ChatMessage { Text = new string('L', 500), IsIncoming = false });
            LastOperation = "Added very long outgoing message";
        });
        AddAction("Rapid programmatic sends", () =>
        {
            for (var i = 0; i < 10; i++)
            {
                Messages.Add(new ChatMessage { Text = $"Rapid send {i + 1}", IsIncoming = false });
            }

            LastOperation = "Added 10 rapid outgoing messages";
        });
    }
}

public sealed class ProgrammaticMessagesPage : ScenarioPageBase
{
    public ProgrammaticMessagesPage()
        : base(
            "Programmatic Message Insertion",
            "Use buttons to add incoming, outgoing, and batches of messages without using the built-in input.",
            "Message count increments once per add and CollectionView displays incoming/outgoing styles.")
    {
        AddAction("Add incoming", () =>
        {
            Messages.Add(new ChatMessage { Text = $"Incoming {Messages.Count + 1}", IsIncoming = true });
            LastOperation = "Added incoming";
        });
        AddAction("Add outgoing", () =>
        {
            Messages.Add(new ChatMessage { Text = $"Outgoing {Messages.Count + 1}", IsIncoming = false });
            LastOperation = "Added outgoing";
        });
        AddAction("Add 25 alternating messages", () =>
        {
            for (var i = 0; i < 25; i++)
            {
                Messages.Add(new ChatMessage { Text = $"Batch {i + 1}", IsIncoming = i % 2 == 0 });
            }

            LastOperation = "Added 25 messages";
        });
        AddAction("Clear messages", () =>
        {
            Messages.Clear();
            LastOperation = "Cleared messages";
        });
    }
}

public sealed class CollectionReplacementPage : ScenarioPageBase
{
    private ObservableCollection<ChatMessage> _oldCollection = new();
    private ObservableCollection<ChatMessage> _newCollection = new();
    private readonly Label _ownerLabel;

    public CollectionReplacementPage()
        : base(
            "Message Collection Replacement",
            "Replace the message collection, mutate old and new collections, and assign null.",
            "Only the current collection changes the UI; old collection mutations do not update the control.")
    {
        _oldCollection = Messages;
        _newCollection = Messages;
        _ownerLabel = AddStatusLabel("Current owner: initial", "CollectionOwner");
        AddAction("Replace with new empty collection", () =>
        {
            _oldCollection = Messages;
            _newCollection = new ObservableCollection<ChatMessage>();
            ReplaceMessages(_newCollection, "new empty collection");
            _ownerLabel.Text = "Current owner: new empty collection";
        });
        AddAction("Replace with populated collection", () =>
        {
            _oldCollection = Messages;
            _newCollection = new ObservableCollection<ChatMessage>
            {
                new() { Text = "Replacement incoming", IsIncoming = true },
                new() { Text = "Replacement outgoing", IsIncoming = false }
            };
            ReplaceMessages(_newCollection, "populated collection");
            _ownerLabel.Text = "Current owner: populated collection";
        });
        AddAction("Mutate old collection", () =>
        {
            _oldCollection.Add(new ChatMessage { Text = "Old mutation", IsIncoming = true });
            LastOperation = $"Mutated old collection; UI owner count should remain {Messages.Count}";
        });
        AddAction("Mutate current/new collection", () =>
        {
            _newCollection.Add(new ChatMessage { Text = "New mutation", IsIncoming = false });
            LastOperation = "Mutated current/new collection";
        });
        AddAction("Assign null collection", () =>
        {
            _oldCollection = Messages;
            ReplaceMessages(null, "null coerced by control");
            _newCollection = Messages;
            _ownerLabel.Text = "Current owner: null assignment coerced to empty collection";
        });
    }
}

public sealed class MultipleInstancesPage : ContentPage
{
    private readonly ObservableCollection<ChatMessage> _leftMessages = new();
    private readonly ObservableCollection<ChatMessage> _rightMessages = new();
    private readonly Label _leftCount = new();
    private readonly Label _rightCount = new();
    private readonly global::FloatingChatButton.Controls.FloatingChatButton _leftButton;
    private readonly global::FloatingChatButton.Controls.FloatingChatButton _rightButton;

    public MultipleInstancesPage()
    {
        Title = "Multiple Independent Instances";
        _leftButton = CreateButton("LeftChatButton", Colors.DodgerBlue, _leftMessages);
        _rightButton = CreateButton("RightChatButton", Colors.SeaGreen, _rightMessages);
        _leftMessages.CollectionChanged += (_, _) => Refresh();
        _rightMessages.CollectionChanged += (_, _) => Refresh();

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };

        grid.Add(CreatePane("Blue instance", _leftCount, _leftMessages, "Left"), 0, 0);
        grid.Add(CreatePane("Green instance", _rightCount, _rightMessages, "Right"), 1, 0);
        grid.Add(_leftButton, 0, 0);
        grid.Add(_rightButton, 1, 0);

        Content = grid;
        Refresh();
    }

    private static global::FloatingChatButton.Controls.FloatingChatButton CreateButton(string automationId, Color color, ObservableCollection<ChatMessage> messages)
    {
        return new global::FloatingChatButton.Controls.FloatingChatButton
        {
            AutomationId = automationId,
            PrimaryColor = color,
            Messages = messages
        };
    }

    private View CreatePane(string title, Label countLabel, ObservableCollection<ChatMessage> messages, string prefix)
    {
        var layout = new VerticalStackLayout
        {
            Padding = 14,
            Spacing = 10,
            Children =
            {
                new Label { Text = title, FontAttributes = FontAttributes.Bold, FontSize = 20 },
                new Label { Text = "Add messages to one control and verify the other count does not change." },
                countLabel
            }
        };

        var add = new Button { Text = $"Add {prefix} message", AutomationId = $"Add{prefix}Message" };
        add.Clicked += (_, _) => messages.Add(new ChatMessage { Text = $"{prefix} {messages.Count + 1}", IsIncoming = messages.Count % 2 == 0 });
        layout.Children.Add(add);

        var toggle = new Button { Text = $"Toggle {prefix}", AutomationId = $"Toggle{prefix}" };
        toggle.Clicked += (_, _) =>
        {
            if (prefix == "Left")
            {
                _leftButton.IsExpanded = !_leftButton.IsExpanded;
            }
            else
            {
                _rightButton.IsExpanded = !_rightButton.IsExpanded;
            }

            Refresh();
        };
        layout.Children.Add(toggle);

        return new Border { Padding = 6, Content = layout };
    }

    private void Refresh()
    {
        _leftCount.Text = $"Left count: {_leftMessages.Count}; IsExpanded: {_leftButton.IsExpanded}";
        _rightCount.Text = $"Right count: {_rightMessages.Count}; IsExpanded: {_rightButton.IsExpanded}";
    }
}

public sealed class NavigationLifecyclePage : ScenarioPageBase
{
    private readonly Label _navigationStatus;
    private int _pagesCreated;
    private int _pagesReturned;
    private int _targetCallbacks;

    public NavigationLifecyclePage()
        : base(
            "Navigation Lifecycle",
            "Open the target page, add messages, expand/collapse, return, and repeat.",
            "One callback per collection change; no stale callbacks or duplicate message additions after returning.")
    {
        _navigationStatus = AddStatusLabel("Navigation status: no target page opened", "NavigationStatus");
        AddAction("Open lifecycle target page", async () =>
        {
            _pagesCreated++;
            await Shell.Current.Navigation.PushAsync(new LifecycleTargetPage(callbacks =>
            {
                _targetCallbacks += callbacks;
                _pagesReturned++;
                _navigationStatus.Text = $"Pages created={_pagesCreated}; returned={_pagesReturned}; target callbacks={_targetCallbacks}";
            }));
        });
    }
}

public sealed class LifecycleTargetPage : ScenarioPageBase
{
    private readonly Action<int> _onReturn;

    public LifecycleTargetPage(Action<int> onReturn)
        : base(
            "Lifecycle Target",
            "Add messages, expand/collapse, then use Back to return to the lifecycle scenario page.",
            "Returning should not leave callbacks from this page active.")
    {
        _onReturn = onReturn;
        AddAction("Add target message", () =>
        {
            Messages.Add(new ChatMessage { Text = $"Target {Messages.Count + 1}", IsIncoming = false });
            LastOperation = "Added target message";
        });
        AddAction("Expand", () => RequestExpanded(true));
        AddAction("Collapse", () => RequestExpanded(false));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _onReturn(CollectionChangeCallbacks);
    }
}

public sealed class ThemeChangesPage : ScenarioPageBase
{
    public ThemeChangesPage()
        : base(
            "Theme Changes",
            "Switch requested app theme and verify text, bubbles, overlay, and controls remain legible.",
            "No control relies on color alone; contrast remains usable in light and dark theme.")
    {
        AddAction("Request light theme", () =>
        {
            Application.Current!.UserAppTheme = AppTheme.Light;
            LastOperation = "Requested light theme";
        });
        AddAction("Request dark theme", () =>
        {
            Application.Current!.UserAppTheme = AppTheme.Dark;
            LastOperation = "Requested dark theme";
        });
        AddAction("Use system theme", () =>
        {
            Application.Current!.UserAppTheme = AppTheme.Unspecified;
            LastOperation = "Requested system theme";
        });
    }
}

public sealed class AccessibilityTextPage : ScenarioPageBase
{
    public AccessibilityTextPage()
        : base(
            "Large Font and Accessibility Text",
            "Enable large accessibility text, TalkBack, or VoiceOver and navigate through every visible control.",
            "Labels, hints, and buttons remain clear. The test does not rely on color alone.")
    {
        AddSectionTitle("Accessibility instructions");
        ContentArea.Children.Add(new Label
        {
            Text = "TalkBack: swipe to the floating chat button, double tap to open, navigate to the entry and send button.\nVoiceOver: swipe through the same elements and verify labels and hints.",
            AutomationId = "AccessibilityInstructions",
            FontSize = 22
        });
        AddAction("Expand for screen-reader test", () => RequestExpanded(true));
        AddAction("Add readable incoming message", () =>
        {
            Messages.Add(new ChatMessage { Text = "Screen reader validation message", IsIncoming = true });
            LastOperation = "Added screen-reader validation message";
        });
    }
}

public sealed class SmallContainerBoundsPage : DragBoundsPage
{
    public SmallContainerBoundsPage()
        : base(
            "Small-Container Bounds",
            "Drag and expand the control inside intentionally tiny containers, including a host smaller than the collapsed control.",
            "Public metrics remain finite and the control stays as reachable as the host permits.",
            90,
            90)
    {
        LastOperation = "Started with a 90 x 90 container";
    }
}

public sealed class LongMessageCollectionPage : ScenarioPageBase
{
    public LongMessageCollectionPage()
        : base(
            "Long Message Collections",
            "Add large batches and long text, expand the chat, and scroll through the list.",
            "The UI remains responsive enough for manual validation and does not duplicate collection callbacks.")
    {
        AddAction("Add 100 messages", () =>
        {
            for (var i = 0; i < 100; i++)
            {
                Messages.Add(new ChatMessage { Text = $"Long collection item {Messages.Count + 1}", IsIncoming = i % 2 == 0 });
            }

            LastOperation = "Added 100 messages";
        });
        AddAction("Add 500-character message", () =>
        {
            Messages.Add(new ChatMessage { Text = new string('A', 500), IsIncoming = false });
            LastOperation = "Added 500-character message";
        });
        AddAction("Clear collection", () =>
        {
            Messages.Clear();
            LastOperation = "Cleared long collection";
        });
    }
}

public sealed class DisposalBehaviorPage : ContentPage
{
    private readonly Grid _host = new();
    private readonly Label _status = new() { AutomationId = "DisposalStatus" };
    private readonly ObservableCollection<ChatMessage> _messages = new();
    private global::FloatingChatButton.Controls.FloatingChatButton? _control;
    private bool _disposed;

    public DisposalBehaviorPage()
    {
        Title = "Disposal Behavior";
        var layout = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 12,
            Children =
            {
                new Label { Text = "Disposal Behavior", FontAttributes = FontAttributes.Bold, FontSize = 24 },
                new Label { Text = "This scenario removes a manually-created control from the visual tree before disposing it. Do not reuse a disposed control." },
                _status,
                CreateButton("Create control", CreateControl),
                CreateButton("Add message before dispose", AddMessage),
                CreateButton("Dispose control", DisposeControl),
                CreateButton("Dispose again", DisposeControl),
                _host
            }
        };

        Content = layout;
        CreateControl();
    }

    private static Button CreateButton(string text, Action action)
    {
        var button = new Button { Text = text, AutomationId = $"Disposal_{text.Replace(" ", string.Empty)}" };
        button.Clicked += (_, _) => action();
        return button;
    }

    private void CreateControl()
    {
        if (_control is not null && !_disposed)
        {
            _status.Text = "Status: active control already exists";
            return;
        }

        _messages.Clear();
        _control = new global::FloatingChatButton.Controls.FloatingChatButton
        {
            AutomationId = "DisposableFloatingChatButton",
            Messages = _messages,
            PrimaryColor = Colors.Purple
        };
        _disposed = false;
        _host.Children.Clear();
        _host.Children.Add(_control);
        _status.Text = "Status: control created";
    }

    private void AddMessage()
    {
        _messages.Add(new ChatMessage { Text = $"Before dispose {_messages.Count + 1}", IsIncoming = false });
        _status.Text = $"Status: message added; count={_messages.Count}";
    }

    private void DisposeControl()
    {
        if (_control is null)
        {
            _status.Text = "Status: no control to dispose";
            return;
        }

        _host.Children.Remove(_control);
        _control.Dispose();
        _disposed = true;
        _status.Text = "Status: disposed; second dispose should be harmless";
    }
}

public sealed class MemoryLeakNavigationLoopPage : ContentPage
{
    private readonly Label _status = new() { AutomationId = "MemoryLoopStatus" };
    private readonly List<WeakReference> _pageReferences = new();
    private readonly List<WeakReference> _controlReferences = new();

    public MemoryLeakNavigationLoopPage()
    {
        Title = "Memory-Leak Navigation Loop";
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 12,
                Children =
                {
                    new Label { Text = "Memory-Leak Navigation Loop", FontAttributes = FontAttributes.Bold, FontSize = 24 },
                    new Label { Text = "Weak references are diagnostic assistance only. This does not prove absence of leaks." },
                    _status,
                    CreateButton("Navigate forward and back once", async () => await RunLoopAsync(1)),
                    CreateButton("Repeat navigation 10 times", async () => await RunLoopAsync(10)),
                    CreateButton("Repeat navigation 25 times", async () => await RunLoopAsync(25)),
#if DEBUG
                    CreateButton("Force best-effort GC", () =>
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        RefreshStatus("Forced debug GC");
                    }),
#endif
                    CreateButton("Refresh weak-reference status", () => RefreshStatus("Refreshed"))
                }
            }
        };
        RefreshStatus("Ready");
    }

    private static Button CreateButton(string text, Func<Task> action)
    {
        var button = new Button { Text = text, AutomationId = $"Memory_{text.Replace(" ", string.Empty)}" };
        button.Clicked += async (_, _) => await action();
        return button;
    }

    private static Button CreateButton(string text, Action action)
    {
        var button = new Button { Text = text, AutomationId = $"Memory_{text.Replace(" ", string.Empty)}" };
        button.Clicked += (_, _) => action();
        return button;
    }

    private async Task RunLoopAsync(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var page = new MemoryProbePage();
            _pageReferences.Add(new WeakReference(page));
            _controlReferences.Add(page.ControlReference);
            await Shell.Current.Navigation.PushAsync(page);
            await Task.Delay(80);
            await Shell.Current.Navigation.PopAsync();
            await Task.Delay(80);
        }

        RefreshStatus($"Completed {count} navigation cycle(s)");
    }

    private void RefreshStatus(string operation)
    {
        var livePages = _pageReferences.Count(reference => reference.IsAlive);
        var liveControls = _controlReferences.Count(reference => reference.IsAlive);
        _status.Text = $"{operation}; tracked pages={_pageReferences.Count}; live page refs={livePages}; live control refs={liveControls}";
    }
}

public sealed class MemoryProbePage : ContentPage
{
    private readonly global::FloatingChatButton.Controls.FloatingChatButton _control = new()
    {
        AutomationId = "MemoryProbeFloatingChatButton",
        Messages = new ObservableCollection<ChatMessage>
        {
            new() { Text = "Probe", IsIncoming = true }
        }
    };

    public MemoryProbePage()
    {
        Title = "Memory Probe";
        ControlReference = new WeakReference(_control);
        Content = new Grid
        {
            Children =
            {
                new Label
                {
                    Text = "Temporary page used by the memory loop.",
                    Margin = 20
                },
                _control
            }
        };
    }

    public WeakReference ControlReference { get; }
}

public sealed class OrientationResizingPage : ScenarioPageBase
{
    public OrientationResizingPage()
        : base(
            "Orientation and Layout Resizing",
            "Rotate the device while collapsed and expanded. Also try split view or resizeable windows where available.",
            "The button remains visible, and expanded dimensions are recalculated for the new page size.")
    {
        AddAction("Expand before rotation", () => RequestExpanded(true));
        AddAction("Collapse before rotation", () => RequestExpanded(false));
        AddAction("Add message after resize", () =>
        {
            Messages.Add(new ChatMessage { Text = $"Resize validation {Messages.Count + 1}", IsIncoming = true });
            LastOperation = "Added message after resize";
        });
    }
}
