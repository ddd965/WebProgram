namespace HRMS.Model
{
    /// <summary>
    /// 部门实体
    /// </summary>
    public class Department
    {
        public int DeptId { get; set; }
        public string DeptName { get; set; }
        public int? ParentId { get; set; }
        public int? ManagerId { get; set; }
        public string Descn { get; set; }
        public System.DateTime CreateTime { get; set; }
    }
}
