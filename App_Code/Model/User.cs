using System;

namespace HRMS.Model
{
    /// <summary>
    /// 用户实体
    /// </summary>
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? EmpId { get; set; }
        public string RoleName { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
