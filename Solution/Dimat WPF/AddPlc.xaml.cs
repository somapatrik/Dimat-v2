using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using static Dimat_WPF.Class.S7PLC;
using System.Threading;
using System.Net.NetworkInformation;
using Sharp7;
using Dimat_WPF.Class;

namespace Dimat_WPF
{
    public partial class AddPlc : UserControl
    {
        public event EventHandler CloseClicked;

        DBGlobal dbhelp = new DBGlobal();
        Timer pingwatch;
        bool pingenabled;

        private bool ValidName;
        private bool ValidType;
        private bool ValidIP;
        private bool ValidSlot;
        private bool ValidRack;

        private string PLCtype;
        IPAddress PLCIP;
        int ID_GROUP;

        bool _Edit;
        S7PLC _PLCedit;

        public AddPlc(int GROUP_ID)
        {
            InitializeComponent();
            ID_GROUP = GROUP_ID;

            pingwatch = new Timer(new TimerCallback(PingCall), pingenabled, 0, 1000);
            SaveAvailable();
        }

        public AddPlc(S7PLC plc)
        {
            InitializeComponent();
            _Edit = true;
            _PLCedit = plc;

            pingwatch = new Timer(new TimerCallback(PingCall), pingenabled, 0, 1000);
            FillFromDB();
            SaveAvailable();
        }

        private void FillFromDB()
        {
            txtName.Text = _PLCedit.Name;
            txtDesc.Text = _PLCedit.Description;
            txtRack.Text = _PLCedit.Rack.ToString();
            txtSlot.Text = _PLCedit.Slot.ToString();

            switch (_PLCedit.TypeName)
            {
                case "S7-300":
                    TypeButton1.Style = (Style)Resources["ColorButtonOK"];
                    PLCtype = TypeButton1.Tag.ToString();
                    break;
                case "S7-400":
                    TypeButton2.Style = (Style)Resources["ColorButtonOK"];
                    PLCtype = TypeButton1.Tag.ToString();
                    break;
                case "S7-1200":
                    TypeButton2.Style = (Style)Resources["ColorButtonOK"];
                    PLCtype = TypeButton1.Tag.ToString();
                    break;
                case "S7-1500":
                    TypeButton2.Style = (Style)Resources["ColorButtonOK"];
                    PLCtype = TypeButton1.Tag.ToString();
                    break;
            }

        }

        // Close button event
        private void Close_clicked(object sender, MouseButtonEventArgs e)
        {
            CloseClicked?.Invoke(this, null);
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_Edit)
            {
                if (txtName.Text.Length < 4 || !dbhelp.IsPlcNameAvailable(txtName.Text))
                {
                    txtName.Style = (Style)Resources["DarkBoxNOK"];
                    ValidName = false;
                }
                else
                {
                    txtName.Style = (Style)Resources["DarkBox"];
                    ValidName = true;
                }
            }
            else
            {
                if (txtName.Text.Length < 4 || dbhelp.IsUpdatePlcIpAvailable(txtName.Text, _PLCedit.ID))
                {
                    txtName.Style = (Style)Resources["DarkBoxNOK"];
                    ValidName = false;
                }
                else
                {
                    txtName.Style = (Style)Resources["DarkBox"];
                    ValidName = true;
                }
            }
            SaveAvailable();
        }

        // Type
        private void TypeButton_Click(object sender, MouseButtonEventArgs e)
        {
            TypeButton1.Style = (Style)Resources["ColorButton"];
            TypeButton2.Style = (Style)Resources["ColorButton"];
            TypeButton3.Style = (Style)Resources["ColorButton"];
            TypeButton4.Style = (Style)Resources["ColorButton"];

            Label button = (Label)sender;
            button.Style = (Style)Resources["ColorButtonOK"];

            string buttontype = button.Tag.ToString();

            if (buttontype == "S7-300" || buttontype == "S7-400" || buttontype == "S7-1200" || buttontype == "S7-1500")
                PLCtype = buttontype;

            ValidType = true;
            SaveAvailable();
        }

        // IP
        private void lblIP_TextChanged(object sender, TextChangedEventArgs e)
       {
            if (!_Edit)
            {
                if (!IPAddress.TryParse(lblIP.Text, out PLCIP))
                {
                    lblIP.Style = (Style)Resources["DarkBoxNOK"];
                    ValidIP = false;
                    pingenabled = false;
                }
                else
                {
                    lblIP.Style = (Style)Resources["DarkBox"];
                    ValidIP = true;
                    pingenabled = true;
                }
            }
            else
            {
                if (!IPAddress.TryParse(lblIP.Text, out PLCIP))
                {
                    lblIP.Style = (Style)Resources["DarkBoxNOK"];
                    ValidIP = false;
                    pingenabled = false;
                }
                else
                {
                    lblIP.Style = (Style)Resources["DarkBox"];
                    ValidIP = true;
                    pingenabled = true;
                }
            }

            SaveAvailable();

            lbls7.Style = (Style)Resources["ColorButton"];
        } 

        // Rack / Slot
        private void txtRack_changed(object sender, TextChangedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            string name = box.Name;
            string value = box.Text;
            int num;

            if (int.TryParse(value, out num) && !string.IsNullOrEmpty(box.Text))
            {
                if (num < 0)
                    box.Text = "0";

                box.Style = (Style)Resources["DarkBox"];
                
                if (name == "txtRack")
                    ValidRack = true;
                else if (name == "txtSlot")
                    ValidSlot = true;

            } else
            {
                box.Style = (Style)Resources["DarkBoxNOK"];

                if (name == "txtRack")
                    ValidRack = false;
                else if (name == "txtSlot")
                    ValidSlot = false;
            }

            SaveAvailable();
            lbls7.Style = (Style)Resources["ColorButton"];

        }

        // S7 connection button
        private async void lbls7_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            await Task.Run(() => {

                this.Dispatcher.Invoke(() =>
                {
                    lbls7.Style = (Style)Resources["ColorLabelNOK"];

                    if (ValidIP & ValidRack & ValidSlot)
                    {
                        this.Cursor = Cursors.Wait;
                        S7Client client = new S7Client();
                        if (client.ConnectTo(PLCIP.ToString(), int.Parse(txtRack.Text), int.Parse(txtSlot.Text)) == 0)
                            lbls7.Style = (Style)Resources["ColorButtonOK"];
                        else
                            lbls7.Style = (Style)Resources["ColorButtonNOK"];

                        client = null;
                        this.Cursor = Cursors.Arrow;
                    }
                });
            });
        }

        //Save button
        private void lblSave_up(object sender, MouseButtonEventArgs e)
        {
            try 
            { 
                if (ValidName && ValidIP && ValidType && ValidRack && ValidSlot)
                {
                    S7PLC plc = new S7PLC(txtName.Text, lblIP.Text, int.Parse(txtRack.Text), int.Parse(txtSlot.Text), PLCtype);
                    plc.Description = !string.IsNullOrEmpty(txtDesc.Text) ? txtDesc.Text : "";
                    plc.Insert();
                    if (plc.IsGroupAvailable(ID_GROUP))
                        plc.UpdateGroup(ID_GROUP);

                    Close_clicked(null, null);
            }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SaveAvailable()
        {
            if (ValidName && ValidIP && ValidType && ValidRack && ValidSlot)
                lblSave.Visibility = Visibility.Visible;
            else
                lblSave.Visibility = Visibility.Collapsed;
            
        }

        private void PingCall(object state)
        {
                Ping ping = new Ping();
                try
                {
                    PingReply reply = ping.Send(PLCIP, 300);
                    lblIP.Dispatcher.Invoke(() =>
                    {
                        lblPingStatus.Style = reply.Status == IPStatus.Success ? (Style)Resources["ColorLabelOK"] : (Style)Resources["ColorLabelNOK"];
                    });
                }
                catch (Exception ex)
                {
                    lblIP.Dispatcher.Invoke(() =>
                    {
                        lblPingStatus.Style = (Style)Resources["ColorLabelNOK"];
                    });

                }
        }
    }
}
