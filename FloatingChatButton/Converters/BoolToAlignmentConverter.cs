using System.Globalization;
namespace FloatingChatButton.Converters;
public class BoolToAlignmentConverter : IValueConverter
{
    public LayoutOptions TrueOption { get; set; } = LayoutOptions.Start;
    public LayoutOptions FalseOption { get; set; } = LayoutOptions.End;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? TrueOption : FalseOption;

        return FalseOption;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
