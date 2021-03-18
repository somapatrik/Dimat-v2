using Dimat_WPF.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;

namespace Dimat_WPF
{
    class DBGlobal
    {

        public void DeletePlc(int ID)
        {
            DBLite db = new DBLite("delete from S7_PLC where ID=@ID");
            db.AddParameter("ID", ID, DbType.Int32);
            db.Exec();
        }

        public int GetLastGroupID()
        {
            DBLite db = new DBLite("select max(rowid) from PLC_GROUP;");
            DataTable dt = db.ExecTable();
            if (dt.Rows.Count == 0)
                return 0;

            return int.Parse(dt.Rows[0][0].ToString());
        }

        public string GetGroupName(int ID)
        {
            DBLite db = new DBLite("select NAME from PLC_GROUP where ID=@id");
            db.AddParameter("id", ID, DbType.Int32);
            DataTable dt = db.ExecTable();
            if (dt.Rows.Count == 0)
                return "";

            string name = dt.Rows[0]["NAME"].ToString();
            return name;
        }

        public void CreateGroup(string name)
        {
            DBLite db = new DBLite("insert into PLC_GROUP(ID,NAME) values (null, @name)");
            db.AddParameter("name", name.Trim(), DbType.String);
            db.Exec();
        }

        public string GetPlcName(int ID)
        {
            DBLite db = new DBLite("select NAME from S7_PLC where ID=@id");
            db.AddParameter("id", ID, DbType.Int32);
            DataTable dt = db.ExecTable();
            if (dt.Rows.Count > 0)
                return dt.Rows[0]["NAME"].ToString();
            else
                return "";
        }

        public bool IsGroupNameAvailable(string name)
        {
            DBLite db = new DBLite("select * from PLC_GROUP where NAME like @name");
            db.AddParameter("name", name.Trim(), DbType.String);
            DataTable dt = db.ExecTable();
            if (dt.Rows.Count > 0)
                return false;

            return true;
        }

        public List<PlcGroup> GetGroupList()
        {
            List<PlcGroup> lg = new List<PlcGroup>();
            DBLite db = new DBLite("select ID, NAME from PLC_GROUP");
            DataTable dt = db.ExecTable();
            foreach(DataRow row in dt.Rows)
            {
                PlcGroup gr = new PlcGroup();
                gr.ID = int.Parse(row["ID"].ToString());
                gr.Name = row["NAME"].ToString();
                lg.Add(gr);
            }
            return lg;
        }

        public bool IsUpdatePlcNameAvailable(string name, int ID)
        {
            DBLite db = new DBLite("SELECT * FROM S7_PLC WHERE NAME = @name AND ID<>@ID");
            db.AddParameter("name", name.Trim(), DbType.String);
            db.AddParameter("ID", ID, DbType.Int32);
            DataTable dt = db.ExecTable();
            return dt.Rows.Count > 0 ? false : true;
        }

        public bool IsPlcNameAvailable(string name)
        {
            DBLite db = new DBLite("select * from S7_PLC where NAME like @name");
            db.AddParameter("name", name.Trim(), DbType.String);
            DataTable dt = db.ExecTable();
            return dt.Rows.Count > 0 ? false : true;
        }
        public bool IsUpdatePlcIpAvailable(string ip, int ID)
        {
            DBLite db = new DBLite("select * from S7_PLC where IP like @ip AND ID<>@ID");
            db.AddParameter("ip", ip.Trim(), DbType.String);
            db.AddParameter("ID", ID, DbType.Int32);
            DataTable dt = db.ExecTable();
            return dt.Rows.Count > 0 ? false : true;
        }

        public bool IsPlcIpAvailable(string ip)
        {
            DBLite db = new DBLite("select * from S7_PLC where IP like @ip");
            db.AddParameter("ip", ip.Trim(), DbType.String);
            DataTable dt = db.ExecTable();
            return dt.Rows.Count > 0 ? false : true;
        }

        /// <summary>
        /// Gets all PLC in a group or without a group
        /// </summary>
        /// <param name="group_id">none or 0 = no group</param>
        /// <returns></returns>
        public DataTable GetPLClist(int group_id = 0)
        {
            DBLite db = new DBLite("");

            if (group_id != 0)
            { 
                db = new DBLite("select * from S7_PLC where GROUP_ID = @groupid");
                db.AddParameter("groupid", group_id, DbType.Int32);
            }
            else 
            { 
                db = new DBLite("select * from S7_PLC where GROUP_ID is null");
            }

            DataTable dt = db.ExecTable();
            return dt;
        }
    }
}
