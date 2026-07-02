using System;

namespace HRMS.Model
{
    /// <summary>
    /// 休假记录实体
    /// </summary>
    public class LeaveRecord
    {
        public int LeaveId { get; set; }
        public int EmpId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal LeaveDays { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public int? ApproverId { get; set; }
        public DateTime ApplyTime { get; set; }
    }
}
