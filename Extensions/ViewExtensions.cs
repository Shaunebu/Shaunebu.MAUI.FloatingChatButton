namespace FloatingChatButton.Extensions;

public static class ViewExtensions
{
    public static Task<bool> ResizeTo(this VisualElement view, double width, double height, uint length = 250, Easing easing = null)
    {
        var tcs = new TaskCompletionSource<bool>();

        var widthAnimation = new Animation(v => view.WidthRequest = v, view.Width, width, easing);
        var heightAnimation = new Animation(v => view.HeightRequest = v, view.Height, height, easing);

        new Animation {
            { 0, 1, widthAnimation },
            { 0, 1, heightAnimation }
        }.Commit(view, "Resize", length, finished: (v, c) => tcs.SetResult(c));

        return tcs.Task;
    }
}
