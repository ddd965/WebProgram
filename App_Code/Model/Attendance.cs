using System;

namespace HRMS.Model
{
    /// <summary>
    /// 考勤实体
    /// </summary>
    public class Attendance
    {
        public int AttId { get; set; }
        public int EmpId { get; set; }
        public DateTime AttDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
    }
}
