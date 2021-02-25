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
        Timer pingwatch;

        public PlcButton(S7PLC plc)
        {
            InitializeComponent();

            s7plc = plc;

            lbl_PLCName.Content = s7plc.Name;
            lbl_PLCIP.Content = s7plc.IP;
            lbl_PLCInfo.Content = s7plc.TypeName;

            lbl_DeletePLC.Visibility = Visibility.Collapsed;


            pingwatch = new Timer(new TimerCallback(PingCall), null, 3000, 5000);
        }


        private async void PingCall(object state)
        {
            await Task.Run(() =>
            {
                Ping ping = new Ping();
                try
                {
                    PingReply reply = ping.Send(s7plc.IP, 300);
                    PlcStatusButton.Dispatcher.Invoke(() =>
                    {
                        PlcStatusButton.Style = reply.Status == IPStatus.Success ?  (Style)Resources["PlcButtonPingOK"] : (Style)Resources["PlcButtonPingNOK"] ;
                    });
                }
                catch (Exception ex)
                {
                    PlcStatusButton.Dispatcher.Invoke(() =>
                    {
                        PlcStatusButton.Style = (Style)Resources["PlcButtonPingNOK"];
                    });

                }
            });
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            lbl_DeletePLC.Visibility = Visibility.Visible;
            brd_PlcButton.Style = (Style)Resources["PlcButtonBorderActive"];
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            lbl_DeletePLC.Visibility = Visibility.Collapsed;
            brd_PlcButton.Style = (Style)Resources["PlcButtonBorder"];
        }

      
    }
}
