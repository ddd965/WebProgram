using System;
using System.Collections.Generic;
using System.Linq;
using HRMS.Model;

namespace HRMS.BLL
{
    public class AttendanceBLL
    {
        /// <summary>
        /// 员工签到 — 如果当天已有记录则更新签到时间
        /// </summary>
        public static bool CheckIn(int empId)
        {
            var today = DateTime.Today;
            var existing = DAL.AttendanceDAL.GetByEmpAndDate(empId, today);

            // 检查当天是否有已批准的休假
            var leaveRecords = DAL.LeaveRecordDAL.Query(empId, null, today, today, "已批准");
            bool isOnLeave = leaveRecords != null && leaveRecords.Count > 0;

            // 获取考勤参数
            var setting = DAL.AttendanceSettingDAL.GetSetting();
            DateTime now = DateTime.Now;

            string status;
            if (isOnLeave)
            {
                status = "请假";
            }
            else if (setting != null && now.TimeOfDay > setting.WorkStartTime.Add(new TimeSpan(0, setting.LateMinutes, 0)))
            {
                status = "迟到";
            }
            else
            {
                status = "正常";
            }

            if (existing != null)
            {
                existing.CheckInTime = now;
                existing.Status = existing.CheckInTime.HasValue && existing.CheckOutTime.HasValue
                    ? existing.Status : status;
                DAL.AttendanceDAL.Update(existing);
            }
            else
            {
                var att = new Attendance
                {
                    EmpId = empId,
                    AttDate = today,
                    CheckInTime = now,
                    Status = status
                };
                DAL.AttendanceDAL.Insert(att);
            }
            return true;
        }

        /// <summary>
        /// 员工签退
        /// </summary>
        public static bool CheckOut(int empId)
        {
            var today = DateTime.Today;
            var existing = DAL.AttendanceDAL.GetByEmpAndDate(empId, today);
            if (existing == null) return false;

            existing.CheckOutTime = DateTime.Now;

            // 如果当前状态为正常，判断是否早退
            if (existing.Status == "正常")
            {
                var setting = DAL.AttendanceSettingDAL.GetSetting();
                if (setting != null && DateTime.Now.TimeOfDay < setting.WorkEndTime.Add(new TimeSpan(0, -setting.EarlyMinutes, 0)))
                {
                    existing.Status = "早退";
                }
            }

            DAL.AttendanceDAL.Update(existing);
            return true;
        }
    }
}
