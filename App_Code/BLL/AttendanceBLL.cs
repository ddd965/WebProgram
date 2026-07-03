using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using HRMS.Model;

namespace HRMS.BLL
{
    public class AttendanceBLL
    {
        public static List<Attendance> GetAll() => DAL.AttendanceDAL.GetAll();
        public static Attendance GetById(int attId) => DAL.AttendanceDAL.GetById(attId);
        public static Attendance GetByEmpAndDate(int empId, DateTime attDate) => DAL.AttendanceDAL.GetByEmpAndDate(empId, attDate);
        public static int Insert(Attendance a) => DAL.AttendanceDAL.Insert(a);
        public static int Update(Attendance a) => DAL.AttendanceDAL.Update(a);
        public static int Delete(int attId) => DAL.AttendanceDAL.Delete(attId);
        public static List<Attendance> QueryByMonth(int empId, int year, int month) => DAL.AttendanceDAL.QueryByMonth(empId, year, month);
        public static DataTable StatByDeptAndMonth(int? deptId, int year, int month) => StatMonthly(deptId, year, month);

        /// <summary>
        /// 月度考勤统一统计口径（AttendanceStat.aspx + Report/AttendanceMonthly.aspx 共同调用）：
        ///   - 工作日 = 当月总天数 − 周六日（已完结月）/ 1号至今天之间的工作日（当前月）/ 0（未来月）
        ///   - 正常 + 迟到 + 早退 + 缺勤 + 请假 = workDays
        ///   - 周末不计入 5 项
        ///   - LeaveRecord 已批准重叠天 → 请假
        ///   - Attendance 无记录 + 非周末 + 非请假 → 缺勤（与按月考勤明细页一致）
        /// 返回列：EmpNo, EmpName, DeptName, NormalDays, LateCount, EarlyCount, AbsentCount, LeaveCount, WorkDays
        /// </summary>
        public static DataTable StatMonthly(int? deptId, int year, int month)
        {
            var today = DateTime.Today;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            // 1) 计算整月工作日（总天数 - 周末天数）
            int workDaysInFull = 0;
            for (int d = 1; d <= daysInMonth; d++)
            {
                var dateTmp = new DateTime(year, month, d);
                if (dateTmp.DayOfWeek != DayOfWeek.Saturday && dateTmp.DayOfWeek != DayOfWeek.Sunday)
                    workDaysInFull++;
            }

            // 2) 按阶段取有效工作日数 & 循环截止天号
            int workDays;
            int loopEndDay;
            if (year < today.Year || (year == today.Year && month < today.Month))
            {
                workDays = workDaysInFull;
                loopEndDay = daysInMonth;
            }
            else if (year == today.Year && month == today.Month)
            {
                int wd = 0;
                for (int d = 1; d <= today.Day; d++)
                {
                    var dateTmp = new DateTime(year, month, d);
                    if (dateTmp.DayOfWeek != DayOfWeek.Saturday && dateTmp.DayOfWeek != DayOfWeek.Sunday)
                        wd++;
                }
                workDays = wd;
                loopEndDay = today.Day;
            }
            else
            {
                workDays = 0;
                loopEndDay = 0;
            }

            // 3) 有效员工 = 非离职（含在职/试用期/实习期）
            var emps = EmployeeBLL.Query(null, null, deptId, null, null)
                .Where(x => x.Status != "离职")
                .ToList();
            if (deptId.HasValue)
                emps = emps.Where(x => x.DeptId == deptId.Value).ToList();

            var deptNameMap = new Dictionary<int, string>();
            foreach (var d in DepartmentBLL.GetAll())
                deptNameMap[d.DeptId] = d.DeptName;

            // 4) 结果表
            var statDt = new DataTable();
            statDt.Columns.Add("EmpNo", typeof(string));
            statDt.Columns.Add("EmpName", typeof(string));
            statDt.Columns.Add("DeptName", typeof(string));
            statDt.Columns.Add("NormalDays", typeof(int));
            statDt.Columns.Add("LateCount", typeof(int));
            statDt.Columns.Add("EarlyCount", typeof(int));
            statDt.Columns.Add("AbsentCount", typeof(int));
            statDt.Columns.Add("LeaveCount", typeof(int));
            statDt.Columns.Add("WorkDays", typeof(int));

            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1).AddDays(-1);

            foreach (var emp in emps)
            {
                int normal = 0, late = 0, early = 0, absent = 0, leave = 0;

                if (loopEndDay > 0)
                {
                    // a) 考勤按天索引
                    var attList = QueryByMonth(emp.EmpId, year, month);
                    var attByDay = new Dictionary<int, string>();
                    foreach (var a in attList)
                        if (a != null) attByDay[a.AttDate.Day] = a.Status ?? "";

                    // b) 批准的请假按天号集合
                    var leaves = LeaveRecordBLL.Query(emp.EmpId, null, from, to, "已批准");
                    var leaveDays = new HashSet<int>();
                    if (leaves != null)
                    {
                        var msDt = new DateTime(year, month, 1);
                        var meDt = msDt.AddMonths(1).AddDays(-1);
                        foreach (var lv in leaves)
                        {
                            var s = lv.StartDate < msDt ? msDt : lv.StartDate;
                            var eDt = lv.EndDate > meDt ? meDt : lv.EndDate;
                            for (var dt2 = s; dt2 <= eDt; dt2 = dt2.AddDays(1))
                                leaveDays.Add(dt2.Day);
                        }
                    }

                    // c) 逐日判断（与 Detail 页完全一致）
                    for (int day = 1; day <= loopEndDay; day++)
                    {
                        var dateTmp = new DateTime(year, month, day);
                        bool isWeekend = dateTmp.DayOfWeek == DayOfWeek.Saturday || dateTmp.DayOfWeek == DayOfWeek.Sunday;
                        if (isWeekend) continue;

                        if (leaveDays.Contains(day))
                        {
                            leave++;
                            continue;
                        }
                        string st;
                        if (attByDay.TryGetValue(day, out st))
                        {
                            switch (st)
                            {
                                case "正常": normal++; break;
                                case "迟到": late++; break;
                                case "早退": early++; break;
                                case "请假": leave++; break;
                                case "缺勤": absent++; break;
                                default: normal++; break;
                            }
                        }
                        else
                        {
                            absent++;   // 工作日 + 没请假 + 没签到 = 缺勤
                        }
                    }
                }

                // 兜底：保证 5 项和 = workDays
                int sum5 = normal + late + early + absent + leave;
                if (sum5 != workDays)
                {
                    int diff = workDays - sum5;
                    if (diff > 0) normal += diff;
                    else absent += Math.Abs(diff);
                }
                if (normal < 0) normal = 0;
                if (late < 0) late = 0;
                if (early < 0) early = 0;
                if (absent < 0) absent = 0;
                if (leave < 0) leave = 0;

                string deptName = emp.DeptId.HasValue && deptNameMap.ContainsKey(emp.DeptId.Value)
                    ? deptNameMap[emp.DeptId.Value] : "未分配";

                statDt.Rows.Add(emp.EmpNo, emp.EmpName, deptName, normal, late, early, absent, leave, workDays);
            }

            return statDt;
        }

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
