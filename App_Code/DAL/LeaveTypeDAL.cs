using System.Collections.Generic;
using System.Data.SqlClient;
using HRMS.Model;

namespace HRMS.DAL
{
    public class LeaveTypeDAL
    {
        private const string SELECT_ALL = "SELECT * FROM LeaveType";
        private const string SELECT_BY_ID = "SELECT * FROM LeaveType WHERE LeaveTypeId = @LeaveTypeId";
        private const string INSERT = @"INSERT INTO LeaveType(TypeName, DefaultDays, NeedApproval, Descn)
                                         VALUES(@TypeName,@DefaultDays,@NeedApproval,@Descn);
                                         SELECT SCOPE_IDENTITY()";
        private const string UPDATE = @"UPDATE LeaveType SET TypeName=@TypeName, DefaultDays=@DefaultDays,
                                         NeedApproval=@NeedApproval, Descn=@Descn
                                         WHERE LeaveTypeId=@LeaveTypeId";
        private const string DELETE = "DELETE FROM LeaveType WHERE LeaveTypeId=@LeaveTypeId";

        public static List<LeaveType> GetAll()
        {
            var list = new List<LeaveType>();
            using (var dr = DBHelper.ExecuteReader(SELECT_ALL))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        public static LeaveType GetById(int leaveTypeId)
        {
            using (var dr = DBHelper.ExecuteReader(SELECT_BY_ID,
                new SqlParameter[] { new SqlParameter("@LeaveTypeId", leaveTypeId) }))
            {
                if (dr.Read()) return Map(dr);
            }
            return null;
        }

        public static int Insert(LeaveType lt)
        {
            return System.Convert.ToInt32(DBHelper.ExecuteScalar(INSERT, GetParams(lt)));
        }

        public static int Update(LeaveType lt)
        {
            var parameters = new List<SqlParameter>(GetParams(lt));
            parameters.Add(new SqlParameter("@LeaveTypeId", lt.LeaveTypeId));
            return DBHelper.ExecuteNonQuery(UPDATE, parameters.ToArray());
        }

        public static int Delete(int leaveTypeId)
        {
            return DBHelper.ExecuteNonQuery(DELETE,
                new SqlParameter[] { new SqlParameter("@LeaveTypeId", leaveTypeId) });
        }

        private static LeaveType Map(SqlDataReader dr)
        {
            return new LeaveType
            {
                LeaveTypeId = (int)dr["LeaveTypeId"],
                TypeName = dr["TypeName"].ToString(),
                DefaultDays = (decimal)dr["DefaultDays"],
                NeedApproval = (bool)dr["NeedApproval"],
                Descn = dr["Descn"] as string
            };
        }

        private static SqlParameter[] GetParams(LeaveType lt)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@TypeName", lt.TypeName),
                new SqlParameter("@DefaultDays", lt.DefaultDays),
                new SqlParameter("@NeedApproval", lt.NeedApproval),
                new SqlParameter("@Descn", (object)lt.Descn ?? System.DBNull.Value)
            };
        }
    }
}
