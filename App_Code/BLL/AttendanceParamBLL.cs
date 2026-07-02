using System;
using HRMS.Model;

namespace HRMS.BLL
{
    public class AttendanceParamBLL
    {
        /// <summary>
        /// 读取默认考勤参数（若无记录则返回一套合理的默认值：朝九晚六、宽容10分钟等）
        /// </summary>
        public static AttendanceParam GetDefault()
        {
            var p = DAL.AttendanceParamDAL.GetDefault();
            if (p != null) return p;
            return new AttendanceParam
            {
                ParamId = 0,
                WorkStartTime = DateTime.Today.AddHours(9),
                WorkEndTime = DateTime.Today.AddHours(18),
                LateToleranceMinutes = 10,
                LeaveEarlyToleranceMinutes = 10,
                HalfDayHours = 4m,
                SaturdayWork = false,
                SundayWork = false
            };
        }

        /// <summary>同 GetDefault（别名，兼容旧调用）</summary>
        public static AttendanceParam LoadDefault() => GetDefault();

        /// <summary>
        /// 保存配置：首次写入为新增，后续为更新
        /// </summary>
        public static int SaveSettings(AttendanceParam p)
        {
            if (p.ParamId > 0)
                return DAL.AttendanceParamDAL.Update(p);
            var exist = DAL.AttendanceParamDAL.GetDefault();
            if (exist != null)
            {
                p.ParamId = exist.ParamId;
                return DAL.AttendanceParamDAL.Update(p);
            }
            return DAL.AttendanceParamDAL.Insert(p);
        }
    }
}
