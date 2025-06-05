using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace FitnesPlan
{
    public class PastSelectedDateToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is DateTime day && values[1] is System.Windows.Controls.Calendar calendar)
            {
                Debug.WriteLine($"Checking date: {day.ToShortDateString()}");
                if (calendar.SelectedDates.Contains(day.Date) )
                {
                    return new SolidColorBrush(Colors.LightSkyBlue);
                }
            }

            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}