using Match_Analysis.Model;
using Match_Analysis.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Match_Analysis.View
{
    /// <summary>
    /// Логика взаимодействия для EditPlayerHistory.xaml
    /// </summary>
    public partial class EditPlayerHistory : Window
    {
        public EditPlayerHistory(PlayerHistory selectedPlayerHistory)
        {
            InitializeComponent();


            ((AddEditPlayerHistory)this.DataContext).SetPlayerHistory(selectedPlayerHistory);
            ((AddEditPlayerHistory)this.DataContext).SetClose(Close);
        }
    }
}
