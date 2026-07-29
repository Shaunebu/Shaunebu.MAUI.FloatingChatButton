using FloatingChatButton.Models;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FloatingChatButton.Demo;

public abstract class ScenarioPageBase : ContentPage
{
    private readonly Label _isExpandedLabel = CreateDiagnosticLabel();
    private readonly Label _messageCountLabel = CreateDiagnosticLabel();
    private readonly Label _translationLabel = CreateDiagnosticLabel();
    private readonly Label _controlSizeLabel = CreateDiagnosticLabel();
    private readonly Label _pageSizeLabel = CreateDiagnosticLabel();
    private readonly Label _lifecycleLabel = CreateDiagnosticLabel();
    private readonly Label _collectionCallbacksLabel = CreateDiagnosticLabel();
    private readonly Label _loadCyclesLabel = CreateDiagnosticLabel();
    private readonly Label _requestsLabel = CreateDiagnosticLabel();
    private readonly Label _themeLabel = CreateDiagnosticLabel();
    private readonly Label _disposedLabel = CreateDiagnosticLabel();
    private readonly Label _lastOperationLabel = CreateDiagnosticLabel();
    private readonly IDispatcherTimer? _diagnosticTimer;
    private string _currentLifecycle = "Constructed";

    protected ScenarioPageBase(string title, string action, string expected)
    {
        Title = title;
        Messages = new ObservableCollection<ChatMessage>
        {
            new() { Text = "Validation message: incoming", IsIncoming = true },
            new() { Text = "Validation message: outgoing", IsIncoming = false }
        };

        ChatButton = new global::FloatingChatButton.Controls.FloatingChatButton
        {
            AutomationId = $"{GetType().Name}_FloatingChatButton",
            PrimaryColor = Colors.DodgerBlue,
            Messages = Messages
        };

        SemanticProperties.SetDescription(ChatButton, $"{title} floating chat control");
        Messages.CollectionChanged += OnMessagesCollectionChanged;

        ContentArea = new VerticalStackLayout
        {
            Spacing = 12
        };

        RootGrid = new Grid();
        RootGrid.Children.Add(CreateScrollableContent(title, action, expected));
        RootGrid.Children.Add(ChatButton);
        Content = RootGrid;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += (_, _) => RefreshDiagnostics();
        ChatButton.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(global::FloatingChatButton.Controls.FloatingChatButton.IsExpanded)
                or nameof(Width)
                or nameof(Height)
                or nameof(TranslationX)
                or nameof(TranslationY))
            {
                RefreshDiagnostics();
            }
        };

        _diagnosticTimer = Dispatcher.CreateTimer();
        _diagnosticTimer.Interval = TimeSpan.FromMilliseconds(500);
        _diagnosticTimer.Tick += (_, _) => RefreshDiagnostics();
    }

    protected ObservableCollection<ChatMessage> Messages { get; private set; }

    protected global::FloatingChatButton.Controls.FloatingChatButton ChatButton { get; }

    protected VerticalStackLayout ContentArea { get; }

    protected Grid RootGrid { get; }

    protected int CollectionChangeCallbacks { get; private set; }

    protected int LoadCycles { get; private set; }

    protected int ExpandCollapseRequests { get; private set; }

    protected bool IsDisposedForScenario { get; set; }

    protected string LastOperation
    {
        get => _lastOperationLabel.Text;
        set
        {
            _lastOperationLabel.Text = $"Last operation: {value}";
            RefreshDiagnostics();
        }
    }

    protected void AddAction(string text, Action action, string? automationId = null)
    {
        var button = new Button
        {
            Text = text,
            AutomationId = automationId ?? $"{GetType().Name}_{text.Replace(" ", string.Empty)}",
            HorizontalOptions = LayoutOptions.Fill
        };
        SemanticProperties.SetDescription(button, text);
        button.Clicked += (_, _) =>
        {
            action();
            RefreshDiagnostics();
        };
        ContentArea.Children.Add(button);
    }

    protected void AddAction(string text, Func<Task> action, string? automationId = null)
    {
        var button = new Button
        {
            Text = text,
            AutomationId = automationId ?? $"{GetType().Name}_{text.Replace(" ", string.Empty)}",
            HorizontalOptions = LayoutOptions.Fill
        };
        SemanticProperties.SetDescription(button, text);
        button.Clicked += async (_, _) =>
        {
            await action();
            RefreshDiagnostics();
        };
        ContentArea.Children.Add(button);
    }

    protected void RequestExpanded(bool isExpanded)
    {
        ExpandCollapseRequests++;
        ChatButton.IsExpanded = isExpanded;
        LastOperation = $"Requested IsExpanded={isExpanded}";
    }

    protected void ToggleExpanded()
    {
        RequestExpanded(!ChatButton.IsExpanded);
    }

    protected void ReplaceMessages(ObservableCollection<ChatMessage>? messages, string ownerName)
    {
        Messages.CollectionChanged -= OnMessagesCollectionChanged;
        ChatButton.Messages = messages!;
        Messages = ChatButton.Messages;
        Messages.CollectionChanged += OnMessagesCollectionChanged;
        LastOperation = $"Messages owner: {ownerName}; count={Messages.Count}";
    }

    protected Label AddStatusLabel(string initialText, string automationId)
    {
        var label = new Label
        {
            Text = initialText,
            AutomationId = automationId,
            FontAttributes = FontAttributes.Bold
        };
        ContentArea.Children.Add(label);
        return label;
    }

    protected void AddSectionTitle(string text)
    {
        ContentArea.Children.Add(new Label
        {
            Text = text,
            FontAttributes = FontAttributes.Bold,
            FontSize = 18
        });
    }

    protected void RefreshDiagnostics()
    {
        _isExpandedLabel.Text = $"IsExpanded: {ChatButton.IsExpanded}";
        _messageCountLabel.Text = $"Message count: {ChatButton.Messages?.Count ?? 0}";
        _translationLabel.Text = $"Control translation: X={ChatButton.TranslationX:0.##}, Y={ChatButton.TranslationY:0.##}";
        _controlSizeLabel.Text = $"Control size: W={ChatButton.Width:0.##}, H={ChatButton.Height:0.##}";
        _pageSizeLabel.Text = $"Page size: W={Width:0.##}, H={Height:0.##}";
        _lifecycleLabel.Text = $"Lifecycle event: {_currentLifecycle}";
        _collectionCallbacksLabel.Text = $"Collection-change callbacks: {CollectionChangeCallbacks}";
        _loadCyclesLabel.Text = $"Load/unload cycles: {LoadCycles}";
        _requestsLabel.Text = $"Expand/collapse requests: {ExpandCollapseRequests}";
        _themeLabel.Text = $"Theme: {Application.Current?.RequestedTheme}";
        _disposedLabel.Text = $"Disposed in scenario: {IsDisposedForScenario}";
    }

    private View CreateScrollableContent(string title, string action, string expected)
    {
        var layout = new VerticalStackLayout
        {
            Padding = new Thickness(20, 16, 20, 120),
            Spacing = 14
        };

        var titleLabel = new Label
        {
            Text = title,
            FontAttributes = FontAttributes.Bold,
            FontSize = 24
        };
        SemanticProperties.SetHeadingLevel(titleLabel, SemanticHeadingLevel.Level1);
        layout.Children.Add(titleLabel);
        layout.Children.Add(CreateInstruction("Action", action));
        layout.Children.Add(CreateInstruction("Expected", expected));
        layout.Children.Add(CreatePlatformInfo());
        layout.Children.Add(CreateDiagnostics());
        layout.Children.Add(ContentArea);

        return new ScrollView { Content = layout };
    }

    private static View CreateInstruction(string heading, string text)
    {
        return new Border
        {
            Padding = 12,
            Stroke = Colors.LightGray,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Label { Text = heading, FontAttributes = FontAttributes.Bold },
                    new Label { Text = text }
                }
            }
        };
    }

    private View CreateDiagnostics()
    {
        var layout = new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label { Text = "Diagnostics", FontAttributes = FontAttributes.Bold, FontSize = 18 },
                _isExpandedLabel,
                _messageCountLabel,
                _translationLabel,
                _controlSizeLabel,
                _pageSizeLabel,
                _lifecycleLabel,
                _collectionCallbacksLabel,
                _loadCyclesLabel,
                _requestsLabel,
                _themeLabel,
                _disposedLabel,
                _lastOperationLabel
            }
        };

        _lastOperationLabel.Text = "Last operation: none";
        return layout;
    }

    private static View CreatePlatformInfo()
    {
        var display = DeviceDisplay.Current.MainDisplayInfo;
        var appVersion = AppInfo.Current.VersionString;
        var orientation = display.Orientation;
        var text =
            $"Platform: {DeviceInfo.Current.Platform}\n" +
            $"OS version: {DeviceInfo.Current.VersionString}\n" +
            $"App version: {appVersion}\n" +
            $"Device idiom: {DeviceInfo.Current.Idiom}\n" +
            $"Display density: {display.Density:0.##}\n" +
            $"Display dimensions: {display.Width:0} x {display.Height:0}\n" +
            $"Orientation: {orientation}\n" +
            $"Theme: {Application.Current?.RequestedTheme}";

        return new Border
        {
            Padding = 12,
            Stroke = Colors.LightGray,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new Label
            {
                Text = text,
                AutomationId = "PlatformInfo"
            }
        };
    }

    private static Label CreateDiagnosticLabel()
    {
        return new Label { FontSize = 13 };
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        LoadCycles++;
        _currentLifecycle = "Loaded";
        _diagnosticTimer?.Start();
        RefreshDiagnostics();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        LoadCycles++;
        _currentLifecycle = "Unloaded";
        _diagnosticTimer?.Stop();
        RefreshDiagnostics();
    }

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChangeCallbacks++;
        RefreshDiagnostics();
    }
}
