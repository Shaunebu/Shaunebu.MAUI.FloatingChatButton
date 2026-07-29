using Microsoft.Maui.Controls.Shapes;
using System.Globalization;

namespace FloatingChatButton.Converters;

/// <summary>
/// Creates the rounded-rectangle shape used to visually distinguish incoming and outgoing chat messages.
/// </summary>
/// <remarks>
/// The converter reads a Boolean incoming-message flag. <see langword="true"/> produces a bubble with the lower-left corner
/// squared; <see langword="false"/> and non-Boolean values produce a bubble with the lower-right corner squared.
/// </remarks>
public class BoolToStrokeShapeConverter : IValueConverter
{
    /// <summary>
    /// Converts a Boolean incoming-message flag into a <see cref="RoundRectangle"/> bubble shape.
    /// </summary>
    /// <param name="value">The binding value to inspect; <see langword="true"/> means the message is incoming.</param>
    /// <param name="targetType">The target binding type requested by the MAUI binding engine.</param>
    /// <param name="parameter">An optional converter parameter; this converter ignores the value.</param>
    /// <param name="culture">The culture supplied by the binding engine; this converter does not use culture-specific formatting.</param>
    /// <returns>A new <see cref="RoundRectangle"/> configured for the message direction represented by <paramref name="value"/>.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isIncoming = value is bool b && b;
        return new RoundRectangle
        {
            CornerRadius = isIncoming
                ? new CornerRadius(10, 10, 0, 10)
                : new CornerRadius(10, 10, 10, 0)
        };
    }

    /// <summary>
    /// Prevents reverse conversion because the shape is derived from message direction.
    /// </summary>
    /// <param name="value">The target value supplied by the binding engine.</param>
    /// <param name="targetType">The source type requested by the binding engine.</param>
    /// <param name="parameter">An optional converter parameter; this converter ignores the value.</param>
    /// <param name="culture">The culture supplied by the binding engine; this converter does not use culture-specific formatting.</param>
    /// <returns><see cref="Binding.DoNothing"/> so the source binding is left unchanged.</returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    /// <summary>
    /// Creates a converter that generates chat-bubble shapes from incoming-message flags.
    /// </summary>
    public BoolToStrokeShapeConverter()
    {
    }
}
