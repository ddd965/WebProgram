namespace HRMS.Model
{
    /// <summary>
    /// 休假类型实体
    /// </summary>
    public class LeaveType
    {
        public int LeaveTypeId { get; set; }
        public string TypeName { get; set; }
        public decimal DefaultDays { get; set; }
        public bool NeedApproval { get; set; }
        public string Descn { get; set; }
    }
}
