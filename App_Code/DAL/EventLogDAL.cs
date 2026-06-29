using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using HRMS.Model;

namespace HRMS.DAL
{
    public class EventLogDAL
    {
        private const string SELECT_ALL = "SELECT * FROM EventLog ORDER BY EventTime DESC";
        private const string SELECT_BY_ID = "SELECT * FROM EventLog WHERE LogId = @LogId";
        private const string INSERT = @"INSERT INTO EventLog(UserName, EventName, Description, IPAddress)
                                         VALUES(@UserName,@EventName,@Description,@IPAddress);
                                         SELECT SCOPE_IDENTITY()";
        private const string DELETE = "DELETE FROM EventLog WHERE LogId = @LogId";

        public static List<EventLog> GetAll()
        {
            var list = new List<EventLog>();
            using (var dr = DBHelper.ExecuteReader(SELECT_ALL))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        public static EventLog GetById(long logId)
        {
            using (var dr = DBHelper.ExecuteReader(SELECT_BY_ID,
                new SqlParameter[] { new SqlParameter("@LogId", logId) }))
            {
                if (dr.Read()) return Map(dr);
            }
            return null;
        }

        public static long Insert(EventLog log)
        {
            return Convert.ToInt64(DBHelper.ExecuteScalar(INSERT, GetParams(log)));
        }

        public static int Delete(long logId)
        {
            return DBHelper.ExecuteNonQuery(DELETE,
                new SqlParameter[] { new SqlParameter("@LogId", logId) });
        }

        /// <summary>
        /// 批量删除过期日志
        /// </summary>
        public static int DeleteBefore(DateTime beforeDate)
        {
            return DBHelper.ExecuteNonQuery("DELETE FROM EventLog WHERE EventTime < @BeforeDate",
                new SqlParameter[] { new SqlParameter("@BeforeDate", beforeDate) });
        }

        /// <summary>
        /// 三条件组合查询
        /// </summary>
        public static List<EventLog> Query(string userName, DateTime? startDate,
            DateTime? endDate, string eventName)
        {
            var sql = new StringBuilder("SELECT * FROM EventLog WHERE 1=1");
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(userName))
            {
                sql.Append(" AND UserName LIKE @UserName");
                parameters.Add(new SqlParameter("@UserName", "%" + userName + "%"));
            }
            if (startDate.HasValue)
            {
                sql.Append(" AND EventTime >= @StartDate");
                parameters.Add(new SqlParameter("@StartDate", startDate.Value));
            }
            if (endDate.HasValue)
            {
                sql.Append(" AND EventTime <= @EndDate");
                parameters.Add(new SqlParameter("@EndDate", endDate.Value));
            }
            if (!string.IsNullOrEmpty(eventName))
            {
                sql.Append(" AND EventName = @EventName");
                parameters.Add(new SqlParameter("@EventName", eventName));
            }
            sql.Append(" ORDER BY EventTime DESC");

            var list = new List<EventLog>();
            using (var dr = DBHelper.ExecuteReader(sql.ToString(), parameters.ToArray()))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        private static EventLog Map(SqlDataReader dr)
        {
            return new EventLog
            {
                LogId = (long)dr["LogId"],
                UserName = dr["UserName"].ToString(),
                EventName = dr["EventName"].ToString(),
                Description = dr["Description"] as string,
                IPAddress = dr["IPAddress"] as string,
                EventTime = (DateTime)dr["EventTime"]
            };
        }

        private static SqlParameter[] GetParams(EventLog log)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@UserName", log.UserName),
                new SqlParameter("@EventName", log.EventName),
                new SqlParameter("@Description", (object)log.Description ?? DBNull.Value),
                new SqlParameter("@IPAddress", (object)log.IPAddress ?? DBNull.Value)
            };
        }
    }
}
