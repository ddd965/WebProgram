using System;

namespace HRMS.Model
{
    /// <summary>
    /// 事件日志实体
    /// </summary>
    public class EventLog
    {
        public long LogId { get; set; }
        public string UserName { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string IPAddress { get; set; }
        public DateTime EventTime { get; set; }
    }
}
