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

        #region PLC data rows

        public void DeleteGroup(int GROUP_ID)
        {
            DBLite db = new DBLite("UPDATE S7_PLC SET GROUP_ID=NULL WHERE GROUP_ID=@groupid");
            db.AddParameter("groupid", GROUP_ID, DbType.Int32);
            db.Exec();

            db = new DBLite("DELETE FROM PLC_GROUP WHERE ID=@groupid");
            db.AddParameter("groupid", GROUP_ID, DbType.Int32);
            db.Exec();

        }

        public DataTable LoadRows(int PLC_ID)
        {
            DBLite db = new DBLite("SELECT DESCRIPTION, ADDRESS, FORMAT FROM S7_PLC_Signal WHERE PLC=@id");
            db.AddParameter("id", PLC_ID, DbType.Int32);
            DataTable dt = db.ExecTable();
            return dt;

        }

        public void DeleteNewRows(int PLC_ID)
        {
            DBLite db = new DBLite("DELETE FROM S7_PLC_Signal WHERE PLC=@id AND NEW=true");
            db.AddParameter("id", PLC_ID, DbType.Int32);
            db.Exec();
        }

        public void DeleteOldRows(int PLC_ID)
        {
            DBLite db = new DBLite("DELETE FROM S7_PLC_Signal WHERE PLC=@id AND NEW=false");
            db.AddParameter("id", PLC_ID, DbType.Int32);
            db.Exec();
        }

        public void MarkOldRows(int PLC_ID)
        {
            DBLite db = new DBLite("UPDATE S7_PLC_Signal SET NEW=false where PLC=@id");
            db.AddParameter("id", PLC_ID, DbType.Int32);
            db.Exec();
        }

        public void SaveRow(int PLC_ID, string Desc, string Address, string Format)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("INSERT INTO S7_PLC_Signal (");
            query.AppendLine("PLC,");
            query.AppendLine("ADDRESS,");
            query.AppendLine("DESCRIPTION,");
            query.AppendLine("FORMAT,");
            query.AppendLine("NEW");
            query.AppendLine(")VALUES(");
            query.AppendLine("@id,");
            query.AppendLine("@address,");
            query.AppendLine("@desc,");
            query.AppendLine("@format,");
            query.AppendLine("true");
            query.AppendLine(");");

            DBLite db = new DBLite(query.ToString());
            db.AddParameter("id", PLC_ID, DbType.Int32);
            db.AddParameter("address", Address, DbType.String);
            db.AddParameter("desc", Desc, DbType.String);
            db.AddParameter("format", Format, DbType.String);

            db.Exec();
        }
        #endregion

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
