using System.Globalization;

namespace FloatingChatButton.Converters;

/// <summary>
/// Selects one of two <see cref="LayoutOptions"/> values from a Boolean binding value.
/// </summary>
/// <remarks>
/// The converter is designed for chat-message alignment, where incoming and outgoing messages use opposite horizontal
/// placement. Non-Boolean input is treated the same as <see langword="false"/>.
/// </remarks>
public class BoolToAlignmentConverter : IValueConverter
{
    /// <summary>
    /// Defines the layout option returned when the source value is <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="LayoutOptions.Start"/>. This is a normal CLR property, not a bindable property; updates
    /// affect subsequent converter calls. Because <see cref="LayoutOptions"/> is a value type, <see langword="null"/> is not
    /// allowed.
    /// </remarks>
    public LayoutOptions TrueOption { get; set; } = LayoutOptions.Start;

    /// <summary>
    /// Defines the layout option returned when the source value is <see langword="false"/> or is not Boolean.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="LayoutOptions.End"/>. This is a normal CLR property, not a bindable property; updates
    /// affect subsequent converter calls. Because <see cref="LayoutOptions"/> is a value type, <see langword="null"/> is not
    /// allowed.
    /// </remarks>
    public LayoutOptions FalseOption { get; set; } = LayoutOptions.End;

    /// <summary>
    /// Converts a Boolean source value into either <see cref="TrueOption"/> or <see cref="FalseOption"/>.
    /// </summary>
    /// <param name="value">The binding value to inspect; only <see langword="true"/> selects <see cref="TrueOption"/>.</param>
    /// <param name="targetType">The target binding type requested by the MAUI binding engine.</param>
    /// <param name="parameter">An optional converter parameter; this converter ignores the value.</param>
    /// <param name="culture">The culture supplied by the binding engine; this converter does not use culture-specific formatting.</param>
    /// <returns><see cref="TrueOption"/> when <paramref name="value"/> is <see langword="true"/>; otherwise <see cref="FalseOption"/>.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && b ? TrueOption : FalseOption;
    }

    /// <summary>
    /// Prevents reverse conversion because alignment selection is intended to be one-way.
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
    /// Creates a converter that maps <see langword="true"/> to <see cref="LayoutOptions.Start"/> and all other input to
    /// <see cref="LayoutOptions.End"/>.
    /// </summary>
    public BoolToAlignmentConverter()
    {
    }
}
