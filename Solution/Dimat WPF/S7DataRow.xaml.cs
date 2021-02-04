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

        public S7DataRow(ref S7Client PlcClient)
        {
            InitializeComponent();
            client = PlcClient;
        }
        
        private void Read()
        {
            if (addressformatter.IsValid)
            {
                int Area;
                int DBNumber;
                int Start;
                int Amount;
                int WordLen;
                byte[] array;


                //client.AsReadArea()
                // read;
            }
        }

        private void txt_Address_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            string input = box.Text;

            addressformatter.Address = input;

            box.Text = addressformatter.Address;
            box.Style = addressformatter.IsValid ? (Style)Resources["AddressBox"] : (Style)Resources["AddressBoxNOK"];

            CreateFormatMenu();
        }

        private void CreateFormatMenu()
        {
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

                cmb_Format.SelectedIndex = 0;
            }

        }
    }
}
