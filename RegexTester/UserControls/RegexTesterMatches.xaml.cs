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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sharomank.RegexTester
{
    /// <summary>
    /// Interaction logic for RegexMatches.xaml
    /// </summary>
    public partial class RegexTesterMatches : UserControl
    {
        public RegexTesterMatches()
        {
            InitializeComponent();
        }

        public event EventHandler<RoutedPropertyChangedEventArgs<object>> SelectedItemChanged; 

        private void tlvMatches_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var handler = SelectedItemChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
