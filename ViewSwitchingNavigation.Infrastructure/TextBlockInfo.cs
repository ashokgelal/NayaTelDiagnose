using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ViewSwitchingNavigation.Infrastructure
{
   public class TextBlockInfo : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
         {
            if (values.Count() > 3)
            {
                object visibility = Visibility.Collapsed;
                if (values[3] is bool)
                {

                    bool hasTextIP = !(bool)values[3];
                    bool hasText = !(bool)values[0];
                    bool hasMouseOver = (bool)values[2];

                    if(hasTextIP)
                          visibility = Visibility.Visible;
                    if (hasText)
                        visibility = Visibility.Collapsed;
                    if (hasMouseOver)
                        visibility = Visibility.Collapsed;

                    return visibility;


                }

            }





            if (values[0] is bool && values[1] is bool  )
            {
                bool hasText = !(bool)values[0];
                bool hasFocus = (bool)values[1];
                bool hasMouseOver = (bool)values[2];

                if (hasFocus || hasText || hasMouseOver)
                    return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}