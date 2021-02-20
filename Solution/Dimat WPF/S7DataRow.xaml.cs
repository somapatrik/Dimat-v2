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
        // Row selected
        private bool _selected;
        public bool Selected
        {
            set 
            {
                if (value)
                    SelectRow();
                else
                    Unselectrow();
            }
            
            get { return _selected; }
        }

        // Connection to PLC
        S7Client client;

        // Address validation & info
        AddressFormatter addressformatter = new AddressFormatter();

        // Address information
        int Area;
        int DBNumber;
        int Start;
        int Amount;
        int WordLen;
        byte[] array;

        // Write information
        bool IsWriteValid;

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

        private async void Write()
        {

        }

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
            else if (addressformatter.IsWord)
            {
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
                    cmb_Format.Items.Add("DECIMAL +/-");
                    cmb_Format.Items.Add("DECIMAL");
                    cmb_Format.Items.Add("CHARACTER");
                }
                else if (addressformatter.IsDouble)
                {
                    cmb_Format.Items.Add("BINARY");
                    cmb_Format.Items.Add("DECIMAL +/-");
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

        #region Format functions

        private void FormatValue()
        {
            switch (cmb_Format.SelectedValue.ToString())
            {
                case "BOOL":
                    txt_Actual.Text = GetBooltS();
                    break;
                case "BINARY":
                    txt_Actual.Text = GetBinS();
                    break;
                case "DECIMAL":
                    txt_Actual.Text = GetUDecS();
                    break;
                case "DECIMAL +/-":
                    txt_Actual.Text = GetSDecS();
                    break;
                case "CHARACTER":
                    txt_Actual.Text = GetCharS();
                    break;
                case "FLOAT":
                    txt_Actual.Text = GetFloatS();
                    break;
            }
        }

        public string GetBooltS()
        {
            Boolean b = S7.GetBitAt(array, 0, 0);
            return b.ToString();
        }

        public string GetSDecS()
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

        public string GetUDecS()
        {
            string i = "[Error]";

            switch (array.Length)
            {
                case 1:
                    i = S7.GetUSIntAt(array, 0).ToString();
                    break;
                case 2:
                    i = S7.GetUIntAt(array, 0).ToString();
                    break;
                case 4:
                    i = S7.GetUDIntAt(array, 0).ToString();
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

        #region GUI events
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

        private void txt_Actual_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Read();
        }

        private void cmb_Format_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (addressformatter.IsValid && cmb_Format.SelectedIndex >= 0)
                FormatValue();
        }

        #endregion

        #region Select / Deselect row

        private void lblSelect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_selected)
                Unselectrow();
            else
                SelectRow();
        }

        private void SelectRow()
        {
            _selected =  true;
            lblSelect.Style =  (Style)Resources["RowButtonSelected"] ;
        }

        private void Unselectrow()
        {
            _selected = false;
            lblSelect.Style = (Style)Resources["RowButton"];
        }

        #endregion

        private void txt_Value_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateInputValue();
        }

        private void ValidateInputValue()
        {
            string input = txt_Value.Text;
            bool result = false;

            if (!string.IsNullOrEmpty(input))
            { 
                try
                {

                    switch (cmb_Format.SelectedValue.ToString())
                    {
                        case "BOOL":
                            bool bool_test = bool.TryParse(input, out result);
                            break;

                        case "DECIMAL +/-":
                            /*
                            Number of bits  Min.value     Max.value
                                8 bit         -128           127
                                16 bit       –32768         32767
                                32 bit     –2147483648   2147483647
                            */
                            int sint_test;

                            switch (array.Length)
                            {
                                case 1:

                                    if (int.TryParse(input, out sint_test) && sint_test >= -128 && sint_test < 128)
                                        result = true;
                                    else
                                        result = false;
                                                                               

                                    break;

                                case 2:

                                    if (int.TryParse(input, out sint_test) && sint_test >= -32768 && sint_test < 32767)
                                        result = true;
                                    else
                                        result = false;

                                    break;

                                case 4:

                                    if (int.TryParse(input, out sint_test))
                                        result = true;
                                    else
                                        result = false;

                                    break;

                            }

                            break;
                        case "DECIMAL":
                            /*
                             Number of bits  Min.value     Max.value
                                 8 bit          0             255
                                 16 bit         0           65 535
                                 32 bit         0        4 294 967 295
                             */
                            uint uint_test;

                            switch (array.Length)
                            {
                                case 1:

                                    if (uint.TryParse(input, out uint_test) && uint_test < 255)
                                        result = true;
                                    else
                                        result = false;


                                    break;

                                case 2:

                                    if (uint.TryParse(input, out uint_test) && uint_test < 65535)
                                        result = true;
                                    else
                                        result = false;

                                    break;

                                case 4:

                                    if (uint.TryParse(input, out uint_test))
                                        result = true;
                                    else
                                        result = false;

                                    break;

                            }

                            break;

                        case "FLOAT":
                            float ftest;
                            result = float.TryParse(input, out ftest);
                            break;
                    }

                }
                catch (Exception ex)
                {
                    IsWriteValid = false;
                }
            }
        }
    }
}
