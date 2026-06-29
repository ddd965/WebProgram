using System;
using System.Data.SqlClient;
using HRMS.Model;

namespace HRMS.DAL
{
    public class AttendanceSettingDAL
    {
        private const string SELECT = "SELECT * FROM AttendanceSetting WHERE SettingId = 1";
        private const string UPDATE = @"UPDATE AttendanceSetting SET WorkStartTime=@WorkStartTime,
                                         WorkEndTime=@WorkEndTime, LateMinutes=@LateMinutes,
                                         EarlyMinutes=@EarlyMinutes, LunchStart=@LunchStart,
                                         LunchEnd=@LunchEnd, UpdateTime=GETDATE()
                                         WHERE SettingId=1";

        /// <summary>
        /// 获取全局唯一考勤参数（SettingId=1）
        /// </summary>
        public static AttendanceSetting GetSetting()
        {
            using (var dr = DBHelper.ExecuteReader(SELECT))
            {
                if (dr.Read())
                {
                    return new AttendanceSetting
                    {
                        SettingId = (int)dr["SettingId"],
                        WorkStartTime = (TimeSpan)dr["WorkStartTime"],
                        WorkEndTime = (TimeSpan)dr["WorkEndTime"],
                        LateMinutes = (int)dr["LateMinutes"],
                        EarlyMinutes = (int)dr["EarlyMinutes"],
                        LunchStart = (TimeSpan)dr["LunchStart"],
                        LunchEnd = (TimeSpan)dr["LunchEnd"],
                        UpdateTime = (DateTime)dr["UpdateTime"]
                    };
                }
            }
            return null;
        }

        public static int Update(AttendanceSetting setting)
        {
            return DBHelper.ExecuteNonQuery(UPDATE,
                new SqlParameter[]
                {
                    new SqlParameter("@WorkStartTime", setting.WorkStartTime),
                    new SqlParameter("@WorkEndTime", setting.WorkEndTime),
                    new SqlParameter("@LateMinutes", setting.LateMinutes),
                    new SqlParameter("@EarlyMinutes", setting.EarlyMinutes),
                    new SqlParameter("@LunchStart", setting.LunchStart),
                    new SqlParameter("@LunchEnd", setting.LunchEnd)
                });
        }
    }
}
