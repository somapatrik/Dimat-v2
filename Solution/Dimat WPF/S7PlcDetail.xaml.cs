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
//using Sharp7;
using System.Threading;
using Snap7;
using System.Data;

namespace Dimat_WPF
{

    public partial class S7PlcDetail : UserControl
    {
        // PLC from DB
        public S7PLC plc;

        private enum MenuDetailTag {None, Properties};
        private MenuDetailTag OpenedMenu;

        // Database
        DBGlobal dbglob = new DBGlobal();

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
        int ReadingTime = 100;
        bool KillReading;

        // Control
        S7Properties properties;

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
            // S7 Properties control
            properties = new S7Properties(ref client);
            // Set timer for status watching
            StatusWatch = new System.Threading.Timer(StatusWatchCallBack, WatchReset, Timeout.Infinite, WatchStatusTime);
            //Set Reading timer
            ReadingData = new System.Threading.Timer(ReadingCallBack, null, Timeout.Infinite, ReadingTime);
            // Fills GUI
            SetGUI();
            // Create rows
            LoadRows();
            CreateRow();
        }

        private void LoadRows()
        {
            try
            {

            DataTable dt = dbglob.LoadRows(ID);
            foreach (DataRow row in dt.Rows)
            {
                string desc = row["DESCRIPTION"].ToString();
                string add = row["ADDRESS"].ToString();
                string format = row["FORMAT"].ToString();

                CreateRowWithData(add, desc, format);
            }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

                // Show values
                properties.UpdateValues(cpuinfo.ASName, cpuinfo.ModuleName, cpuinfo.ModuleTypeName, cpuinfo.SerialNumber, cpinfo.MaxConnections.ToString());

            });
        }

        private void EnableReading(bool enable)
        {
            if (enable)
                ReadingData.Change(0, ReadingTime);
            else
                ReadingData.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void ReadingCallBack(object state)
        {
            Task.Run(() => { 
            EnableReading(false);

                try
                {
                    StackData.Dispatcher.Invoke(() => {
                        multivar = new S7MultiVar(client);
                        double rowcount = StackData.Children.Count;

                        if (rowcount > 0)
                        {

                            int varcount = 0;

                            foreach (S7DataRow row in StackData.Children)
                            {
                                if (row.IsReadValid) 
                                {
                                    multivar.Add(row.AddressInfo.Area,
                                        row.AddressInfo.WordLen,
                                        row.AddressInfo.DBNumber,
                                        row.AddressInfo.Start,
                                        row.AddressInfo.Amount,
                                        ref row.array);

                                    varcount++;

                                    if (varcount == 19)
                                    {
                                        varcount = 0;
                                        multivar.Read();
                                        //multivar = new S7MultiVar(client);
                                        multivar.Clear();
                                    }
                                }
                            }

                            if (varcount != 0)
                            {
                                multivar.Read();
                                multivar.Clear();
                            }

                                foreach (S7DataRow row in StackData.Children)
                                    row.UpdateValue();

                        }
                    });
            } 
                catch (Exception ex)
                {

                }
                finally
                {
                    if (!KillReading)
                    {
                        Thread.Sleep(50);
                        EnableReading(true);
                    }
                }   
            });
        }

        private void WriteAction()
        {
            S7MultiVar write = new S7MultiVar(client);

            try
            {
                List<S7MultiVar> writestack = new List<S7MultiVar>();
                List<S7DataRow> rowstack = new List<S7DataRow>();

                StackData.Dispatcher.Invoke(()=> 
                {
                    int validrowcount = 0;
                    foreach (S7DataRow row in StackData.Children)
                        if (row.IsReadValid && row.IsWriteValid)
                        {
                            validrowcount++;
                            rowstack.Add(row);
                            write.Add(row.AddressInfo.Area,
                                        row.AddressInfo.WordLen,
                                        row.AddressInfo.DBNumber,
                                        row.AddressInfo.Start,
                                        row.AddressInfo.Amount,
                                        ref row.writearray);

                            // Every 20 row, create a new object
                            if (validrowcount % 20 == 0)
                            {
                                writestack.Add(write);
                                write = new S7MultiVar(client);
                            }
                                
                        }
                    
                    if (validrowcount % 20 != 0)
                        writestack.Add(write);

                    foreach (S7MultiVar writevar in writestack)
                        writevar.Write();


                            
                });

            }catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void SetGUI()
        {
            // Add properties to side menu
            StackControls.Children.Add(properties);

            //Hide functions when S7-1200, S7-1500
            if (plc.Type == "S7-1200" || plc.Type == "S7-1500")
                properties.HideFunctions();

            btnDisconnect.Visibility = Visibility.Collapsed;
            btnReadingStart.Visibility = Visibility.Collapsed;
            btnReadingPause.Visibility = Visibility.Collapsed;
            btnReadOnce.Visibility = Visibility.Collapsed;
            btnWriteAll.Visibility = Visibility.Collapsed;

            lblname.Content = plc.Name;
            lblIP.Content = plc.IP;

            //lblGroupProperties_MouseLeftButtonUp(null, null);
            //lblGroupFunctions_MouseLeftButtonUp(null, null);

            // Close left menu
            ToogleMenu(MenuDetailTag.None);
        }

        public void Kill()
        {
            DisconnectAction();
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
            KillReading = true;
            EnableWatch(false);

            client.Disconnect();

            btnDisconnect.Visibility = Visibility.Collapsed;
            btnConnect.Visibility = Visibility.Visible;
            btnReadingStart.Visibility = Visibility.Collapsed;
            btnReadingPause.Visibility = Visibility.Collapsed;
            btnReadOnce.Visibility = Visibility.Collapsed;
            btnWriteAll.Visibility = Visibility.Collapsed;

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
                btnReadOnce.Visibility = Visibility.Visible;
                btnWriteAll.Visibility = Visibility.Visible;
                EnableWatch(true);
            }
            else
            {
                //LogConsole.Log("Unable to connect to PLC: " + plc.IP + " (" + plc.Name + ") rack: " + plc.Rack + " slot: " + plc.Slot);
            }
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

        private void CreateRowWithData(string add, string desc , string format)
        {
            S7DataRow row = new S7DataRow(ref client);
            row.Description = desc;
            row.Address = add;
            row.Format = format;
            StackData.Children.Add(row);
        }

        #endregion

        #region Top menu

        // Select all
        private void btnSelectAll_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int i = 0;
            foreach (S7DataRow row in StackData.Children)
            {
                row.Selected = true;
                i++;
            }

            //LogConsole.Log(i + " rows selected");
        }

        // Unselect all
        private void btnUnselectAll_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int i = 0;
            foreach (S7DataRow row in StackData.Children)
            {
                row.Selected = false;
                i++;
            }
        }

        // Delete selected
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

        // Start reading
        private void btnReadingStart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            KillReading = false;
            EnableReading(true);
            btnReadingStart.Visibility = Visibility.Collapsed;
            btnReadingPause.Visibility = Visibility.Visible;
            btnReadOnce.Visibility = Visibility.Collapsed;
        }

        // Pause reading
        private void btnReadingPause_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            KillReading = true;
            EnableReading(false);
            btnReadingStart.Visibility = Visibility.Visible;
            btnReadingPause.Visibility = Visibility.Collapsed;
            btnReadOnce.Visibility = Visibility.Visible;
        }

        // Read once
        private void btnReadOnce_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            KillReading = true;
            EnableReading(true);
        } 
        
        // Save button
        private void btnSave_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SaveRows();
        }

        #endregion

        private void SaveRows()
        {
            try
            {
                // Add new rows to database
                foreach (S7DataRow row in StackData.Children)
                    if (row.IsReadValid)
                        dbglob.SaveRow(ID, row.Description, row.Address, row.Format);

                // Delete old rows
                dbglob.DeleteOldRows(ID);

                // Set rows as old
                dbglob.MarkOldRows(ID);

            } catch (Exception ex)
            {
                // Failed - remove new rows
                dbglob.DeleteNewRows(ID);
                MessageBox.Show(ex.Message);
                //LogConsole.Log(ex.Message);
            }
        }

        private void btnProperties_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToogleMenu(MenuDetailTag.Properties, sender);
        }

        private void ToogleMenu(MenuDetailTag tag, object clickedbutton = null)
        {
            DeselectSideMenu();

            // Compare required menu with actual one
            if (OpenedMenu == tag)
                // Same menu => Toogle
                GridRightSideInfo.Visibility = GridRightSideInfo.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            else
                // Just open
                GridRightSideInfo.Visibility = Visibility.Visible;


            if (GridRightSideInfo.Visibility == Visibility.Visible)
            {
                col_RightMenu.Width = new GridLength(300, GridUnitType.Pixel);
                col_RightMenuGripper.Width = new GridLength(1, GridUnitType.Pixel);

                if (clickedbutton != null)
                    ((Label)clickedbutton).Style = (Style)Resources["DetailSideButtonActive"];
            }
            else
            {
                col_RightMenu.Width = new GridLength(0, GridUnitType.Pixel);
                col_RightMenuGripper.Width = new GridLength(0, GridUnitType.Pixel);
            }

            OpenedMenu = tag;
        }

        private void DeselectSideMenu()
        {
            foreach (Label button in StackRightSideMenu.Children)
                button.Style = (Style)Resources["DetailSideButton"];
        }

        private void btnWriteAll_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WriteAction();
        }
    }
}
