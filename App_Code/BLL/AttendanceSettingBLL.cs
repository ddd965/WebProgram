using System;
using HRMS.Model;

namespace HRMS.BLL
{
    public class AttendanceSettingBLL
    {
        /// <summary>
        /// 获取全局唯一考勤参数
        /// </summary>
        public static AttendanceSetting GetSetting() => DAL.AttendanceSettingDAL.GetSetting();

        /// <summary>
        /// 更新考勤参数
        /// </summary>
        public static int Update(AttendanceSetting setting) => DAL.AttendanceSettingDAL.Update(setting);
    }
}
