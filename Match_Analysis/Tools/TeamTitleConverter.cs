using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Match_Analysis.Model;
using System.Windows.Data;

namespace Match_Analysis.Tools
{
    public class TeamTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var team = value as Team;

            if (team == null || team.Id == 0)
                return "Свободный агент";

            return team.Title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
