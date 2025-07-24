# FloatingChatButton for .NET MAUI

![NuGet Version](https://img.shields.io/nuget/v/shaunebu.MAUI.Controls.FloatingChatButton?color=blue&label=NuGet)

Un botón de chat flotante completamente personalizable para aplicaciones .NET MAUI, con soporte para mensajes y animaciones fluidas.

## 📦 Instalación

Agrega el paquete NuGet a tu proyecto:

```bash
dotnet add package Shaunebu.MAUI.Controls.FloatingChatButton
```

## 🚀 Uso Básico

1. Agrega el namespace en tu XAML:

```xml
xmlns:fc="http://schemas.shaunebu.com/maui/controls"
```

2. Implementa el control:

```xml
<fc:FloatingChatButton PrimaryColor="#2196F3">
    <fc:FloatingChatButton.Messages>
        <x:Array Type="{x:Type fc:ChatMessage}">
            <fc:ChatMessage Text="Bienvenido!" IsIncoming="true"/>
            <fc:ChatMessage Text="¿En qué puedo ayudarte?" IsIncoming="false"/>
        </x:Array>
    </fc:FloatingChatButton.Messages>
</fc:FloatingChatButton>
```

## 🛠 Propiedades Principales

| Propiedad | Tipo | Descripción | Valor Predeterminado |
|-----------|------|-------------|----------------------|
| `PrimaryColor` | Color | Color principal del botón | `#2196F3` (Azul) |
| `Messages` | `ObservableCollection<ChatMessage>` | Colección de mensajes | `new ObservableCollection<ChatMessage>()` |
| `IsExpanded` | bool | Estado expandido/contraído | `false` |

## 🎨 Personalización Avanzada

### Cambiar colores:
```xml
<fc:FloatingChatButton PrimaryColor="#4CAF50"
                       OverlayColor="#80000000">
```

### Configurar tamaño expandido:
```csharp
// En tu código
floatingChatButton.ExpandedWidth = 0.8; // 80% del ancho
floatingChatButton.ExpandedHeight = 0.6; // 60% del alto
```

## 📱 Screenshots

![Vista móvil](https://ejemplo.com/screenshot-mobile.png) 
![Vista desktop](https://ejemplo.com/screenshot-desktop.png)

## ⁉️ Soporte

¿Encontraste un problema o tienes una sugerencia?  
[Abre un issue](mailto:jorge.p@shaunebu.com)

## 📄 Licencia

MIT License - Copyright © 2025 [Jorge Perales Diaz]