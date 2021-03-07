using Dimat_WPF.Class;
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
using System.Threading;
using System.Net.NetworkInformation;

namespace Dimat_WPF
{

    public partial class PlcButton : UserControl
    {

        public S7PLC s7plc;

        int PingTime = 2500;
        Timer pingwatch;

        public event EventHandler DeleteClicked;
        public event EventHandler EditClicked;

        public PlcButton(S7PLC plc)
        {
            InitializeComponent();

            s7plc = plc;

            lbl_PLCName.Content = s7plc.Name;
            lbl_PLCIP.Content = s7plc.IP;
            lbl_PLCInfo.Content = s7plc.TypeName;

            lblDelete.Visibility = Visibility.Collapsed;
            lblEdit.Visibility = Visibility.Collapsed;


            pingwatch = new Timer(PingCall, null, PingTime, PingTime);
        }


        private void PingCall(object state)
        {
            pingwatch.Change(Timeout.Infinite, Timeout.Infinite);
            
            try
            {

                    Ping ping = new Ping();
                    try
                    {
                        PingReply reply = ping.Send(s7plc.IP, 300);
                        PlcStatusButton.Dispatcher.Invoke(() =>
                        {
                            PlcStatusButton.Style = reply.Status == IPStatus.Success ? (Style)Resources["PlcButtonPingOK"] : (Style)Resources["PlcButtonPingNOK"];
                        });
                    }
                    catch (Exception ex)
                    {
                        PlcStatusButton.Dispatcher.Invoke(() =>
                        {
                            PlcStatusButton.Style = (Style)Resources["PlcButtonPingNOK"];
                        });

                    }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                pingwatch.Change(PingTime, PingTime);
            }
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            lblDelete.Visibility = Visibility.Visible;
            lblEdit.Visibility = Visibility.Visible;
            brd_PlcButton.Style = (Style)Resources["PlcButtonBorderActive"];
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            lblDelete.Visibility = Visibility.Collapsed;
            lblEdit.Visibility = Visibility.Collapsed;
            brd_PlcButton.Style = (Style)Resources["PlcButtonBorder"];
        }

        private void lbl_DeletePLC_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DeleteClicked?.Invoke(s7plc.ID, e); 
        }

        private void lblEdit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EditClicked?.Invoke(s7plc.ID, e);
        }
    }
}
