using Microsoft.Maui.Controls.Shapes;

namespace FloatingChatButton.Demo;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        Title = "Validation Dashboard";
        Content = CreateDashboard();
    }

    private View CreateDashboard()
    {
        var layout = new VerticalStackLayout
        {
            Padding = new Thickness(20, 16, 20, 40),
            Spacing = 12
        };

        layout.Children.Add(new Label
        {
            Text = "FloatingChatButton Runtime Validation",
            FontAttributes = FontAttributes.Bold,
            FontSize = 24
        });

        layout.Children.Add(new Label
        {
            Text = "Open each scenario, perform the listed actions, and record the result in the Android or iOS checklist. Runtime validation is not marked as passed automatically.",
            AutomationId = "DashboardInstructions"
        });

        layout.Children.Add(CreatePlatformInfo());

        AddScenario(layout, "1. Basic expand and collapse", () => new BasicExpandCollapsePage());
        AddScenario(layout, "2. Rapid transition interruption", () => new RapidTransitionPage());
        AddScenario(layout, "3. Dragging and edge snapping", () => new DragBoundsPage());
        AddScenario(layout, "4. Keyboard and message sending", () => new KeyboardMessagePage());
        AddScenario(layout, "5. Programmatic message insertion", () => new ProgrammaticMessagesPage());
        AddScenario(layout, "6. Message collection replacement", () => new CollectionReplacementPage());
        AddScenario(layout, "7. Multiple independent control instances", () => new MultipleInstancesPage());
        AddScenario(layout, "8. Navigation lifecycle", () => new NavigationLifecyclePage());
        AddScenario(layout, "9. Theme changes", () => new ThemeChangesPage());
        AddScenario(layout, "10. Large font and accessibility text", () => new AccessibilityTextPage());
        AddScenario(layout, "11. Small-container bounds", () => new SmallContainerBoundsPage());
        AddScenario(layout, "12. Long message collections", () => new LongMessageCollectionPage());
        AddScenario(layout, "13. Disposal behavior", () => new DisposalBehaviorPage());
        AddScenario(layout, "14. Memory-leak navigation loop", () => new MemoryLeakNavigationLoopPage());
        AddScenario(layout, "15. Orientation and layout resizing", () => new OrientationResizingPage());

        return new ScrollView { Content = layout };
    }

    private static View CreatePlatformInfo()
    {
        var display = DeviceDisplay.Current.MainDisplayInfo;
        return new Border
        {
            Padding = 12,
            Stroke = Colors.LightGray,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new Label
            {
                AutomationId = "DashboardPlatformInfo",
                Text =
                    $"Platform: {DeviceInfo.Current.Platform}\n" +
                    $"OS version: {DeviceInfo.Current.VersionString}\n" +
                    $"App version: {AppInfo.Current.VersionString}\n" +
                    $"Device idiom: {DeviceInfo.Current.Idiom}\n" +
                    $"Display density: {display.Density:0.##}\n" +
                    $"Display dimensions: {display.Width:0} x {display.Height:0}\n" +
                    $"Orientation: {display.Orientation}\n" +
                    $"Theme: {Application.Current?.RequestedTheme}"
            }
        };
    }

    private static void AddScenario(VerticalStackLayout layout, string title, Func<Page> pageFactory)
    {
        var button = new Button
        {
            Text = title,
            AutomationId = $"Open_{title.Split('.')[0]}",
            HorizontalOptions = LayoutOptions.Fill
        };
        SemanticProperties.SetDescription(button, $"Open scenario {title}");
        button.Clicked += async (_, _) => await Shell.Current.Navigation.PushAsync(pageFactory());
        layout.Children.Add(button);
    }
}
