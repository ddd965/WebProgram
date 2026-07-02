using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using HRMS.Model;

namespace HRMS.DAL
{
    public class AttendanceDAL
    {
        private const string SELECT_ALL = "SELECT * FROM Attendance";
        private const string SELECT_BY_ID = "SELECT * FROM Attendance WHERE AttId = @AttId";
        private const string SELECT_BY_EMP_DATE = "SELECT * FROM Attendance WHERE EmpId = @EmpId AND AttDate = @AttDate";
        private const string INSERT = @"INSERT INTO Attendance(EmpId, AttDate, CheckInTime, CheckOutTime, Status, Remark)
                                         VALUES(@EmpId,@AttDate,@CheckInTime,@CheckOutTime,@Status,@Remark);
                                         SELECT SCOPE_IDENTITY()";
        private const string UPDATE = @"UPDATE Attendance SET CheckInTime=@CheckInTime,
                                         CheckOutTime=@CheckOutTime, Status=@Status, Remark=@Remark
                                         WHERE AttId=@AttId";
        private const string DELETE = "DELETE FROM Attendance WHERE AttId=@AttId";

        public static List<Attendance> GetAll()
        {
            var list = new List<Attendance>();
            using (var dr = DBHelper.ExecuteReader(SELECT_ALL))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        public static Attendance GetById(int attId)
        {
            using (var dr = DBHelper.ExecuteReader(SELECT_BY_ID,
                new SqlParameter[] { new SqlParameter("@AttId", attId) }))
            {
                if (dr.Read()) return Map(dr);
            }
            return null;
        }

        /// <summary>
        /// 按员工+日期获取当天考勤记录
        /// </summary>
        public static Attendance GetByEmpAndDate(int empId, DateTime attDate)
        {
            using (var dr = DBHelper.ExecuteReader(SELECT_BY_EMP_DATE,
                new SqlParameter[] {
                    new SqlParameter("@EmpId", empId),
                    new SqlParameter("@AttDate", attDate.Date)
                }))
            {
                if (dr.Read()) return Map(dr);
            }
            return null;
        }

        public static int Insert(Attendance a)
        {
            return Convert.ToInt32(DBHelper.ExecuteScalar(INSERT, GetParams(a)));
        }

        public static int Update(Attendance a)
        {
            var parameters = new List<SqlParameter>(GetParams(a));
            parameters.Add(new SqlParameter("@AttId", a.AttId));
            return DBHelper.ExecuteNonQuery(UPDATE, parameters.ToArray());
        }

        public static int Delete(int attId)
        {
            return DBHelper.ExecuteNonQuery(DELETE,
                new SqlParameter[] { new SqlParameter("@AttId", attId) });
        }

        /// <summary>
        /// 按月查询指定员工考勤明细
        /// </summary>
        public static List<Attendance> QueryByMonth(int empId, int year, int month)
        {
            var sql = @"SELECT * FROM Attendance
                         WHERE EmpId = @EmpId AND YEAR(AttDate)=@Year AND MONTH(AttDate)=@Month
                         ORDER BY AttDate";
            var list = new List<Attendance>();
            using (var dr = DBHelper.ExecuteReader(sql,
                new SqlParameter[] {
                    new SqlParameter("@EmpId", empId),
                    new SqlParameter("@Year", year),
                    new SqlParameter("@Month", month)
                }))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        /// <summary>
        /// 按部门+月份统计考勤汇总
        /// </summary>
        public static System.Data.DataTable StatByDeptAndMonth(int? deptId, int year, int month)
        {
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1);

            var sql = new StringBuilder();
            sql.Append(@"SELECT e.EmpId, e.EmpNo, e.EmpName, d.DeptName,
                          SUM(CASE WHEN a.Status=N'正常' THEN 1 ELSE 0 END) AS NormalDays,
                          SUM(CASE WHEN a.Status=N'迟到' THEN 1 ELSE 0 END) AS LateCount,
                          SUM(CASE WHEN a.Status=N'早退' THEN 1 ELSE 0 END) AS EarlyCount,
                          SUM(CASE WHEN a.Status=N'缺勤' THEN 1 ELSE 0 END) AS AbsentCount,
                          SUM(CASE WHEN a.Status=N'请假' THEN 1 ELSE 0 END) AS LeaveCount
                         FROM Employee e
                         LEFT JOIN Department d ON e.DeptId = d.DeptId
                         LEFT JOIN Attendance a ON e.EmpId = a.EmpId
                           AND a.AttDate >= @StartDate AND a.AttDate < @EndDate
                         WHERE 1=1");
            var parameters = new List<SqlParameter>();
            if (deptId.HasValue)
            {
                sql.Append(" AND e.DeptId = @DeptId");
                parameters.Add(new SqlParameter("@DeptId", deptId.Value));
            }
            sql.Append(" GROUP BY e.EmpId, e.EmpNo, e.EmpName, d.DeptName");

            parameters.Add(new SqlParameter("@StartDate", startDate));
            parameters.Add(new SqlParameter("@EndDate", endDate));
            return DBHelper.ExecuteDataTable(sql.ToString(), parameters.ToArray());
        }

        private static Attendance Map(SqlDataReader dr)
        {
            return new Attendance
            {
                AttId = (int)dr["AttId"],
                EmpId = (int)dr["EmpId"],
                AttDate = (DateTime)dr["AttDate"],
                CheckInTime = dr["CheckInTime"] as DateTime?,
                CheckOutTime = dr["CheckOutTime"] as DateTime?,
                Status = dr["Status"].ToString(),
                Remark = dr["Remark"] as string
            };
        }

        private static SqlParameter[] GetParams(Attendance a)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@EmpId", a.EmpId),
                new SqlParameter("@AttDate", a.AttDate),
                new SqlParameter("@CheckInTime", (object)a.CheckInTime ?? DBNull.Value),
                new SqlParameter("@CheckOutTime", (object)a.CheckOutTime ?? DBNull.Value),
                new SqlParameter("@Status", a.Status),
                new SqlParameter("@Remark", (object)a.Remark ?? DBNull.Value)
            };
        }
    }
}
