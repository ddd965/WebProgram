using System;

namespace HRMS.Model
{
    /// <summary>
    /// 考勤参数设置实体（全局单例，SettingId=1）
    /// </summary>
    public class AttendanceSetting
    {
        public int SettingId { get; set; }
        public TimeSpan WorkStartTime { get; set; }
        public TimeSpan WorkEndTime { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyMinutes { get; set; }
        public TimeSpan LunchStart { get; set; }
        public TimeSpan LunchEnd { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
