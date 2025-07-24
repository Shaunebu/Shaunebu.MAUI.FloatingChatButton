using Microsoft.Maui.Controls.Shapes;
using System.Globalization;

namespace FloatingChatButton.Converters
{
    public class BoolToStrokeShapeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isIncoming)
            {
                return isIncoming
                    ? new RoundRectangle { CornerRadius = new CornerRadius(10, 10, 0, 10) } :
                    new RoundRectangle { CornerRadius = new CornerRadius(10, 10, 10, 0) };
            }

            // Default shape if binding fails
            return new RoundRectangle { CornerRadius = new CornerRadius(10, 10, 10, 0) };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
