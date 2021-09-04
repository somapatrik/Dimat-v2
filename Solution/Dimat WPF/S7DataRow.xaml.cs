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
//using Sharp7;
using Snap7;

namespace Dimat_WPF
{

    public partial class S7DataRow : UserControl
    {
       
        // Connection to PLC
        S7Client client;

        // Address validation & info
        AddressFormatter addressformatter = new AddressFormatter();

        // Address information
        public AddressInfo AddressInfo;

        // Actual value from PLC
        public byte[] array;

        // Value to PLC
        public byte[] writearray;

        #region Selected row property

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

        #endregion

        #region Database for data property

        public string Description 
        { 
            get { return txt_Desc.Text; } 
            set { txt_Desc.Text = value; }
        }
        public string Address 
        { 
            get { return txt_Address.Text; } 
            set 
            { 
                txt_Address.Text = value; 
                txt_Address_LostFocus(txt_Address, null); 
            }
        }
        public string Format 
        { 
            get { return cmb_Format.Text; }
            set
            {
                if (cmb_Format.Items.Contains(value))
                    cmb_Format.SelectedItem = value;
            }
        }

        #endregion

        public bool IsReadValid
        {
            get { return addressformatter.IsValid; }
        }

        public bool IsWriteValid;

        public S7DataRow(ref S7Client PlcClient)
        {
            InitializeComponent();
            client = PlcClient;
        }

        public async void Read()
        {
            await Task.Run(() =>
            {
                GridRow.Dispatcher.Invoke(() =>
                {
                    txt_Actual.Text = "";
                    if (addressformatter.IsValid && client.Connected() && 
                        client.ReadArea(AddressInfo.Area, AddressInfo.DBNumber, AddressInfo.Start, AddressInfo.Amount, AddressInfo.WordLen, array) == 0)
                    {
                        FormatValue();
                        ValidateInputValue();
                    }
                });
            });
        }

        public async void UpdateValue()
        {
            await Task.Run(() =>
            {
                GridRow.Dispatcher.Invoke(() =>
                {
                    txt_Actual.Text = "";
                    if (addressformatter.IsValid && client.Connected())
                    {
                        FormatValue();
                        ValidateInputValue();
                    }
                });
            });
        }

        private async void Write()
        {
            await Task.Run(()=> 
            {
                if (addressformatter.IsValid && client.Connected() && IsWriteValid)
                    client.WriteArea(AddressInfo.Area, AddressInfo.DBNumber, AddressInfo.Start, AddressInfo.Amount, AddressInfo.WordLen, writearray);

            });
        }

        private void SetReading()
        {
            AddressInfo = new AddressInfo();

            if (addressformatter.IsInput)
                AddressInfo.Area = S7Client.S7AreaPE;
            else if (addressformatter.IsOutput)
                AddressInfo.Area = S7Client.S7AreaPA;
            else if (addressformatter.IsMerker)
                AddressInfo.Area = S7Client.S7AreaMK;
            else if (addressformatter.IsDB)
                AddressInfo.Area = S7Client.S7AreaDB;

            if (addressformatter.IsBit)
            {
                AddressInfo.WordLen = S7Client.S7WLBit;
                array = new byte[1];
            }
            else if (addressformatter.IsByte)
            {
                AddressInfo.WordLen = S7Client.S7WLByte;
                array = new byte[1];
            }
            else if (addressformatter.IsWord)
            {
                AddressInfo.WordLen = S7Client.S7WLWord;
                array = new byte[2];
            }
            else if (addressformatter.IsDouble)
            {
                AddressInfo.WordLen = S7Client.S7WLDWord;
                array = new byte[4];
            }


            AddressInfo.Amount = 1;

            if (addressformatter.IsBit)
                AddressInfo.Start = (addressformatter.Byte * 8) + addressformatter.Bit;
            else
                AddressInfo.Start = addressformatter.Byte;

            if (addressformatter.IsDB)
                AddressInfo.DBNumber = addressformatter.DBNumber;
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
            //Read();
        }

        private void cmb_Format_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Only if address makes sense
            if (addressformatter.IsValid && cmb_Format.SelectedIndex >= 0)
            {
                // Format loaded value in bits
                FormatValue();

                // Validate value in input box
                ValidateInputValue();
            }
                
        }

        private void txt_Value_LostFocus(object sender, RoutedEventArgs e)
        {
            if (addressformatter.IsValid)
                ValidateInputValue();
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

        private void ValidateInputValue()
        {
            string input = txt_Value.Text;
            bool result = false;
            writearray = new byte[array.Length];

            // Test values
            bool bool_test;
            byte byte_test;
            int sint_test;
            uint uint_test;
            float float_test;

            if (!string.IsNullOrEmpty(input) && addressformatter.IsValid)
            { 
                try
                {

                    switch (cmb_Format.SelectedValue.ToString())
                    {
                        case "BOOL":
                            bool_test = bool.TryParse(input, out result);
                            if (result)
                                S7.SetBitAt(ref writearray, 0, addressformatter.Bit, bool_test);

                            break;

                        case "BINARY":

                            string clear = "";
                            foreach (char c in input)
                                if (c == '1' || c == '0')
                                    clear += c;                                    

                            switch (array.Length)
                            {
                                case 1:
                                    if (clear.Length == 8)
                                        result = true;
                                        S7.SetByteAt(writearray, 0, Convert.ToByte(clear, 2));
                                    break;
                                case 2:
                                    if (clear.Length == 16)
                                        result = true;
                                        S7.SetWordAt(writearray, 0, Convert.ToUInt16(clear, 2));
                                    break;
                                case 4:
                                    if (clear.Length == 32)
                                        result = true;
                                        S7.SetDWordAt(writearray, 0, Convert.ToUInt32(clear, 2));
                                    break;
                            }
                            break;

                        case "DECIMAL +/-":
                            /*
                            Number of bits  Min.value         Max.value
                                8 bit         -128              127
                                16 bit       –32768           32 767
                                32 bit   –2 147 483 648    2 147 483 647
                            */

                            switch (array.Length)
                            {
                                case 1:

                                    if (int.TryParse(input, out sint_test) && sint_test >= -128 && sint_test < 128)
                                        result = true;
                                    else
                                        result = false;
                                                           
                                    if (result)
                                        S7.SetSIntAt(writearray, 0, sint_test);

                                    break;

                                case 2:

                                    if (int.TryParse(input, out sint_test) && sint_test >= -32768 && sint_test <= 32767)
                                        result = true;
                                    else
                                        result = false;

                                    if (result)
                                        S7.SetIntAt(writearray, 0, (short)sint_test);

                                    break;

                                case 4:

                                    if (int.TryParse(input, out sint_test))
                                        result = true;
                                    else
                                        result = false;

                                    if (result)
                                        S7.SetDIntAt(writearray, 0, sint_test);

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

                            switch (array.Length)
                            {
                                case 1:

                                    if (uint.TryParse(input, out uint_test) && uint_test <= 255)
                                        result = true;
                                    else
                                        result = false;

                                    if (result)
                                        S7.SetUSIntAt(writearray, 0, Byte.Parse(input));
                                            
                                    break;

                                case 2:

                                    if (uint.TryParse(input, out uint_test) && uint_test <= 65535)
                                        result = true;
                                    else
                                        result = false;

                                    if (result)
                                        S7.SetUIntAt(writearray, 0, ushort.Parse(input));

                                    break;

                                case 4:

                                    if (uint.TryParse(input, out uint_test))
                                        result = true;
                                    else
                                        result = false;

                                    S7.SetUDIntAt(writearray, 0, uint_test);

                                    break;
                            }

                            break;

                        case "FLOAT":
                            result = float.TryParse(input, out float_test);

                            if (result)
                                S7.SetRealAt(writearray, 0, float_test);

                            break;
                    }


                    txt_Value.Style = result ? (Style)Resources["AddressBox"] : (Style)Resources["AddressBoxNOK"];
                    IsWriteValid = result;

                }
                catch (Exception ex)
                {
                    IsWriteValid = false;
                }
            }
            else
            {
                IsWriteValid = false;
            }
        }

        private void btnWrite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Write();
        }
    }
}
