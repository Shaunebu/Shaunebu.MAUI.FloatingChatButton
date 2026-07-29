namespace FloatingChatButton.Models;

/// <summary>
/// Represents one message item rendered inside a <see cref="Controls.FloatingChatButton"/> conversation.
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Contains the text shown in the message bubble.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="string.Empty"/>. This is a normal CLR property rather than a bindable property, so
    /// collection item updates should raise change notifications from a custom model when live editing is required. Use a
    /// non-null string for predictable display.
    /// </remarks>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Determines whether the message is styled and aligned as an incoming message.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="false"/>, which displays the message as an outgoing message. This is a normal CLR
    /// property rather than a bindable property; changes take effect when the collection view refreshes or the item is rebound.
    /// </remarks>
    public bool IsIncoming { get; set; }

    /// <summary>
    /// Creates an empty outgoing chat message.
    /// </summary>
    public ChatMessage()
    {
    }
}
