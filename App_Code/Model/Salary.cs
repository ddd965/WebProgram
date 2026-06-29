using System;

namespace HRMS.Model
{
    /// <summary>
    /// 工资实体
    /// </summary>
    public class Salary
    {
        public int SalaryId { get; set; }
        public int EmpId { get; set; }
        public string SalaryMonth { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Performance { get; set; }
        public decimal Bonus { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal Insurance { get; set; }
        public decimal Fund { get; set; }
        public decimal Tax { get; set; }
        public decimal Deduction { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime? PayDate { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
