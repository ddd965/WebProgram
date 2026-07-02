using System.Collections.Generic;
using System.Data;
using HRMS.Model;

namespace HRMS.BLL
{
    public class SalaryBLL
    {
        public static List<Salary> GetAll() => DAL.SalaryDAL.GetAll();
        public static Salary GetById(int salaryId) => DAL.SalaryDAL.GetById(salaryId);

        public static int Insert(Salary s)
        {
            // 自动计算实发工资
            s.NetSalary = s.BaseSalary + s.Performance + s.Bonus + s.OvertimePay
                          - s.Insurance - s.Fund - s.Tax - s.Deduction;
            return DAL.SalaryDAL.Insert(s);
        }

        public static int Update(Salary s)
        {
            s.NetSalary = s.BaseSalary + s.Performance + s.Bonus + s.OvertimePay
                          - s.Insurance - s.Fund - s.Tax - s.Deduction;
            return DAL.SalaryDAL.Update(s);
        }

        public static int Delete(int salaryId) => DAL.SalaryDAL.Delete(salaryId);

        public static List<Salary> QueryByEmpAndMonth(int empId, string monthFrom, string monthTo)
            => DAL.SalaryDAL.QueryByEmpAndMonth(empId, monthFrom, monthTo);

        public static DataTable StatByDeptAndMonth(int? deptId, string salaryMonth)
            => DAL.SalaryDAL.StatByDeptAndMonth(deptId, salaryMonth);
    }
}
