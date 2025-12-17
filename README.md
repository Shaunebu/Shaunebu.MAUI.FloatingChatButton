# 🚀 FloatingChatButton for .NET MAUI

![NuGet Version](https://img.shields.io/nuget/v/Shaunebu.MAUI.Controls.FloatingChatButton?color=blue&label=NuGet)
![Platform Support](https://img.shields.io/badge/platforms-Android|iOS-lightgrey)
![MAUI Version](https://img.shields.io/badge/.NET%20MAUI-%3E%3D9.0-blueviolet)

A fully customizable floating chat button component for .NET MAUI applications with built-in messaging UI, message sending capabilities, and smooth animations.

## 📦 Installation
```
dotnet add package Shaunebu.MAUI.Controls.FloatingChatButton
```

🎯 Features
-----------

*   **Drag-and-drop** with edge snapping behavior

*   **Smooth expand/collapse** animations (spring physics)
    
*   **Fully bindable** properties (MVVM compatible)
    
*   **Customizable** colors, icons and sizing
    
*   **Optimized performance** (60 FPS animations)
    
*   **Built-in chat UI** with message templates
    

🚀 Basic Usage
--------------

1.  Add the namespace:
```
xmlns:fc="clr-namespace:Shaunebu.MAUI.Controls;assembly=Shaunebu.MAUI.Controls.FloatingChatButton"
```

2.  Add the control:
```xaml
<fc:FloatingChatButton
    x:Name="chatButton"
    PrimaryColor="#2196F3"
    BotIcon="chat_icon.png"
    MessageSentCommand="{Binding SendMessageCommand}">
</fc:FloatingChatButton>
```

3.  Handle message sending in your ViewModel or Code-Behind:

### XAML (Code-Behind Approach)
```csharp
public partial class MainPage : ContentPage
{
    public ICommand MessageSentCommand { get; }

    public MainPage()
    {
        MessageSentCommand = new Command<string>(OnMessageSent);
        InitializeComponent();
        BindingContext = this;
        
        chatButton.Messages = new ObservableCollection<ChatMessage>
        {
            new() { Text = "Hello! How can I help you?", IsIncoming = true }
        };
    }

    private async void OnMessageSent(string message)
    {
        // Process the message (e.g., send to API, chatbot)
        var response = await GetBotResponseAsync(message);
        
        // Add bot's response to the chat
        chatButton.Messages.Add(new ChatMessage
        {
            Text = response,
            IsIncoming = true
        });
    }
}
```

### MVVM Approach
```csharp
public class ChatViewModel : ObservableObject
{
    public ObservableCollection<ChatMessage> Messages { get; } = new();
    public ICommand MessageSentCommand { get; }

    public ChatViewModel()
    {
        MessageSentCommand = new AsyncRelayCommand<string>(OnMessageSentAsync);
        Messages.Add(new() { Text = "Hello! How can I help?", IsIncoming = true });
    }

    private async Task OnMessageSentAsync(string message)
    {
        // Call your API or chatbot service
        var response = await _chatService.SendMessageAsync(message);
        
        // Add response to UI
        Messages.Add(new ChatMessage
        {
            Text = response,
            IsIncoming = true
        });
    }
}
```

⚙️ Core Properties
------------------

| Property | Type | Description | Default |
| --- | --- | --- | --- |
| `PrimaryColor` | Color | Button accent color | `#2196F3` |
| `Messages` | `ObservableCollection<ChatMessage>` | Chat messages | Empty |
| `IsExpanded` | bool | Expanded state | `false` |
| `BotIcon` | ImageSource | Custom icon | `dotnet_bot` |
| `MessageSentCommand` | ICommand | Command triggered when message is sent | `null` |
| `SendOnEnter` | bool | Send message when Enter is pressed | `true` |

🎨 Customization
----------------

### Configure Message Sending Behavior
```xaml
<fc:FloatingChatButton
    x:Name="chatButton"
    MessageSentCommand="{Binding SendMessageCommand}"
    SendOnEnter="True"
    PrimaryColor="#4CAF50"/>
```

### Disable Enter to Send
```xaml
<fc:FloatingChatButton
    SendOnEnter="False"
    MessageSentCommand="{Binding SendMessageCommand}"/>
```
*Users must click the Send button instead of pressing Enter*

### Programmatic Control
```csharp
// Toggle state
floatingChatButton.IsExpanded = !floatingChatButton.IsExpanded;

// Add messages
floatingChatButton.Messages.Add(new ChatMessage 
{
    Text = "New message!",
    IsIncoming = true
});

// Add outgoing message
floatingChatButton.Messages.Add(new ChatMessage 
{
    Text = "User's message",
    IsIncoming = false
});

// Toggle Enter key behavior at runtime
floatingChatButton.SendOnEnter = false;
```

### XAML Setup
```xaml
<fc:FloatingChatButton
    x:Name="chatButton"
    MessageSentCommand="{Binding MessageSentCommand}"
    SendOnEnter="True"
    PrimaryColor="#2196F3"/>
```

📱 Screenshots
--------------

![Collapsed](https://dev.azure.com/jpdmaui/32808558-5c79-418c-906e-a9f52802efc6/_apis/git/repositories/a8c6dfa9-4558-4758-a8b8-6ca3b7f94576/Items?path=/.attachments/Screenshot%202025-07-24%20135441-4e2d7e5c-8050-461d-bde7-16cbf6cb62dc.png&download=false&resolveLfs=true&%24format=octetStream&api-version=5.0-preview.1&sanitize=true&versionDescriptor.version=wikiMaster)
![Expanded](https://dev.azure.com/jpdmaui/32808558-5c79-418c-906e-a9f52802efc6/_apis/git/repositories/a8c6dfa9-4558-4758-a8b8-6ca3b7f94576/Items?path=/.attachments/Screenshot%202025-07-24%20135614-acec17e9-1499-4bd3-bc4f-ce4f8b0b7651.png&download=false&resolveLfs=true&%24format=octetStream&api-version=5.0-preview.1&sanitize=true&versionDescriptor.version=wikiMaster)

🛠 Troubleshooting
------------------

**Common Issues:**
1.  **Missing icons** - Ensure images are in:
    *   Shared: `Resources/Images/`
    *   Android: `Resources/drawable/`
    *   iOS: `Resources/`
        
2.  **Binding not updating** - Use:
```csharp
Messages = new ObservableCollection<ChatMessage>(); 
```

3.  **Animation performance** - Test in Release mode.

4.  **MessageSentCommand not firing** - Ensure:
    *   Command is bound correctly in XAML
    *   BindingContext is set
    *   Command's CanExecute returns true

5.  **Enter key not working** - Check:
    *   `SendOnEnter="True"` is set (default)
    *   Entry field has focus

📚 Resources
------------

*   [Sample App](https://github.com/shaunebu/FloatingChatButton-Sample)


⁉️ Support
----------

Report issues:  

📧 [jorge.p@jpdblog.com](https://mailto:jorge.p@shaunebu.com/)  
🐛 [GitHub Issues](https://github.com/jpd21122012/FloatingChatButton/issues)

📄 License
----------

MIT License © 2025 [Jorge Perales Diaz](https://jpdblog.com/)