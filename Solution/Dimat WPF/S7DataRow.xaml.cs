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
using Sharp7;

namespace Dimat_WPF
{

    public partial class S7DataRow : UserControl
    {
        S7Client client;
        AddressFormatter addressformatter = new AddressFormatter();

        int Area;
        int DBNumber;
        int Start;
        int Amount;
        int WordLen;
        byte[] array;

        public S7DataRow(ref S7Client PlcClient)
        {
            InitializeComponent();
            client = PlcClient;
        }
        
        private async void Read()
        {
            await Task.Run(() =>
            {
                GridRow.Dispatcher.Invoke(()=>
                {
                    if (addressformatter.IsValid && client.Connected && client.ReadArea(Area, DBNumber, Start, Amount, WordLen, array) == 0)
                        FormatValue();
                    else
                        txt_Actual.Text = "";
                });
            });
        }

        private void FormatValue()
        {
            switch(cmb_Format.SelectedValue.ToString())
            {
                case "BOOL":
                    txt_Actual.Text = GetBooltS();
                    break;
                case "BINARY":
                    txt_Actual.Text = GetBinS();
                    break;
                case "DECIMAL":
                    txt_Actual.Text = GetDecS();
                    break;
                case "CHARACTER":
                    txt_Actual.Text = GetCharS();
                    break;
                case "FLOAT":
                    txt_Actual.Text = GetFloatS();
                    break;
            }
        }

        #region Format functions

        public string GetBooltS()
        {
            Boolean b = S7.GetBitAt(array, 0, 0);
            return b.ToString();
        }

        public string GetDecS()
        {
            string i = "[Error]";

            switch (array.Length)
            {
                case 1:
                    i = S7.GetSIntAt(array, 0).ToString();
                    break;
                case 2:
                    i = S7.GetIntAt(array, 0).ToString();
                    break;
                case 4:
                    i = S7.GetDIntAt(array, 0).ToString();
                    break;
            }

            return i;
        }

        public string GetCharS()
        {
            string s = "";
            if (array.Length > 0)
                s = S7.GetCharsAt(array, 0, array.Length);

            return s;
        }

        public string GetBinS()
        {
            string s = string.Join(" ", array.Select(x => Convert.ToString(x, 2).PadLeft(8, '0')));
            return s;
        }

        public string GetFloatS()
        {
            string i = "";
            switch (array.Length)
            {
                case 4:
                    i = S7.GetRealAt(array, 0).ToString();
                    break;
            }
            return i;
        }

        #endregion

        private void SetReading()
        {
            if (addressformatter.IsInput)
                Area = S7Consts.S7AreaPE;
            else if (addressformatter.IsOutput)
                Area = S7Consts.S7AreaPA;
            else if (addressformatter.IsMerker)
                Area = S7Consts.S7AreaMK;
            else if (addressformatter.IsDB)
                Area = S7Consts.S7AreaDB;

            if (addressformatter.IsBit)
            {
                WordLen = S7Consts.S7WLBit;
                array = new byte[1];
            }
            else if (addressformatter.IsByte)
            {
                WordLen = S7Consts.S7WLByte;
                array = new byte[1];
            }
            else if (addressformatter.IsWord) {
                WordLen = S7Consts.S7WLWord; 
                array = new byte[2];
            }
            else if (addressformatter.IsDouble)
            {
                WordLen = S7Consts.S7WLDWord;
                array = new byte[4];
            }
                

            Amount = 1;

            if (addressformatter.IsBit)
                Start = (addressformatter.Byte * 8) + addressformatter.Bit;
            else
                Start = addressformatter.Byte;

            if (addressformatter.IsDB)
                DBNumber = addressformatter.DBNumber;
        }

        // Address lost focus
        private void txt_Address_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            string input = box.Text;

            addressformatter.Address = input;
            box.Text = addressformatter.Address;

            if (addressformatter.IsValid)
            {
                box.Style = (Style)Resources["AddressBox"];
                SetReading();
            } else
            {
                box.Style = (Style)Resources["AddressBoxNOK"];
            }

            CreateFormatMenu();
        }

        private void CreateFormatMenu()
        {
            // Last selected value
            object selected = null;
            if (cmb_Format.Items.Count > 0)
                selected = cmb_Format.SelectedItem;

            // New menu
            cmb_Format.Items.Clear();

            if (addressformatter.IsValid)
            {
                if (addressformatter.IsBit)
                {
                    cmb_Format.Items.Add("BOOL");
                }
                else if (addressformatter.IsByte || addressformatter.IsWord)
                {
                    cmb_Format.Items.Add("BINARY");
                    cmb_Format.Items.Add("DECIMAL");
                    cmb_Format.Items.Add("CHARACTER");
                }
                else if (addressformatter.IsDouble)
                {
                    cmb_Format.Items.Add("BINARY");
                    cmb_Format.Items.Add("DECIMAL");
                    cmb_Format.Items.Add("CHARACTER");
                    cmb_Format.Items.Add("FLOAT");
                }

                // Set last selected
                if (selected != null && cmb_Format.Items.Contains(selected))
                    cmb_Format.SelectedItem = selected;
                else
                    cmb_Format.SelectedIndex = 0;
            }

        }

        private void txt_Actual_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Read();
        }

        private void cmb_Format_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (addressformatter.IsValid && cmb_Format.SelectedIndex >= 0)
                FormatValue();
        }
    }
}
