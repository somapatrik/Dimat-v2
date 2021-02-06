using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dimat_WPF.Class
{
    class AddressFormatter
    {

        #region Private properties

        private string RawAddress;
        private bool _Valid;

        private int _Byte;
        private int _Bit;
        private int _DBNumber;

        private bool ibit;
        private bool ibyte;
        private bool iword;
        private bool idouble;

        private bool qbit;
        private bool qbyte;
        private bool qword;
        private bool qdouble;

        private bool mbit;
        private bool mbyte;
        private bool mword;
        private bool mdouble;

        private bool dbbit;
        private bool dbbyte;
        private bool dbword;
        private bool dbdouble;

        private bool _IsInput;
        private bool _IsOutput;
        private bool _IsMerker;
        private bool _IsDB;

        #endregion

        #region Public properties

        public bool IsValid
        {
            get { return _Valid; }
        }

        public bool IsBit
        {
            get{  return ibit || qbit || mbit || dbbit; }
        }

        public bool IsByte
        {
            get{ return ibyte || qbyte || mbyte || dbbyte; }
        }

        public bool IsWord
        {
            get{return iword || qword || mword || dbword;}
        }

        public bool IsDouble
        {
            get{return idouble || qdouble || mdouble || dbdouble; }
        }

        public bool IsInput
        {
            get { return _IsInput; }
        }

        public bool IsOutput
        {
            get { return _IsOutput; }
        }

        public bool IsMerker
        {
            get { return _IsMerker; }
        }

        public bool IsDB
        {
            get { return _IsDB; }
        }

        public int Byte
        {
            get { return _Byte; }
        }

        public int Bit
        {
            get { return _Bit; }
        }

        public int DBNumber
        {
            get { return _DBNumber; }
        }

        public string Address
        {
            set
            {
                RawAddress = value.ToUpper().Trim().Replace(" ", "");
                CheckAddress();
                if (_Valid)
                {
                    GetByte();
                    if (IsBit)
                        GetBit();
                    else
                        _Bit = 0;

                    if (_IsDB)
                        GetDBNumber();
                    else
                        _DBNumber = 0;

                }else
                {
                    _Byte = 0;
                    _Bit = 0;
                }
            }
            get
            {
                return RawAddress;
            }
        }

        #endregion

        
        private void GetDBNumber()
        {
            // int.TryParse( RawAddress.Substring(1, RawAddress.IndexOf('.')) ,out _DBNumber);
            int.TryParse( (RawAddress.Split('.')[0]).Substring(2) ,out _DBNumber);
        }

        private void GetByte()
        {
            int Byte;

            if (_IsInput || _IsOutput || _IsMerker)
                int.TryParse(RawAddress.Substring(2),out Byte);
            else
                int.TryParse(RawAddress.Split('.')[1].Substring(3), out Byte);

            _Byte = Byte;
        }

        private void GetBit()
        {
            int Bit;

            if (_IsInput || _IsOutput || _IsMerker)
                int.TryParse(RawAddress.Split('.')[1], out Bit);
            else
                int.TryParse(RawAddress.Split('.')[2], out Bit);

            _Bit = Bit;
        }

        private void CheckAddress()
        {
            Regex InputBit = new Regex(@"[I]\d+[.][0-7]$", RegexOptions.IgnoreCase);
            Regex InputByte = new Regex(@"[I][B]\d+$", RegexOptions.IgnoreCase);
            Regex InputWord = new Regex(@"[I][W]\d+$", RegexOptions.IgnoreCase);
            Regex InputDouble = new Regex(@"[I][D]\d+$", RegexOptions.IgnoreCase);

            Regex OutputBit = new Regex(@"[Q]\d+[.][0-7]$", RegexOptions.IgnoreCase);
            Regex OutputByte = new Regex(@"[Q][B]\d+$", RegexOptions.IgnoreCase);
            Regex OutputWord = new Regex(@"[Q][W]\d+$", RegexOptions.IgnoreCase);
            Regex OutputDouble = new Regex(@"[Q][D]\d+$", RegexOptions.IgnoreCase);

            Regex MerkerBit = new Regex(@"[M]\d+[.][0-7]$", RegexOptions.IgnoreCase);
            Regex MerkerByte = new Regex(@"[M][B]\d+$", RegexOptions.IgnoreCase);
            Regex MerkerWord = new Regex(@"[M][W]\d+$", RegexOptions.IgnoreCase);
            Regex MerkerDouble = new Regex(@"[M][D]\d+$", RegexOptions.IgnoreCase);

            Regex DBBit = new Regex(@"\DB\d+.DBX\d+[.][0-7]$", RegexOptions.IgnoreCase);
            Regex DBByte = new Regex(@"\DB\d+.DB[B]\d+$", RegexOptions.IgnoreCase);
            Regex DBWord = new Regex(@"\DB\d+.DB[W]\d+$", RegexOptions.IgnoreCase);
            Regex DBDouble = new Regex(@"\DB\d+.DB[D]\d+$", RegexOptions.IgnoreCase);

            ibit = InputBit.IsMatch(RawAddress) ? true : false;
            ibyte = InputByte.IsMatch(RawAddress) ? true : false;
            iword = InputWord.IsMatch(RawAddress) ? true : false;
            idouble = InputDouble.IsMatch(RawAddress) ? true : false;

            if (ibit || ibyte || iword || idouble)
            {
                _IsInput = true;
                //_Valid = true;
            } else
            {
                _IsInput = false;
                //_Valid = false;
            }

            qbit = OutputBit.IsMatch(RawAddress) ? true : false;
            qbyte = OutputByte.IsMatch(RawAddress) ? true : false;
            qword = OutputWord.IsMatch(RawAddress) ? true : false;
            qdouble = OutputDouble.IsMatch(RawAddress) ? true : false;

            if (qbit || qbyte || qword || qdouble)
            {
                _IsOutput = true;
                //_Valid = true;
            }
            else
            {
                _IsOutput = false;
                //_Valid = false;
            }

            mbit = MerkerBit.IsMatch(RawAddress) ? true : false;
            mbyte = MerkerByte.IsMatch(RawAddress) ? true : false;
            mword = MerkerWord.IsMatch(RawAddress) ? true : false;
            mdouble = MerkerDouble.IsMatch(RawAddress) ? true : false;

            if (mbit || mbyte || mword || mdouble)
            {
                _IsMerker = true;
                //_Valid = true;
            }
            else
            {
                _IsMerker = false;
                //_Valid = false;
            }

            dbbit = DBBit.IsMatch(RawAddress) ? true : false;
            dbbyte = DBByte.IsMatch(RawAddress) ? true : false;
            dbword = DBWord.IsMatch(RawAddress) ? true : false;
            dbdouble = DBDouble.IsMatch(RawAddress) ? true : false;

            if (dbbit || dbbyte || dbword || dbdouble)
            {
                _IsDB = true;
                //_Valid = true;
            }
            else
            {
                _IsDB = false;
                //_Valid = false;
            }

            if (_IsInput ^ _IsOutput ^ _IsMerker ^ _IsDB)
                _Valid = true;
            else
                _Valid = false;

        }

    }
}

