﻿using System;
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
//using Sharp7;
using System.Threading;
using Snap7;

namespace Dimat_WPF
{

    public partial class S7PlcDetail : UserControl
    {
        // PLC from DB
        public S7PLC plc;

        // PLC client
        private S7Client client;
        S7MultiVar multivar;

        // client.connected lock
        object ConnectedLock = new object();

        // Check PLC connection timer
        Timer StatusWatch;
        int WatchStatusTime = 1000;
        AutoResetEvent WatchReset = new AutoResetEvent(false);

        // Reading
        Timer ReadingData;
        int ReadingTime = 50;
                
        public int ID
        {
            get { return plc.ID; }
        }

        public bool IsClientConnected
        {
            get { return IsConnected(); }
        }

        private bool IsConnected()
        {
            lock (ConnectedLock)
                return client.Connected();
        }

        public S7PlcDetail(int ID)
        {
            InitializeComponent();

            // Gets PLC data
            plc = new S7PLC(ID);
            // Connection to PLC
            client = new S7Client();
            //Reading variables
            multivar = new S7MultiVar(client);
            // Set timer for status watching

            StatusWatch = new System.Threading.Timer(StatusWatchCallBack, WatchReset, Timeout.Infinite, WatchStatusTime);
            //Set Reading timer
            ReadingData = new System.Threading.Timer(ReadingCallBack, null, Timeout.Infinite, ReadingTime);
            // Fills GUI
            SetGUI();
            // Create rows
            CreateRow();
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

        // Reading variable code
        private void EnableReading(bool enable)
        {
            if (enable)
                ReadingData.Change(0, ReadingTime);
            else
                ReadingData.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void ReadingCallBack(object state)
        {
            EnableReading(false);

            try
            {
                StackData.Dispatcher.Invoke(() => {
                foreach (S7DataRow row in StackData.Children)
                    {  
                      row.Read();
                    }
                });

            } 
            catch (Exception ex)
            {

            }
            finally
            {
                Thread.Sleep(1000);   
                EnableReading(true);
            }

            
        }

        private void SetGUI()
        {
            btnDisconnect.Visibility = Visibility.Collapsed;
            btnReadingStart.Visibility = Visibility.Collapsed;
            btnReadingPause.Visibility = Visibility.Collapsed;

            lblname.Content = plc.Name;
            lblIP.Content = plc.IP;

            lblGroupProperties_MouseLeftButtonUp(null, null);
            lblGroupFunctions_MouseLeftButtonUp(null, null);

        }

        #region Connect / disconnect button
        private void lblConnect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (client.Connected())
                DisconnectAction();
            else
                ConnectAction();
        }

        private void DisconnectAction()
        {
            EnableWatch(false);
            client.Disconnect();
            btnDisconnect.Visibility = Visibility.Collapsed;
            btnConnect.Visibility = Visibility.Visible;
            btnReadingStart.Visibility = Visibility.Collapsed;
            btnReadingPause.Visibility = Visibility.Collapsed;
            lblPlcStatus.Style = (Style)Resources["ColorLabelNOK"];
            lblPlcStatus.Content = "Disconnected";
        }

        private void ConnectAction()
        {
            if (client.ConnectTo(plc.IP, plc.Rack, plc.Slot) == 0)
            {
                btnDisconnect.Visibility = Visibility.Visible;
                btnConnect.Visibility = Visibility.Collapsed;
                btnReadingStart.Visibility = Visibility.Visible;
                EnableWatch(true);
            }
        }

        #endregion

        #region Functions groups
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

        #endregion

        #region New row
        private void btnNewRow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CreateRow();
        }

        private void CreateRow()
        {
            S7DataRow row = new S7DataRow(ref client);
            StackData.Children.Add(row);
        }
        #endregion

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

        #region Top menu
        private void btnSelectAll_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (S7DataRow row in StackData.Children)
            {
                row.Selected = true;
            }
        }

        private void btnUnselectAll_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (S7DataRow row in StackData.Children)
                row.Selected = false;
        }

        private void btnDeleteSelected_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            List<S7DataRow> MarkDelete = new List<S7DataRow>();
            
            foreach (S7DataRow row in StackData.Children)
                if (row.Selected)
                    MarkDelete.Add(row);

            foreach (S7DataRow row in MarkDelete)
                StackData.Children.Remove(row);

            MarkDelete.Clear();
        }

        private void btnReadingStart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EnableReading(true);
            btnReadingStart.Visibility = Visibility.Collapsed;
            btnReadingPause.Visibility = Visibility.Visible;
        }

        private void btnReadingPause_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EnableReading(false);
            btnReadingStart.Visibility = Visibility.Visible;
            btnReadingPause.Visibility = Visibility.Collapsed;
        }
        #endregion

    }
}
