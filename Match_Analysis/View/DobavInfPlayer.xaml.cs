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
using Match_Analysis.Model;
using Match_Analysis.VM;

namespace Match_Analysis.View
{
    /// <summary>
    /// Логика взаимодействия для DobavInfPlayer.xaml
    /// </summary>
    public partial class DobavInfPlayer : Window
    {
        public DobavInfPlayer()
        {
            InitializeComponent();

            ((AddInfPlayer)this.DataContext).SetClose(Close);
        }
    }
}
