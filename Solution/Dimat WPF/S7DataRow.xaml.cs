﻿using Dimat_WPF.Class;
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
        
        private void Read()
        {
            if (addressformatter.IsValid && client.ReadArea(Area, DBNumber, Start, Amount, WordLen, array) == 0)
            {
                txt_Actual.Text = array.ToString();  
            }
            else
            {
                txt_Actual.Text = "";
            }
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
