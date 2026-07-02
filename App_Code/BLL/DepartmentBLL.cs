using System.Collections.Generic;
using HRMS.Model;

namespace HRMS.BLL
{
    public class DepartmentBLL
    {
        public static List<Department> GetAll() => DAL.DepartmentDAL.GetAll();
        public static Department GetById(int deptId) => DAL.DepartmentDAL.GetById(deptId);
        public static int Insert(Department d) => DAL.DepartmentDAL.Insert(d);
        public static int Update(Department d) => DAL.DepartmentDAL.Update(d);
        public static int Delete(int deptId) => DAL.DepartmentDAL.Delete(deptId);
        public static List<Department> GetSubDepartments(int parentId) => DAL.DepartmentDAL.GetSubDepartments(parentId);
    }
}
