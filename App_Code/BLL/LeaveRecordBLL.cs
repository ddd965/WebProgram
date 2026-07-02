using System;
using System.Collections.Generic;
using HRMS.Model;

namespace HRMS.BLL
{
    public class LeaveRecordBLL
    {
        public static List<LeaveRecord> GetAll() => DAL.LeaveRecordDAL.GetAll();
        public static LeaveRecord GetById(int leaveId) => DAL.LeaveRecordDAL.GetById(leaveId);

        public static int Insert(LeaveRecord lr) => DAL.LeaveRecordDAL.Insert(lr);
        public static int Update(LeaveRecord lr) => DAL.LeaveRecordDAL.Update(lr);
        public static int Delete(int leaveId) => DAL.LeaveRecordDAL.Delete(leaveId);

        /// <summary>
        /// 审批休假申请
        /// </summary>
        public static int Approve(int leaveId, string status, int approverId)
            => DAL.LeaveRecordDAL.Approve(leaveId, status, approverId);

        /// <summary>
        /// 计算工作日天数（剔除周末）
        /// </summary>
        public static decimal CalculateLeaveDays(DateTime startDate, DateTime endDate)
        {
            int days = 0;
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    days++;
            }
            return days;
        }

        public static List<LeaveRecord> Query(int? empId, int? leaveTypeId, DateTime? startDate,
            DateTime? endDate, string status)
            => DAL.LeaveRecordDAL.Query(empId, leaveTypeId, startDate, endDate, status);

        public static System.Data.DataTable StatByTypeAndMonth(int? deptId, string month)
            => DAL.LeaveRecordDAL.StatByTypeAndMonth(deptId, month);
    }
}
