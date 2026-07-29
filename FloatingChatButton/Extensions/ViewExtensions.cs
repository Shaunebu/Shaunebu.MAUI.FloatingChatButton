namespace FloatingChatButton.Extensions;

/// <summary>
/// Adds animation helpers for MAUI visual elements used by the floating chat button.
/// </summary>
public static class ViewExtensions
{
    /// <summary>
    /// Animates a visual element's requested width and height together.
    /// </summary>
    /// <param name="view">The visual element whose <see cref="VisualElement.WidthRequest"/> and <see cref="VisualElement.HeightRequest"/> are animated.</param>
    /// <param name="width">The final requested width, in device-independent units.</param>
    /// <param name="height">The final requested height, in device-independent units.</param>
    /// <param name="length">The animation duration in milliseconds; the default duration is 250 milliseconds.</param>
    /// <param name="easing">The easing function applied to both dimensions, or <see langword="null"/> to use the MAUI animation default.</param>
    /// <returns>
    /// A task that completes when the resize animation finishes. The result is <see langword="true"/> when MAUI reports that
    /// the animation was canceled; otherwise, the result is <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/> is negative, NaN, or infinite.</exception>
    /// <remarks>
    /// Starting a new resize animation on the same element aborts any existing animation registered with the <c>Resize</c> name.
    /// </remarks>
    public static Task<bool> ResizeTo(this VisualElement view, double width, double height, uint length = 250, Easing? easing = null)
    {
        ArgumentNullException.ThrowIfNull(view);

        if (!double.IsFinite(width) || width < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be a finite non-negative value.");
        }

        if (!double.IsFinite(height) || height < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be a finite non-negative value.");
        }

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var startWidth = double.IsFinite(view.Width) && view.Width >= 0 ? view.Width : view.WidthRequest;
        var startHeight = double.IsFinite(view.Height) && view.Height >= 0 ? view.Height : view.HeightRequest;

        view.AbortAnimation("Resize");

        var widthAnimation = new Animation(v => view.WidthRequest = v, startWidth, width, easing);
        var heightAnimation = new Animation(v => view.HeightRequest = v, startHeight, height, easing);

        new Animation
        {
            { 0, 1, widthAnimation },
            { 0, 1, heightAnimation }
        }.Commit(view, "Resize", length, finished: (v, c) => tcs.TrySetResult(c));

        return tcs.Task;
    }
}
