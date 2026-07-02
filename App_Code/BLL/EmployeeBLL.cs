using System.Collections.Generic;
using HRMS.Model;

namespace HRMS.BLL
{
    public class EmployeeBLL
    {
        public static List<Employee> GetAll() => DAL.EmployeeDAL.GetAll();
        public static Employee GetById(int empId) => DAL.EmployeeDAL.GetById(empId);

        public static int Insert(Employee e) => DAL.EmployeeDAL.Insert(e);

        public static int Update(Employee e) => DAL.EmployeeDAL.Update(e);

        /// <summary>
        /// 物理删除员工（级联清除从表记录由数据库 FK CASCADE 处理）
        /// </summary>
        public static int Delete(int empId) => DAL.EmployeeDAL.Delete(empId);

        /// <summary>
        /// 逻辑删除（设置状态为"离职"）
        /// </summary>
        public static int SetResigned(int empId) => DAL.EmployeeDAL.SetStatus(empId, "离职");

        public static List<Employee> Query(string empNo, string empName, int? deptId,
            int? positionId, string status)
            => DAL.EmployeeDAL.Query(empNo, empName, deptId, positionId, status);
    }
}
