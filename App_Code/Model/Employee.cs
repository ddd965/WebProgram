using System;

namespace HRMS.Model
{
    /// <summary>
    /// 员工实体
    /// </summary>
    public class Employee
    {
        public int EmpId { get; set; }
        public string EmpNo { get; set; }
        public string EmpName { get; set; }
        public string Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string IdCard { get; set; }
        public DateTime HireDate { get; set; }
        public int? DeptId { get; set; }
        public int? PositionId { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhotoPath { get; set; }
        public string Status { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
