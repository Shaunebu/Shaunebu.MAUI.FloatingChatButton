namespace FloatingChatButton;

using Microsoft.Maui.Hosting;

/// <summary>
/// Provides MAUI application registration helpers for the FloatingChatButton package.
/// </summary>
public static class MauiExtensions
{
    /// <summary>
    /// Registers the FloatingChatButton package with a MAUI application builder.
    /// </summary>
    /// <param name="builder">The application builder being configured during MAUI startup.</param>
    /// <returns>The same <paramref name="builder"/> instance so additional startup calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The current control does not require service registration, but this extension gives applications a stable startup hook
    /// for package initialization.
    /// </remarks>
    public static MauiAppBuilder UseFloatingChatButton(this MauiAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder;
    }
}
