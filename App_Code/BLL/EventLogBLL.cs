using System;
using System.Collections.Generic;
using HRMS.Model;

namespace HRMS.BLL
{
    public class EventLogBLL
    {
        public static List<EventLog> GetAll() => DAL.EventLogDAL.GetAll();
        public static EventLog GetById(long logId) => DAL.EventLogDAL.GetById(logId);

        public static long Insert(EventLog log) => DAL.EventLogDAL.Insert(log);
        public static int Delete(long logId) => DAL.EventLogDAL.Delete(logId);

        /// <summary>
        /// 批量删除过期日志（默认删除6个月前）
        /// </summary>
        public static int DeleteOldLogs(int monthsBefore = 6)
        {
            return DAL.EventLogDAL.DeleteBefore(DateTime.Now.AddMonths(-monthsBefore));
        }

        public static List<EventLog> Query(string userName, DateTime? startDate,
            DateTime? endDate, string eventName)
            => DAL.EventLogDAL.Query(userName, startDate, endDate, eventName);
    }
}
