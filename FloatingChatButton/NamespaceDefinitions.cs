using System.Windows.Markup;

[assembly: XmlnsDefinition("http://schemas.shaunebu.com/maui/controls", "FloatingChatButton.Controls")]
[assembly: XmlnsDefinition("http://schemas.shaunebu.com/maui/controls", "FloatingChatButton.Converters")]
[assembly: XmlnsDefinition("http://schemas.shaunebu.com/maui/controls", "FloatingChatButton.Extensions")]
[assembly: XmlnsDefinition("http://schemas.shaunebu.com/maui/controls", "FloatingChatButton.Models")]
[assembly: Microsoft.Maui.Controls.XmlnsPrefix("http://schemas.shaunebu.com/maui/controls", "fc")]

/// <summary>
/// Identifies the assembly that registers the FloatingChatButton XAML namespace mappings.
/// </summary>
/// <remarks>
/// Applications can reference <c>http://schemas.shaunebu.com/maui/controls</c> with the suggested <c>fc</c> prefix to use the
/// controls, converters, extensions, and models exported by this package in XAML.
/// </remarks>
public static class NamespaceRegistration
{
}
