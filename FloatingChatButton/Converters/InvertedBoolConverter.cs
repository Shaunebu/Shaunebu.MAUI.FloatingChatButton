using System.Globalization;

namespace FloatingChatButton.Converters;

/// <summary>
/// Produces the logical inverse of a Boolean binding value.
/// </summary>
/// <remarks>
/// Non-Boolean input converts to <see langword="false"/>. This converter is useful for mutually exclusive visibility,
/// enabled-state, or layout bindings in XAML.
/// </remarks>
public class InvertedBoolConverter : IValueConverter
{
    /// <summary>
    /// Converts a Boolean source value to its inverse.
    /// </summary>
    /// <param name="value">The binding value to invert; only Boolean values are inverted.</param>
    /// <param name="targetType">The target binding type requested by the MAUI binding engine.</param>
    /// <param name="parameter">An optional converter parameter; this converter ignores the value.</param>
    /// <param name="culture">The culture supplied by the binding engine; this converter does not use culture-specific formatting.</param>
    /// <returns><see langword="true"/> when <paramref name="value"/> is <see langword="false"/>; otherwise <see langword="false"/>.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    /// <summary>
    /// Converts a target Boolean value back to its inverse for two-way bindings.
    /// </summary>
    /// <param name="value">The binding value to invert; only Boolean values are inverted.</param>
    /// <param name="targetType">The source type requested by the binding engine.</param>
    /// <param name="parameter">An optional converter parameter; this converter ignores the value.</param>
    /// <param name="culture">The culture supplied by the binding engine; this converter does not use culture-specific formatting.</param>
    /// <returns><see langword="true"/> when <paramref name="value"/> is <see langword="false"/>; otherwise <see langword="false"/>.</returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    /// <summary>
    /// Creates a converter for Boolean inversion in one-way or two-way bindings.
    /// </summary>
    public InvertedBoolConverter()
    {
    }
}
