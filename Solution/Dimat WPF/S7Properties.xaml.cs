using Snap7;
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

    public partial class S7Properties : UserControl
    {

        S7Client client;

        public S7Properties(ref S7Client clientref)
        {
            client = clientref;
            InitializeComponent();
        }

        public void UpdateValues(string ASName = "", string ModuleName = "",string ModuleTypeName = "", string SerialNumber = "", string MaxConnections = "")
        {
            lbl_ASname.Content = ASName;
            lbl_Modulname.Content = ModuleName;
            lbl_Modultype.Content = ModuleTypeName;
            lbl_Serialnumber.Content = SerialNumber;
            lbl_MaxConnections.Content = MaxConnections;
        }

        public void HideFunctions()
        {
            GridFunctionBar.Visibility = Visibility.Collapsed;
            GridFunctions.Visibility = Visibility.Collapsed;
        }

        private void lblGroupFunctions_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GridFunctions.Visibility == Visibility.Visible)
            {
                GridFunctions.Visibility = Visibility.Collapsed;
                lblGroupFunctionsArrow.Content = "4";
            }
            else
            {
                GridFunctions.Visibility = Visibility.Visible;
                lblGroupFunctionsArrow.Content = "6";
            }
        }

        private void lblGroupProperties_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GridProperties.Visibility == Visibility.Visible)
            {
                GridProperties.Visibility = Visibility.Collapsed;
                lblGroupPropertiesArrow.Content = "4";
            }
            else
            {
                GridProperties.Visibility = Visibility.Visible;
                lblGroupPropertiesArrow.Content = "6";
            }
        }

        // Danger waring confirmation
        private void DangerButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            GridDanger.Visibility = Visibility.Collapsed;
        }

        #region PLC functions
        private void PlcStop_Clicked(object sender, MouseButtonEventArgs e)
        {
            client.PlcStop();
        }

        private void HotStart_Clicked(object sender, MouseButtonEventArgs e)
        {
            client.PlcHotStart();
        }

        private void btnColdStart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            client.PlcColdStart();
        }
        #endregion

    }
}
