namespace HRMS.Model
{
    /// <summary>
    /// 职位实体
    /// </summary>
    public class Position
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; }
        public int? Level { get; set; }
        public string Descn { get; set; }
    }
}
