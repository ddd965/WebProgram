using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using HRMS.Model;

namespace HRMS.DAL
{
    public class AttendanceParamDAL
    {
        private const string SELECT_TOP1 = "SELECT TOP 1 * FROM AttendanceParam ORDER BY ParamId DESC";
        private const string INSERT = @"INSERT INTO AttendanceParam(WorkStartTime, WorkEndTime,
            LateToleranceMinutes, LeaveEarlyToleranceMinutes, HalfDayHours, SaturdayWork, SundayWork)
            VALUES(@WorkStartTime,@WorkEndTime,@LateToleranceMinutes,@LeaveEarlyToleranceMinutes,
            @HalfDayHours,@SaturdayWork,@SundayWork); SELECT SCOPE_IDENTITY()";
        private const string UPDATE = @"UPDATE AttendanceParam SET
            WorkStartTime=@WorkStartTime, WorkEndTime=@WorkEndTime,
            LateToleranceMinutes=@LateToleranceMinutes, LeaveEarlyToleranceMinutes=@LeaveEarlyToleranceMinutes,
            HalfDayHours=@HalfDayHours, SaturdayWork=@SaturdayWork, SundayWork=@SundayWork,
            UpdateTime=GETDATE()
            WHERE ParamId=@ParamId";

        public static AttendanceParam GetDefault()
        {
            try
            {
                using (var dr = DBHelper.ExecuteReader(SELECT_TOP1))
                {
                    if (dr.Read()) return Map(dr);
                }
            }
            catch
            {
                // 表可能不存在或未初始化，返回 null
            }
            return null;
        }

        public static int Insert(AttendanceParam p)
        {
            var pars = GetParams(p);
            return Convert.ToInt32(DBHelper.ExecuteScalar(INSERT, pars));
        }

        public static int Update(AttendanceParam p)
        {
            var list = new List<SqlParameter>(GetParams(p));
            list.Add(new SqlParameter("@ParamId", p.ParamId));
            return DBHelper.ExecuteNonQuery(UPDATE, list.ToArray());
        }

        private static AttendanceParam Map(SqlDataReader dr)
        {
            return new AttendanceParam
            {
                ParamId = (int)dr["ParamId"],
                // 兼容存储类型：可能是 DateTime 或字符串/TimeSpan
                WorkStartTime = ToDateTime(dr["WorkStartTime"]),
                WorkEndTime = ToDateTime(dr["WorkEndTime"]),
                LateToleranceMinutes = dr["LateToleranceMinutes"] as int? ?? Convert.ToInt32(dr["LateToleranceMinutes"]),
                LeaveEarlyToleranceMinutes = dr["LeaveEarlyToleranceMinutes"] as int? ?? Convert.ToInt32(dr["LeaveEarlyToleranceMinutes"]),
                HalfDayHours = Convert.ToDecimal(dr["HalfDayHours"]),
                SaturdayWork = Convert.ToBoolean(dr["SaturdayWork"]),
                SundayWork = Convert.ToBoolean(dr["SundayWork"]),
                UpdateTime = dr["UpdateTime"] as DateTime?
            };
        }

        private static DateTime ToDateTime(object v)
        {
            if (v == null || v == DBNull.Value) return DateTime.Today;
            if (v is DateTime) return (DateTime)v;
            if (v is TimeSpan) return DateTime.Today + (TimeSpan)v;
            TimeSpan ts;
            if (TimeSpan.TryParse(v.ToString(), out ts)) return DateTime.Today + ts;
            DateTime dt;
            if (DateTime.TryParse(v.ToString(), out dt)) return dt;
            return DateTime.Today;
        }

        private static SqlParameter[] GetParams(AttendanceParam p)
        {
            return new SqlParameter[]
            {
                // 存 TimeSpan 格式，避免日期干扰
                new SqlParameter("@WorkStartTime", p.WorkStartTime.TimeOfDay),
                new SqlParameter("@WorkEndTime", p.WorkEndTime.TimeOfDay),
                new SqlParameter("@LateToleranceMinutes", p.LateToleranceMinutes),
                new SqlParameter("@LeaveEarlyToleranceMinutes", p.LeaveEarlyToleranceMinutes),
                new SqlParameter("@HalfDayHours", p.HalfDayHours),
                new SqlParameter("@SaturdayWork", p.SaturdayWork),
                new SqlParameter("@SundayWork", p.SundayWork),
            };
        }
    }
}
