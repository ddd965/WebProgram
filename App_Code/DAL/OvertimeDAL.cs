using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using HRMS.Model;

namespace HRMS.DAL
{
    public class OvertimeDAL
    {
        private const string SELECT_ALL = "SELECT * FROM Overtime";
        private const string SELECT_BY_ID = "SELECT * FROM Overtime WHERE OtId = @OtId";
        private const string INSERT = @"INSERT INTO Overtime(EmpId, OtDate, StartTime, EndTime,
                                         OtHours, OtType, Reason, Status)
                                         VALUES(@EmpId,@OtDate,@StartTime,@EndTime,
                                         @OtHours,@OtType,@Reason,@Status);
                                         SELECT SCOPE_IDENTITY()";
        private const string UPDATE = @"UPDATE Overtime SET EmpId=@EmpId, OtDate=@OtDate,
                                         StartTime=@StartTime, EndTime=@EndTime, OtHours=@OtHours,
                                         OtType=@OtType, Reason=@Reason, Status=@Status
                                         WHERE OtId=@OtId";
        private const string DELETE = "DELETE FROM Overtime WHERE OtId=@OtId";

        public static List<Overtime> GetAll()
        {
            var list = new List<Overtime>();
            using (var dr = DBHelper.ExecuteReader(SELECT_ALL))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        public static Overtime GetById(int otId)
        {
            using (var dr = DBHelper.ExecuteReader(SELECT_BY_ID,
                new SqlParameter[] { new SqlParameter("@OtId", otId) }))
            {
                if (dr.Read()) return Map(dr);
            }
            return null;
        }

        public static int Insert(Overtime ot)
        {
            return Convert.ToInt32(DBHelper.ExecuteScalar(INSERT, GetParams(ot)));
        }

        public static int Update(Overtime ot)
        {
            var parameters = new List<SqlParameter>(GetParams(ot));
            parameters.Add(new SqlParameter("@OtId", ot.OtId));
            return DBHelper.ExecuteNonQuery(UPDATE, parameters.ToArray());
        }

        public static int Delete(int otId)
        {
            return DBHelper.ExecuteNonQuery(DELETE,
                new SqlParameter[] { new SqlParameter("@OtId", otId) });
        }

        public static List<Overtime> Query(int? empId, DateTime? startDate, DateTime? endDate,
            string otType, string status)
        {
            var sql = new StringBuilder("SELECT * FROM Overtime WHERE 1=1");
            var parameters = new List<SqlParameter>();

            if (empId.HasValue)
            {
                sql.Append(" AND EmpId = @EmpId");
                parameters.Add(new SqlParameter("@EmpId", empId.Value));
            }
            if (startDate.HasValue)
            {
                sql.Append(" AND OtDate >= @StartDate");
                parameters.Add(new SqlParameter("@StartDate", startDate.Value));
            }
            if (endDate.HasValue)
            {
                sql.Append(" AND OtDate <= @EndDate");
                parameters.Add(new SqlParameter("@EndDate", endDate.Value));
            }
            if (!string.IsNullOrEmpty(otType))
            {
                sql.Append(" AND OtType = @OtType");
                parameters.Add(new SqlParameter("@OtType", otType));
            }
            if (!string.IsNullOrEmpty(status))
            {
                sql.Append(" AND Status = @Status");
                parameters.Add(new SqlParameter("@Status", status));
            }
            sql.Append(" ORDER BY OtDate DESC");

            var list = new List<Overtime>();
            using (var dr = DBHelper.ExecuteReader(sql.ToString(), parameters.ToArray()))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        private static Overtime Map(SqlDataReader dr)
        {
            return new Overtime
            {
                OtId = (int)dr["OtId"],
                EmpId = (int)dr["EmpId"],
                OtDate = (DateTime)dr["OtDate"],
                StartTime = (TimeSpan)dr["StartTime"],
                EndTime = (TimeSpan)dr["EndTime"],
                OtHours = (decimal)dr["OtHours"],
                OtType = dr["OtType"].ToString(),
                Reason = dr["Reason"].ToString(),
                Status = dr["Status"].ToString(),
                CreateTime = (DateTime)dr["CreateTime"]
            };
        }

        private static SqlParameter[] GetParams(Overtime ot)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@EmpId", ot.EmpId),
                new SqlParameter("@OtDate", ot.OtDate),
                new SqlParameter("@StartTime", ot.StartTime),
                new SqlParameter("@EndTime", ot.EndTime),
                new SqlParameter("@OtHours", ot.OtHours),
                new SqlParameter("@OtType", ot.OtType),
                new SqlParameter("@Reason", ot.Reason),
                new SqlParameter("@Status", ot.Status)
            };
        }
    }
}
