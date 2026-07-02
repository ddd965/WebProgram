using System;

namespace HRMS.Model
{
    /// <summary>
    /// 考勤参数配置（单例配置）
    /// </summary>
    public class AttendanceParam
    {
        public int ParamId { get; set; }
        /// <summary>上班时间（日期部分忽略，仅使用 TimeOfDay）</summary>
        public DateTime WorkStartTime { get; set; }
        /// <summary>下班时间</summary>
        public DateTime WorkEndTime { get; set; }
        /// <summary>迟到宽容分钟数（上班多少分钟内不算迟到）</summary>
        public int LateToleranceMinutes { get; set; }
        /// <summary>早退宽容分钟数</summary>
        public int LeaveEarlyToleranceMinutes { get; set; }
        /// <summary>半天判定小时数（低于此时长算半天）</summary>
        public decimal HalfDayHours { get; set; }
        public bool SaturdayWork { get; set; }
        public bool SundayWork { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
