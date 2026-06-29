using System;
using System.Collections.Generic;
using System.Transactions;
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
        /// 计算加班费：工作日 1.5×、周末 2×、节假日 3×（基于每小时工资本薪/21.75/8）
        /// </summary>
        public static decimal CalculateOvertimePay(decimal baseSalary, decimal otHours, string otType)
        {
            decimal hourlyRate = baseSalary / 21.75m / 8m;
            decimal multiplier = otType switch
            {
                "工作日" => 1.5m,
                "周末" => 2.0m,
                "节假日" => 3.0m,
                _ => 1.0m
            };
            return Math.Round(hourlyRate * otHours * multiplier, 2);
        }
    }
}
