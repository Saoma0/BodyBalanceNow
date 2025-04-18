using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyBalanceNow.Converters
{
    public class NivelEstresToEmojiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int nivel)
            {
                return nivel switch
                {
                    <= 2 => "😌",
                    <= 4 => "🙂",
                    <= 6 => "😐",
                    <= 8 => "😟",
                    _ => "😫"
                };
            }

            return "❓";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}