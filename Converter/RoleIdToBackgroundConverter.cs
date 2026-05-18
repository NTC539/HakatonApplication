using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace HakatonApplication.Converter
{
    public class RoleIdToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int roleId = value is int r ? r : 0;
            return roleId switch
            {
                1 => new SolidColorBrush(Colors.Green),
                2 => new SolidColorBrush(Colors.Orange),
                3 => new SolidColorBrush(Colors.CornflowerBlue),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
