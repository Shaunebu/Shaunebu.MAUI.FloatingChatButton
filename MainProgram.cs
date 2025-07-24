namespace FloatingChatButton;

using CommunityToolkit.Maui;
using Microsoft.Maui.Hosting;

public static class MauiExtensions
{
    public static MauiAppBuilder UseFloatingChatButton(this MauiAppBuilder builder)
    {
        builder.UseMauiCommunityToolkit();
        return builder;
    }
}
