using System.Globalization;

namespace FloatingChatButton.Converters;

/// <summary>
/// Selects one of two <see cref="Color"/> values from a Boolean binding value.
/// </summary>
/// <remarks>
/// The converter is typically used to distinguish incoming and outgoing chat bubbles. Non-Boolean input is treated the same as
/// <see langword="false"/>.
/// </remarks>
public class BoolToColorConverter : IValueConverter
{
    /// <summary>
    /// Defines the color returned when the source value is <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="Colors.LightGray"/>. This is a normal CLR property, not a bindable property; updates
    /// affect subsequent converter calls. Assign a non-null <see cref="Color"/> for predictable rendering.
    /// </remarks>
    public Color TrueColor { get; set; } = Colors.LightGray;

    /// <summary>
    /// Defines the color returned when the source value is <see langword="false"/> or is not Boolean.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="Colors.White"/>. This is a normal CLR property, not a bindable property; updates affect
    /// subsequent converter calls. Assign a non-null <see cref="Color"/> for predictable rendering.
    /// </remarks>
    public Color FalseColor { get; set; } = Colors.White;

    /// <summary>
    /// Converts a Boolean source value into either <see cref="TrueColor"/> or <see cref="FalseColor"/>.
    /// </summary>
    /// <param name="value">The binding value to inspect; only <see langword="true"/> selects <see cref="TrueColor"/>.</param>
    /// <param name="targetType">The target binding type requested by the MAUI binding engine.</param>
    /// <param name="parameter">An optional converter parameter; this converter ignores the value.</param>
    /// <param name="culture">The culture supplied by the binding engine; this converter does not use culture-specific formatting.</param>
    /// <returns><see cref="TrueColor"/> when <paramref name="value"/> is <see langword="true"/>; otherwise <see cref="FalseColor"/>.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && b ? TrueColor : FalseColor;
    }

    /// <summary>
    /// Prevents reverse conversion because color selection is intended to be one-way.
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
    /// Creates a converter that maps <see langword="true"/> to <see cref="Colors.LightGray"/> and all other input to
    /// <see cref="Colors.White"/>.
    /// </summary>
    public BoolToColorConverter()
    {
    }
}
