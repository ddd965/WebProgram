using System;

namespace HRMS.Model
{
    /// <summary>
    /// 加班实体
    /// </summary>
    public class Overtime
    {
        public int OtId { get; set; }
        public int EmpId { get; set; }
        public DateTime OtDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal OtHours { get; set; }
        public string OtType { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
