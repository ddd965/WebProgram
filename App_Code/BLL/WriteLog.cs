using System;
using System.Web;

namespace HRMS.BLL
{
    /// <summary>
    /// 统一事件日志写入工具类
    /// </summary>
    public static class WriteLog
    {
        /// <summary>
        /// 记录操作日志
        /// </summary>
        /// <param name="userName">操作用户名</param>
        /// <param name="eventName">事件名：登录/新增/修改/删除/审批/导出/签到/签退 等</param>
        /// <param name="description">详细描述</param>
        public static void Write(string userName, string eventName, string description)
        {
            try
            {
                string ip = string.Empty;
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    ip = HttpContext.Current.Request.UserHostAddress;
                }
                var log = new Model.EventLog
                {
                    UserName = userName,
                    EventName = eventName,
                    Description = description,
                    IPAddress = ip
                };
                DAL.EventLogDAL.Insert(log);
            }
            catch
            {
                // 日志写入失败不应影响主流程
            }
        }
    }
}
