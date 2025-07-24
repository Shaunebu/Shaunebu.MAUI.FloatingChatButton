# 🚀 FloatingChatButton for .NET MAUI

![NuGet Version](https://img.shields.io/nuget/v/Shaunebu.MAUI.Controls.FloatingChatButton?color=blue&label=NuGet)
![Platform Support](https://img.shields.io/badge/platforms-Android|iOS-lightgrey)
![MAUI Version](https://img.shields.io/badge/.NET%20MAUI-%3E%3D9.0-blueviolet)

A fully customizable floating chat button component for .NET MAUI applications with built-in messaging UI and smooth animations.

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
```
<fc:FloatingChatButton
    PrimaryColor="#2196F3"
    BotIcon="chat_icon.png">
    
    <fc:FloatingChatButton.Messages>
        <x:Array Type="{x:Type fc:ChatMessage}">
            <fc:ChatMessage Text="Welcome!" IsIncoming="true"/>
            <fc:ChatMessage Text="How can I help?" IsIncoming="false"/>
        </x:Array>
    </fc:FloatingChatButton.Messages>
</fc:FloatingChatButton>
```

⚙️ Core Properties
------------------

| Property | Type | Description | Default |
| --- | --- | --- | --- |
| `PrimaryColor` | Color | Button accent color | `#2196F3` |
| `Messages` | `ObservableCollection<ChatMessage>` | Chat messages | Empty |
| `IsExpanded` | bool | Expanded state | `false` |
| `BotIcon` | ImageSource | Custom icon | `icon_bot` |
| `ExpandedWidth` | double | Width ratio (0-1) | `0.8` |
| `ExpandedHeight` | double | Height ratio (0-1) | `0.6` |
| `EdgePadding` | int | Screen edge margin | `20` |

🎨 Customization
----------------

### Change Colors
```
<fc:FloatingChatButton
    PrimaryColor="#4CAF50"
    MessageIncomingColor="#EEEEEE"
    MessageOutgoingColor="#4CAF50"/>
```

### Programmatic Control
```
// Toggle state
floatingChatButton.IsExpanded = !floatingChatButton.IsExpanded;

// Add messages
floatingChatButton.Messages.Add(new ChatMessage {
    Text = "New message!",
    IsIncoming = true
});

// Customize animations
floatingChatButton.ExpandDuration = 400;
```

📱 Screenshots
--------------

![Collapsed](https://dev.azure.com/jpdmaui/32808558-5c79-418c-906e-a9f52802efc6/_apis/git/repositories/a8c6dfa9-4558-4758-a8b8-6ca3b7f94576/Items?path=/.attachments/Screenshot%202025-07-24%20135441-4e2d7e5c-8050-461d-bde7-16cbf6cb62dc.png&download=false&resolveLfs=true&%24format=octetStream&api-version=5.0-preview.1&sanitize=true&versionDescriptor.version=wikiMaster)
![Expanded](https://dev.azure.com/jpdmaui/32808558-5c79-418c-906e-a9f52802efc6/_apis/git/repositories/a8c6dfa9-4558-4758-a8b8-6ca3b7f94576/Items?path=/.attachments/Screenshot%202025-07-24%20135614-acec17e9-1499-4bd3-bc4f-ce4f8b0b7651.png&download=false&resolveLfs=true&%24format=octetStream&api-version=5.0-preview.1&sanitize=true&versionDescriptor.version=wikiMaster)

🛠 Troubleshooting
------------------

**Common Issues:**
1.  **Missing icons** - Ensure images are in:
    *   Shared: `Resources/Images/`
        
    *   Android: `Resources/drawable/`
        
    *   iOS: `Resources/`
        
2.  **Binding not updating** - Use:
    
```
Messages = new ObservableCollection<ChatMessage>(); 
```

3.  **Animation performance** - Test in Release mode.
    

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