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

namespace Dimat_WPF
{
    /// <summary>
    /// Interaction logic for StyledCombo.xaml
    /// </summary>
    public partial class StyledCombo : UserControl
    {
        public StyledCombo()
        {
            InitializeComponent();
            cmb.Items.Add("BINARY");
            cmb.Items.Add("DECIMAL");
            cmb.Items.Add("FLOAT");
            cmb.Items.Add("CHAR");
        }
    }
}
