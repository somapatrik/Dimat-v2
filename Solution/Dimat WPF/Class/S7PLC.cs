using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp7;

namespace Dimat_WPF.Class
{
    public class S7PLC
    {
        private string _IP;
        private int _Rack;
        private int _Slot;
        private string _PlcType;
        private string _Name;
        private string _Description;
        private int _id;
        private S7Client client;
        private int _id_group;

        #region Constructors

        public S7PLC(int ID)
        {
            _id = ID;
            LoadFromDB();
        }

        public S7PLC(string Name, string Adrress, string PlcType)
        {
            _PlcType = PlcType;
            switch (_PlcType)
            {
                case "S7-300":
                    SetPlc(Name, Adrress, 0, 2);
                    break;
                case "S7-400":
                case "S7-1200":
                case "S7-1500":
                    SetPlc(Name, Adrress, 0, 0);
                    break;
            }
        }

        public S7PLC(string Name, string Adrress, int Rack, int Slot, string Type)
        {
            Rack = Rack < 0 ? 0 : Rack;
            Slot = Slot < 0 ? 0 : Slot;

            _PlcType = Type;

            SetPlc(Name, Adrress, Rack, Slot);
        }

        public S7PLC(string Name, string Adrress, int Rack, int Slot)
        {
            Rack = Rack < 0 ? 0 : Rack;
            Slot = Slot < 0 ? 0 : Slot;

            SetPlc(Name, Adrress, Rack, Slot);
        }

        private void SetPlc(string Name, string Adrress, int Rack, int Slot)
        {
            this._Name = Name;
            this._IP = Adrress;
            this._Rack = Rack;
            this._Slot = Slot;

            this.client = new S7Client();
        }

        #endregion

        #region Public properties

        public string Name 
        { 
            get { return _Name; }
            set { _Name = value; }
        }

        public int ID
        {
            get { return _id; }
        }

        public string Description
        {
            set { _Description = value; }
            get { return _Description; }
        }

        public string IP
        {
            get { return _IP; }
            set { _IP = value; }
        }

        public int Slot
        {
            get { return _Slot; }
            set { _Slot = value; }
        }

        public int Rack
        {
            get { return _Rack; }
            set { _Rack = value; }
        }

        public string Type
        {
            set { _PlcType = value; }
            get { return _PlcType; }
        }

        public string TypeName
        {
            get { return _PlcType.ToUpper(); }
        }

        public Boolean Connected
        {
            get { return client.Connected; }
        }


        #endregion

        #region Database

        private void LoadFromDB()
        {
            DBLite db = new DBLite("SELECT ID, NAME, DESCRIPTION, IP, RACK, SLOT, TYPE, GROUP_ID FROM S7_PLC where ID=@id;");
            db.AddParameter("id", _id, DbType.Int32);
            DataTable dt = db.ExecTable();
            if (dt.Rows.Count > 0)
            {
                _Name = dt.Rows[0]["NAME"].ToString();
                _IP = dt.Rows[0]["IP"].ToString();
                _Description = dt.Rows[0]["DESCRIPTION"].ToString();
                _Rack = int.Parse(dt.Rows[0]["RACK"].ToString());
                _Slot = int.Parse(dt.Rows[0]["SLOT"].ToString());
                _PlcType = dt.Rows[0]["TYPE"].ToString();
                _id_group = dt.Rows[0]["GROUP_ID"].Equals(DBNull.Value) ? 0 : int.Parse(dt.Rows[0]["GROUP_ID"].ToString());
            }
        }

        private int GetLastPlcID()
        {
            int ret;
            DBLite db = new DBLite("select max(rowid) from S7_PLC;");
            DataTable dt = db.ExecTable();
            int.TryParse(dt.Rows[0][0].ToString(),out ret);
            return ret;
        }

        public void Insert()
        {
            
            StringBuilder query = new StringBuilder();
            query.Append("INSERT INTO S7_PLC (ID,NAME,DESCRIPTION,IP,RACK,SLOT,TYPE)");
            query.AppendLine("VALUES(null,@NAME,@DESCRIPTION,@IP,@RACK,@SLOT,@TYPE);");
            DBLite db = new DBLite(query.ToString());
            db.AddParameter("NAME", _Name, System.Data.DbType.String);
            db.AddParameter("DESCRIPTION", _Description, System.Data.DbType.String);
            db.AddParameter("IP", _IP, System.Data.DbType.String);
            db.AddParameter("RACK", _Rack, System.Data.DbType.String);
            db.AddParameter("SLOT", _Slot, System.Data.DbType.String);
            db.AddParameter("TYPE", _PlcType, System.Data.DbType.String);
            db.Exec();

            _id = GetLastPlcID();
        }

        public void Update()
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("UPDATE S7_PLC");
            query.AppendLine("SET");
            query.AppendLine("NAME = @NAME,");
            query.AppendLine("DESCRIPTION = @DESCRIPTION,");
            query.AppendLine("IP = @IP,");
            query.AppendLine("RACK = @RACK,");
            query.AppendLine("SLOT = @SLOT,");
            query.AppendLine("TYPE = @TYPE");
            query.AppendLine("WHERE ID = @ID");

            DBLite db = new DBLite(query.ToString());
            db.AddParameter("ID", _id, DbType.Int32);
            db.AddParameter("NAME", _Name, DbType.String);
            db.AddParameter("DESCRIPTION", _Description, DbType.String);
            db.AddParameter("IP", _IP, DbType.String);
            db.AddParameter("RACK", _Rack, DbType.String);
            db.AddParameter("SLOT", _Slot, DbType.String);
            db.AddParameter("TYPE", _PlcType, DbType.String);

            db.Exec();

            LoadFromDB();
        }

        public bool IsGroupAvailable(int ID_Group)
        {
            if (ID_Group == 0)
                return true;
                            
            DBLite db = new DBLite("select * from PLC_GROUP where ID=@id");
            db.AddParameter("id", ID_Group, DbType.Int32);
            DataTable dt = db.ExecTable();
            if (dt.Rows.Count > 0)
                return true;
            else
                return false;
            
        }

        public void UpdateGroup(int ID_Group)
        {
            string query = "UPDATE S7_PLC SET GROUP_ID=@group where ID=@id";

            if (ID_Group == 0)
                query = "UPDATE S7_PLC SET GROUP_ID=null where ID=@id";

            DBLite db = new DBLite(query);
            db.AddParameter("id", _id, DbType.Int32);

            if (ID_Group > 0)
                db.AddParameter("group", ID_Group, DbType.Int32);

            db.Exec();
            LoadFromDB();
        }

        #endregion

        #region Public function

        public Boolean Connect()
        {
            if (client.ConnectTo(_IP, _Rack, _Slot) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Disconnect()
        {
            client.Disconnect();
        }

        #endregion

        #region Value reading
        public byte[] GetValue(string ValueRaw)
        {
            string Value = ValueRaw.Trim();

            Boolean Input = Value.StartsWith("I");
            Boolean Output = Value.StartsWith("Q");
            Boolean M = Value.StartsWith("M");
            Boolean DB = Value.StartsWith("DB");

            //Type of memory
            int Area = 0;
            // Number of bytes to be red
            int BufferSize;
            // How many entities will be red
            int Amount = 1;
            // Where reading starts (if bits = number of bit)
            // 10.3 = (10 * 8) + 3
            int Start;
            // 0 - 7 Position of the bit inside a byte
            int BitPosition;
            // DB number, is ignored if not reading DB
            int DBNumber = 0;

            int WordLen = 0;

            // Set type of variable to read
            if (Input) { Area = S7Consts.S7AreaPE; }
            if (Output) { Area = S7Consts.S7AreaPA; }
            if (M) { Area = S7Consts.S7AreaMK; }
            if (DB) { Area = S7Consts.S7AreaDB; }

            // Do I read bit?
            Boolean ReadBit = ReadingBit(DB, Value);

            // Set Buffer Size
            BufferSize = SetBufferSize(ReadBit, DB, Value);

            // Start adrress
            Start = SetStartAdrress(ReadBit, DB, Value);

            // Bit position
            if (ReadBit)
            {
                BitPosition = SetBitPosition(DB, Value);
                Start = Start + BitPosition;
            }

            // Set DBnummber
            if (DB)
            {
                DBNumber = SetDBnummber(Value);
            }

            // WordLen
            WordLen = SetWordLen(ReadBit, BufferSize);

            // Raw bytes
            byte[] buffer = new byte[BufferSize];
            buffer = GetBuffer(Area, BufferSize, DBNumber, Start, Amount, WordLen);

            return buffer;

        }

        public string GetBooltS(byte[] buffer)
        {
            Boolean b = S7.GetBitAt(buffer, 0, 0);
            return b.ToString();
        }

        public string GetDecS(byte[] buffer)
        {
            string i = "[Error]";

            switch (buffer.Length)
            {
                case 1:
                    i = S7.GetSIntAt(buffer, 0).ToString();
                    break;
                case 2:
                    i = S7.GetIntAt(buffer, 0).ToString();
                    break;
                case 4:
                    i = S7.GetDIntAt(buffer, 0).ToString();
                    break;
            }

            return i;
        }

        public string GetCharS(byte[] buffer)
        {
            string s = "";
            if (buffer.Length > 0)
                s = S7.GetCharsAt(buffer, 0, buffer.Length);

            return s;
        }

        public string GetBinS(byte[] buffer)
        {
            string s = string.Join(" ", buffer.Select(x => Convert.ToString(x, 2).PadLeft(8, '0')));
            return s;
        }

        public string GetFloatS(byte[] buffer)
        {
            string i = "";
            switch (buffer.Length)
            {
                case 4:
                    i = S7.GetRealAt(buffer, 0).ToString();
                    break;
            }
            return i;
        }

        private byte[] GetBuffer(int S7Area, int BufferSize, int DBNumber, int Start, int Amount, int WordLen)
        {
            byte[] buffer = new byte[BufferSize];
            client.ReadArea(S7Area, DBNumber, Start, Amount, WordLen, buffer);
            return buffer;
        }

        private int SetWordLen(Boolean ReadBit, int BufferSize)
        {
            int WordLen = 0;

            if (ReadBit)
            {
                WordLen = S7Consts.S7WLBit;
            }
            else
            {
                WordLen = BufferSize == 1 ? S7Consts.S7WLByte : 0;
                WordLen = BufferSize == 2 ? S7Consts.S7WLWord : WordLen;
                WordLen = BufferSize == 4 ? S7Consts.S7WLDWord : WordLen;
            }

            return WordLen;
        }

        private int SetDBnummber(string Value)
        {
            return Int32.Parse(Value.Split('.')[0].Substring(2));

        }

        private int SetBitPosition(Boolean DB, string Value)
        {
            if (!DB)
            {
                return Int32.Parse(Value.Split('.')[1]);
            }
            else
            {
                return Int32.Parse(Value.Split('.')[2]);
            }
        }

        private int SetStartAdrress(Boolean ReadBit, Boolean DB, string Value)
        {
            if (!ReadBit)
            {
                if (!DB)
                {
                    return Int32.Parse(Value.Substring(2));
                }
                else
                {
                    return Int32.Parse(Value.Split('.')[1].Substring(3));
                }

            }
            else
            {
                // Start adrress for reading bit
                if (!DB)
                {
                    return (Int32.Parse(Value.Split('.')[0].Substring(1)) * 8);
                }
                else
                {
                    return (Int32.Parse(Value.Split('.')[1].Substring(3)) * 8);
                }
            }
        }

        private int SetBufferSize(Boolean ReadBit, Boolean DB, string Value)
        {
            int val = 0;

            if (ReadBit)
            {
                val = 1;
            }
            else
            {

                string ReadLetter;

                if (!DB)
                {
                    // I[B], I[W], I[D]
                    ReadLetter = Value.Substring(1, 1);
                }
                else
                {
                    // DB100.DB[B]1
                    ReadLetter = Value.Split('.')[1].Substring(2, 1);
                }

                if (ReadLetter == "B") { val = 1; }
                if (ReadLetter == "W") { val = 2; }
                if (ReadLetter == "D") { val = 4; }
            }

            return val;
        }

        private Boolean ReadingBit(Boolean DB, string Value)
        {
            // I1.1, M50.4
            if (!DB && Value.Contains("."))
            {
                return true;
            }

            // [DB100].[DBX1].[0]
            if (DB)
            {
                if (Value.Split('.').Length > 2)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
