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
    /// Interaction logic for DeletePlc.xaml
    /// </summary>
    public partial class DeletePlc : UserControl
    {

        public event EventHandler Close;
        private DBGlobal dbglob = new DBGlobal();

        private int ID;

        public DeletePlc(int PLC_ID)
        {
            InitializeComponent();
            ID = PLC_ID;
        }

        private void lbl_No_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseDialog();
        }

        private void lbl_Yes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dbglob.DeletePlc(ID);
            CloseDialog();
        }

        private void CloseDialog()
        {
            Close?.Invoke(this, null);
        }
    }
}
