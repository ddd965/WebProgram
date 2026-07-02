using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using HRMS.Model;

namespace HRMS.DAL
{
    public class LeaveRecordDAL
    {
        private const string SELECT_ALL = "SELECT * FROM LeaveRecord";
        private const string SELECT_BY_ID = "SELECT * FROM LeaveRecord WHERE LeaveId = @LeaveId";
        private const string INSERT = @"INSERT INTO LeaveRecord(EmpId, LeaveTypeId, StartDate, EndDate,
                                         LeaveDays, Reason, Status, ApproverId)
                                         VALUES(@EmpId,@LeaveTypeId,@StartDate,@EndDate,
                                         @LeaveDays,@Reason,@Status,@ApproverId);
                                         SELECT SCOPE_IDENTITY()";
        private const string UPDATE = @"UPDATE LeaveRecord SET EmpId=@EmpId, LeaveTypeId=@LeaveTypeId,
                                         StartDate=@StartDate, EndDate=@EndDate, LeaveDays=@LeaveDays,
                                         Reason=@Reason, Status=@Status, ApproverId=@ApproverId
                                         WHERE LeaveId=@LeaveId";
        private const string DELETE = "DELETE FROM LeaveRecord WHERE LeaveId=@LeaveId";

        public static List<LeaveRecord> GetAll()
        {
            var list = new List<LeaveRecord>();
            using (var dr = DBHelper.ExecuteReader(SELECT_ALL))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        public static LeaveRecord GetById(int leaveId)
        {
            using (var dr = DBHelper.ExecuteReader(SELECT_BY_ID,
                new SqlParameter[] { new SqlParameter("@LeaveId", leaveId) }))
            {
                if (dr.Read()) return Map(dr);
            }
            return null;
        }

        public static int Insert(LeaveRecord lr)
        {
            return Convert.ToInt32(DBHelper.ExecuteScalar(INSERT, GetParams(lr)));
        }

        public static int Update(LeaveRecord lr)
        {
            var parameters = new List<SqlParameter>(GetParams(lr));
            parameters.Add(new SqlParameter("@LeaveId", lr.LeaveId));
            return DBHelper.ExecuteNonQuery(UPDATE, parameters.ToArray());
        }

        public static int Delete(int leaveId)
        {
            return DBHelper.ExecuteNonQuery(DELETE,
                new SqlParameter[] { new SqlParameter("@LeaveId", leaveId) });
        }

        public static int Approve(int leaveId, string status, int approverId)
        {
            return DBHelper.ExecuteNonQuery(
                "UPDATE LeaveRecord SET Status=@Status, ApproverId=@ApproverId WHERE LeaveId=@LeaveId",
                new SqlParameter[] {
                    new SqlParameter("@LeaveId", leaveId),
                    new SqlParameter("@Status", status),
                    new SqlParameter("@ApproverId", approverId)
                });
        }

        /// <summary>
        /// 多条件休假查询
        /// </summary>
        public static List<LeaveRecord> Query(int? empId, int? leaveTypeId, DateTime? startDate,
            DateTime? endDate, string status)
        {
            var sql = new StringBuilder("SELECT * FROM LeaveRecord WHERE 1=1");
            var parameters = new List<SqlParameter>();

            if (empId.HasValue)
            {
                sql.Append(" AND EmpId = @EmpId");
                parameters.Add(new SqlParameter("@EmpId", empId.Value));
            }
            if (leaveTypeId.HasValue)
            {
                sql.Append(" AND LeaveTypeId = @LeaveTypeId");
                parameters.Add(new SqlParameter("@LeaveTypeId", leaveTypeId.Value));
            }
            if (startDate.HasValue)
            {
                sql.Append(" AND StartDate >= @StartDate");
                parameters.Add(new SqlParameter("@StartDate", startDate.Value));
            }
            if (endDate.HasValue)
            {
                sql.Append(" AND EndDate <= @EndDate");
                parameters.Add(new SqlParameter("@EndDate", endDate.Value));
            }
            if (!string.IsNullOrEmpty(status))
            {
                sql.Append(" AND Status = @Status");
                parameters.Add(new SqlParameter("@Status", status));
            }
            sql.Append(" ORDER BY ApplyTime DESC");

            var list = new List<LeaveRecord>();
            using (var dr = DBHelper.ExecuteReader(sql.ToString(), parameters.ToArray()))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        /// <summary>
        /// 按休假类型+月份汇总（可限定部门）
        /// 返回DataTable列：LeaveTypeName, LeaveCount, TotalDays
        /// </summary>
        public static System.Data.DataTable StatByTypeAndMonth(int? deptId, string month)
        {
            // month: YYYY-MM
            string fromStr = month + "-01";
            string toStr = fromStr; // only for parameterizing range; we use LIKE below
            var sql = new StringBuilder();
            sql.Append(@"SELECT lt.TypeName AS LeaveTypeName,
                            COUNT(lr.LeaveId) AS LeaveCount,
                            ISNULL(SUM(lr.LeaveDays),0) AS TotalDays
                         FROM LeaveRecord lr
                         INNER JOIN LeaveType lt ON lr.LeaveTypeId = lt.LeaveTypeId
                         INNER JOIN Employee e ON lr.EmpId = e.EmpId
                         WHERE CONVERT(VARCHAR(7), lr.StartDate, 120) = @Month");
            var pars = new List<SqlParameter> { new SqlParameter("@Month", month) };
            if (deptId.HasValue)
            {
                sql.Append(" AND e.DeptId = @DeptId");
                pars.Add(new SqlParameter("@DeptId", deptId.Value));
            }
            sql.Append(" GROUP BY lt.TypeName ORDER BY TotalDays DESC");
            return DBHelper.ExecuteDataTable(sql.ToString(), pars.ToArray());
        }

        private static LeaveRecord Map(SqlDataReader dr)
        {
            return new LeaveRecord
            {
                LeaveId = (int)dr["LeaveId"],
                EmpId = (int)dr["EmpId"],
                LeaveTypeId = (int)dr["LeaveTypeId"],
                StartDate = (DateTime)dr["StartDate"],
                EndDate = (DateTime)dr["EndDate"],
                LeaveDays = (decimal)dr["LeaveDays"],
                Reason = dr["Reason"].ToString(),
                Status = dr["Status"].ToString(),
                ApproverId = dr["ApproverId"] as int?,
                ApplyTime = (DateTime)dr["ApplyTime"]
            };
        }

        private static SqlParameter[] GetParams(LeaveRecord lr)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@EmpId", lr.EmpId),
                new SqlParameter("@LeaveTypeId", lr.LeaveTypeId),
                new SqlParameter("@StartDate", lr.StartDate),
                new SqlParameter("@EndDate", lr.EndDate),
                new SqlParameter("@LeaveDays", lr.LeaveDays),
                new SqlParameter("@Reason", lr.Reason),
                new SqlParameter("@Status", lr.Status),
                new SqlParameter("@ApproverId", (object)lr.ApproverId ?? DBNull.Value)
            };
        }
    }
}
