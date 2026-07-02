using System;
using System.Collections.Generic;
using HRMS.Model;

namespace HRMS.BLL
{
    public class OvertimeBLL
    {
        public static List<Overtime> GetAll() => DAL.OvertimeDAL.GetAll();
        public static Overtime GetById(int otId) => DAL.OvertimeDAL.GetById(otId);

        public static int Insert(Overtime ot)
        {
            // 自动计算加班小时数
            ot.OtHours = Math.Round((decimal)(ot.EndTime - ot.StartTime).TotalHours, 1);
            return DAL.OvertimeDAL.Insert(ot);
        }

        public static int Update(Overtime ot)
        {
            ot.OtHours = Math.Round((decimal)(ot.EndTime - ot.StartTime).TotalHours, 1);
            return DAL.OvertimeDAL.Update(ot);
        }

        public static int Delete(int otId) => DAL.OvertimeDAL.Delete(otId);

        public static List<Overtime> Query(int? empId, DateTime? startDate, DateTime? endDate,
            string otType, string status)
            => DAL.OvertimeDAL.Query(empId, startDate, endDate, otType, status);

        /// <summary>
        /// 按部门+月份汇总加班（含预估加班费，取员工最近一次基本工资）
        /// 修复：使用 "已批准" 状态
        /// </summary>
        public static System.Data.DataTable StatByDeptAndMonth(int? deptId, string month)
        {
            int y = int.Parse(month.Substring(0, 4));
            int m = int.Parse(month.Substring(5, 2));
            var from = new DateTime(y, m, 1);
            var to = from.AddMonths(1).AddDays(-1);
            return DoStatByRange(deptId, from, to, "月");
        }

        /// <summary>
        /// 按季度汇总加班：quarter=1~4
        /// </summary>
        public static System.Data.DataTable StatByDeptAndQuarter(int? deptId, int year, int quarter)
        {
            int startMonth = (quarter - 1) * 3 + 1;
            var from = new DateTime(year, startMonth, 1);
            var to = from.AddMonths(3).AddDays(-1);
            return DoStatByRange(deptId, from, to, "Q" + quarter);
        }

        private static System.Data.DataTable DoStatByRange(int? deptId, DateTime from, DateTime to, string periodLabel)
        {
            var allOT = Query(null, from, to.AddDays(1), null, "已批准"); // 修复：使用"已批准"

            // 员工基础信息缓存（最新基本工资）
            var empBase = new Dictionary<int, decimal>();
            var empDept = new Dictionary<int, string>();
            var emps = EmployeeBLL.GetAll();
            foreach (var e in emps)
            {
                if (deptId.HasValue && e.DeptId != deptId.Value) continue;
                var salaryList = SalaryBLL.QueryByEmpAndMonth(e.EmpId, null, null);
                decimal bs = 0;
                if (salaryList.Count > 0) bs = salaryList[salaryList.Count - 1].BaseSalary;
                empBase[e.EmpId] = bs;
                string dname = "未分配";
                if (e.DeptId.HasValue)
                {
                    var d = DepartmentBLL.GetById(e.DeptId.Value);
                    if (d != null) dname = d.DeptName;
                }
                empDept[e.EmpId] = dname;
            }

            var dt = new System.Data.DataTable();
            dt.Columns.Add("Period", typeof(string));
            dt.Columns.Add("DeptName", typeof(string));
            dt.Columns.Add("EmpCount", typeof(int));
            dt.Columns.Add("TotalHours", typeof(decimal));
            dt.Columns.Add("TotalPay", typeof(decimal));

            var groups = new Dictionary<string, OvertimeStatAcc>();
            var empSeenPerDept = new Dictionary<string, HashSet<int>>();

            foreach (var ot in allOT)
            {
                if (!empDept.ContainsKey(ot.EmpId)) continue;
                string dn = empDept[ot.EmpId];
                if (!groups.ContainsKey(dn))
                {
                    groups[dn] = new OvertimeStatAcc();
                    empSeenPerDept[dn] = new HashSet<int>();
                }
                groups[dn].TotalHours += ot.OtHours;
                decimal bs = empBase.ContainsKey(ot.EmpId) ? empBase[ot.EmpId] : 0;
                groups[dn].TotalPay += CalculateOvertimePay(bs, ot.OtHours, ot.OtType);
                empSeenPerDept[dn].Add(ot.EmpId);
            }
            foreach (var kv in groups)
            {
                dt.Rows.Add(periodLabel, kv.Key, empSeenPerDept[kv.Key].Count,
                    Math.Round(kv.Value.TotalHours, 1), Math.Round(kv.Value.TotalPay, 2));
            }
            return dt;
        }

        private class OvertimeStatAcc { public decimal TotalHours; public decimal TotalPay; }

        /// <summary>
        /// 计算加班费：工作日 1.5×、周末 2×、节假日 3×（基于每小时工资本薪/21.75/8）
        /// </summary>
        public static decimal CalculateOvertimePay(decimal baseSalary, decimal otHours, string otType)
        {
            if (baseSalary <= 0 || otHours <= 0) return 0m;
            decimal hourlyRate = baseSalary / 21.75m / 8m;
            decimal multiplier;
            switch (otType)
            {
                case "工作日": multiplier = 1.5m; break;
                case "周末": multiplier = 2.0m; break;
                case "节假日": multiplier = 3.0m; break;
                default: multiplier = 1.0m; break;
            }
            return Math.Round(hourlyRate * otHours * multiplier, 2);
        }

        /// <summary>
        /// 回写加班费到工资模块：计算指定月份每位员工的加班费，并写入 Salary 表
        /// 如果该员工当月已有工资记录，更新 OvertimePay；否则不做处理（需要先在工资编辑里创建）
        /// 返回实际更新的记录数
        /// </summary>
        public static int WriteBackOvertimePay(string salaryMonth, int? deptId)
        {
            int y = int.Parse(salaryMonth.Substring(0, 4));
            int m = int.Parse(salaryMonth.Substring(5, 2));
            var from = new DateTime(y, m, 1);
            var to = from.AddMonths(1).AddDays(-1);

            var allOT = Query(null, from, to.AddDays(1), null, "已批准");
            // 按员工汇总 OtHours 与 OtPay
            var empOtHours = new Dictionary<int, decimal>();
            var empOtPay = new Dictionary<int, decimal>();
            var emps = EmployeeBLL.GetAll();
            var empSalaryBase = new Dictionary<int, decimal>();

            foreach (var ot in allOT)
            {
                var emp = emps.Find(e => e.EmpId == ot.EmpId);
                if (emp == null) continue;
                if (deptId.HasValue && emp.DeptId != deptId.Value) continue;
                if (!empSalaryBase.ContainsKey(emp.EmpId))
                {
                    var slist = SalaryBLL.QueryByEmpAndMonth(emp.EmpId, null, null);
                    decimal bs = 0;
                    if (slist.Count > 0) bs = slist[slist.Count - 1].BaseSalary;
                    empSalaryBase[emp.EmpId] = bs;
                }
                if (!empOtHours.ContainsKey(emp.EmpId))
                {
                    empOtHours[emp.EmpId] = 0; empOtPay[emp.EmpId] = 0;
                }
                empOtHours[emp.EmpId] += ot.OtHours;
                empOtPay[emp.EmpId] += CalculateOvertimePay(empSalaryBase[emp.EmpId], ot.OtHours, ot.OtType);
            }

            int updated = 0;
            foreach (var kv in empOtPay)
            {
                int empId = kv.Key;
                var slist = SalaryBLL.QueryByEmpAndMonth(empId, salaryMonth, salaryMonth);
                if (slist.Count == 0) continue; // 没有对应工资单，跳过
                var salary = slist[0];
                salary.OvertimePay = Math.Round(kv.Value, 2);
                SalaryBLL.Update(salary);
                updated++;
            }
            WriteLog.Write("System", "修改",
                $"加班汇总回写工资月份 {salaryMonth}，共更新 {updated} 条工资记录的 OvertimePay 字段。");
            return updated;
        }
    }
}
