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
using Dimat_WPF.Class;
using Sharp7;
using System.Threading;

namespace Dimat_WPF
{

    public partial class S7PlcDetail : UserControl
    {
        // PLC from DB
        public S7PLC plc;

        private S7Client client;

        Timer StatusWatch;
        int WatchStatusTime = 500;
        AutoResetEvent WatchReset = new AutoResetEvent(false);

        // client.connected lock
        object ConnectedLock = new object();

        public int ID
        {
            get { return plc.ID; }
        }

        public bool IsClientConnected
        {
            get { return IsConnected(); }
        }

        public S7PlcDetail(int ID)
        {
            InitializeComponent();

            plc = new S7PLC(ID);
            lblname.Content = plc.Name;
            lblIP.Content = plc.IP;

            client = new S7Client();

            StatusWatch = new System.Threading.Timer(StatusWatchCallBack, WatchReset, Timeout.Infinite, WatchStatusTime);

            S7DataRow row = new S7DataRow(ref client);
            StackData.Children.Add(row);

            SetGUI();
        }

        private void SetGUI()
        {

            lblGroupProperties_MouseLeftButtonUp(null, null);
            lblGroupFunctions_MouseLeftButtonUp(null,null);

        }

        private bool IsConnected()
        {
            lock (ConnectedLock)
                return client.Connected;
        }

        private void EnableWatch(bool enable)
        {
            if (enable)
                StatusWatch.Change(0, WatchStatusTime);
            else
                StatusWatch.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void StatusWatchCallBack(object state)
        {

            GridRightSideInfo.Dispatcher.Invoke(() =>
            {
                if (!IsConnected())
                {
                    DisconnectAction();
                    return;
                }

                // CPU status
                int cpustatus = 0;
                client.PlcGetStatus(ref cpustatus);

                lblPlcStatus.Style = cpustatus == 8 ? (Style)Resources["ColorLabelOK"] : (Style)Resources["ColorLabelNOK"];

                if (cpustatus == 0)
                    lblPlcStatus.Content = "Unknown status";

                if (cpustatus == 4)
                    lblPlcStatus.Content = "STOP";

                if (cpustatus == 8)
                    lblPlcStatus.Content = "RUN";


                S7Client.S7CpuInfo cpuinfo = new S7Client.S7CpuInfo();
                client.GetCpuInfo(ref cpuinfo);
                S7Client.S7CpInfo cpinfo = new S7Client.S7CpInfo();
                client.GetCpInfo(ref cpinfo);

                lbl_ASname.Content = cpuinfo.ASName;
                lbl_Modulname.Content = cpuinfo.ModuleName;
                lbl_Modultype.Content = cpuinfo.ModuleTypeName;
                lbl_Serialnumber.Content = cpuinfo.SerialNumber;
                lbl_MaxConnections.Content = cpinfo.MaxConnections.ToString();

            });
        }

        // Connect button
        private void lblConnect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (client.Connected)
                DisconnectAction();
            else
                ConnectAction();
        }

        private void DisconnectAction()
        {
            EnableWatch(false);
            client.Disconnect();
            lblConnect.Content = "Connect";
            lblPlcStatus.Style = (Style)Resources["ColorLabelNOK"];
            lblPlcStatus.Content = "Disconnected";
        }

        private void ConnectAction()
        {
            if (client.ConnectTo(plc.IP, plc.Rack, plc.Slot) == 0)
            {
                lblConnect.Content = "Disconnect";
                EnableWatch(true);
            }
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            client.PlcStop();
        }

        private void Label_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            client.PlcHotStart();
        }

        private void lblGroupFunctions_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //GridFunctions.Visibility = GridFunctions.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
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
            //GridProperties.Visibility = GridProperties.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
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

        private void DangerButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            GridDanger.Visibility = Visibility.Collapsed;
        }
    }
}
