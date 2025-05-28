using System.Globalization;
namespace FloatingChatButton.Converters;

public class BoolToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Colors.LightGray;
    public Color FalseColor { get; set; } = Colors.LightBlue;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? TrueColor : FalseColor;

        return FalseColor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
