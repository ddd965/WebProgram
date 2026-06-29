using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using HRMS.Model;

namespace HRMS.DAL
{
    public class SalaryDAL
    {
        private const string SELECT_ALL = "SELECT * FROM Salary";
        private const string SELECT_BY_ID = "SELECT * FROM Salary WHERE SalaryId = @SalaryId";
        private const string INSERT = @"INSERT INTO Salary(EmpId, SalaryMonth, BaseSalary, Performance,
                                         Bonus, OvertimePay, Insurance, Fund, Tax, Deduction, NetSalary, PayDate, Remark)
                                         VALUES(@EmpId,@SalaryMonth,@BaseSalary,@Performance,
                                         @Bonus,@OvertimePay,@Insurance,@Fund,@Tax,@Deduction,@NetSalary,@PayDate,@Remark);
                                         SELECT SCOPE_IDENTITY()";
        private const string UPDATE = @"UPDATE Salary SET EmpId=@EmpId, SalaryMonth=@SalaryMonth,
                                         BaseSalary=@BaseSalary, Performance=@Performance,
                                         Bonus=@Bonus, OvertimePay=@OvertimePay,
                                         Insurance=@Insurance, Fund=@Fund, Tax=@Tax,
                                         Deduction=@Deduction, NetSalary=@NetSalary,
                                         PayDate=@PayDate, Remark=@Remark
                                         WHERE SalaryId=@SalaryId";
        private const string DELETE = "DELETE FROM Salary WHERE SalaryId=@SalaryId";

        public static List<Salary> GetAll()
        {
            var list = new List<Salary>();
            using (var dr = DBHelper.ExecuteReader(SELECT_ALL))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        public static Salary GetById(int salaryId)
        {
            using (var dr = DBHelper.ExecuteReader(SELECT_BY_ID,
                new SqlParameter[] { new SqlParameter("@SalaryId", salaryId) }))
            {
                if (dr.Read()) return Map(dr);
            }
            return null;
        }

        public static int Insert(Salary s)
        {
            return Convert.ToInt32(DBHelper.ExecuteScalar(INSERT, GetParams(s)));
        }

        public static int Update(Salary s)
        {
            var parameters = new List<SqlParameter>(GetParams(s));
            parameters.Add(new SqlParameter("@SalaryId", s.SalaryId));
            return DBHelper.ExecuteNonQuery(UPDATE, parameters.ToArray());
        }

        public static int Delete(int salaryId)
        {
            return DBHelper.ExecuteNonQuery(DELETE,
                new SqlParameter[] { new SqlParameter("@SalaryId", salaryId) });
        }

        /// <summary>
        /// 查询特定员工薪酬（按工号+月份范围）
        /// </summary>
        public static List<Salary> QueryByEmpAndMonth(int empId, string monthFrom, string monthTo)
        {
            var sql = new StringBuilder("SELECT * FROM Salary WHERE EmpId = @EmpId");
            if (!string.IsNullOrEmpty(monthFrom))
            {
                sql.Append(" AND SalaryMonth >= @MonthFrom");
            }
            if (!string.IsNullOrEmpty(monthTo))
            {
                sql.Append(" AND SalaryMonth <= @MonthTo");
            }
            sql.Append(" ORDER BY SalaryMonth DESC");

            var parameters = new List<SqlParameter> { new SqlParameter("@EmpId", empId) };
            if (!string.IsNullOrEmpty(monthFrom))
                parameters.Add(new SqlParameter("@MonthFrom", monthFrom));
            if (!string.IsNullOrEmpty(monthTo))
                parameters.Add(new SqlParameter("@MonthTo", monthTo));

            var list = new List<Salary>();
            using (var dr = DBHelper.ExecuteReader(sql.ToString(), parameters.ToArray()))
            {
                while (dr.Read()) list.Add(Map(dr));
            }
            return list;
        }

        /// <summary>
        /// 按部门/月份汇总薪资
        /// </summary>
        public static System.Data.DataTable StatByDeptAndMonth(int? deptId, string salaryMonth)
        {
            var sql = new StringBuilder();
            sql.Append(@"SELECT d.DeptName, COUNT(DISTINCT s.EmpId) AS EmpCount,
                          SUM(s.BaseSalary) AS TotalBase, SUM(s.NetSalary) AS TotalNet,
                          AVG(s.NetSalary) AS AvgNet
                         FROM Salary s
                         INNER JOIN Employee e ON s.EmpId = e.EmpId
                         LEFT JOIN Department d ON e.DeptId = d.DeptId
                         WHERE s.SalaryMonth = @SalaryMonth");
            var parameters = new List<SqlParameter> { new SqlParameter("@SalaryMonth", salaryMonth) };
            if (deptId.HasValue)
            {
                sql.Append(" AND e.DeptId = @DeptId");
                parameters.Add(new SqlParameter("@DeptId", deptId.Value));
            }
            sql.Append(" GROUP BY d.DeptName");
            return DBHelper.ExecuteDataTable(sql.ToString(), parameters.ToArray());
        }

        private static Salary Map(SqlDataReader dr)
        {
            return new Salary
            {
                SalaryId = (int)dr["SalaryId"],
                EmpId = (int)dr["EmpId"],
                SalaryMonth = dr["SalaryMonth"].ToString(),
                BaseSalary = (decimal)dr["BaseSalary"],
                Performance = (decimal)dr["Performance"],
                Bonus = (decimal)dr["Bonus"],
                OvertimePay = (decimal)dr["OvertimePay"],
                Insurance = (decimal)dr["Insurance"],
                Fund = (decimal)dr["Fund"],
                Tax = (decimal)dr["Tax"],
                Deduction = (decimal)dr["Deduction"],
                NetSalary = (decimal)dr["NetSalary"],
                PayDate = dr["PayDate"] as DateTime?,
                Remark = dr["Remark"] as string,
                CreateTime = (DateTime)dr["CreateTime"]
            };
        }

        private static SqlParameter[] GetParams(Salary s)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@EmpId", s.EmpId),
                new SqlParameter("@SalaryMonth", s.SalaryMonth),
                new SqlParameter("@BaseSalary", s.BaseSalary),
                new SqlParameter("@Performance", s.Performance),
                new SqlParameter("@Bonus", s.Bonus),
                new SqlParameter("@OvertimePay", s.OvertimePay),
                new SqlParameter("@Insurance", s.Insurance),
                new SqlParameter("@Fund", s.Fund),
                new SqlParameter("@Tax", s.Tax),
                new SqlParameter("@Deduction", s.Deduction),
                new SqlParameter("@NetSalary", s.NetSalary),
                new SqlParameter("@PayDate", (object)s.PayDate ?? DBNull.Value),
                new SqlParameter("@Remark", (object)s.Remark ?? DBNull.Value)
            };
        }
    }
}
