using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Attendance_AttendanceDetail : HRMS.Common.BasePage
{
    private static readonly DateTime StartDate = new DateTime(2026, 5, 1);

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // 加载员工列表
            var emps = IsInRole("管理员", "部门经理")
                ? EmployeeBLL.Query(null, null, null, null, "在职")
                : new List<Employee>();
            // 普通员工只能看自己
            if (!IsInRole("管理员", "部门经理") && CurrentEmpId.HasValue)
            {
                var me = EmployeeBLL.GetById(CurrentEmpId.Value);
                if (me != null) emps = new List<Employee> { me };
            }
            // 部门经理只看自己部门
            if (IsInRole("部门经理") && !IsInRole("管理员") && CurrentEmpId.HasValue)
            {
                var me = EmployeeBLL.GetById(CurrentEmpId.Value);
                if (me != null && me.DeptId.HasValue)
                {
                    emps = emps.Where(x => x.DeptId == me.DeptId.Value).ToList();
                }
            }
            foreach (var emp in emps)
                ddlEmp.Items.Add(new ListItem($"{emp.EmpName} ({emp.EmpNo})", emp.EmpId.ToString()));

            // 年份：从 2026 到 当前+1，默认 2026
            int curYear = DateTime.Now.Year;
            int fromY = Math.Min(2026, curYear);
            int toY = Math.Max(2026, curYear) + 1;
            for (int y = fromY; y <= toY; y++)
                ddlYear.Items.Add(new ListItem(y + "年", y.ToString()));
            // 默认 2026 年 5 月；但如果今天大于 2026-5，就默认今天所在月
            var def = DateTime.Today < StartDate ? StartDate : DateTime.Today;
            ddlYear.SelectedValue = def.Year.ToString();
            ddlMonth.SelectedValue = def.Month.ToString();

            // 普通员工默认选自己
            if (!IsInRole("管理员", "部门经理") && CurrentEmpId.HasValue)
            {
                var li = ddlEmp.Items.FindByValue(CurrentEmpId.Value.ToString());
                if (li != null) ddlEmp.SelectedValue = CurrentEmpId.Value.ToString();
                ddlEmp.Enabled = false;
            }

            BindView();
        }
    }

    protected void btnQuery_Click(object sender, EventArgs e)
    {
        gvDetail.PageIndex = 0;
        BindView();
    }

    protected void rbViewMode_CheckedChanged(object sender, EventArgs e)
    {
        pnlList.Visible = rbList.Checked;
        pnlCalendar.Visible = rbCalendar.Checked;
        BindView();
    }

    protected string GetStatusCss(string s)
    {
        switch (s)
        {
            case "正常": return "label label-success";
            case "迟到": return "label label-warning";
            case "早退": return "label label-warning";
            case "缺勤": return "label label-danger";
            case "请假": return "label label-info";
            case "周末": return "label label-default";
            default: return "label label-default";
        }
    }

    private List<AttendanceViewRow> _cachedList;

    private void LoadData()
    {
        _cachedList = new List<AttendanceViewRow>();
        int empId = int.Parse(ddlEmp.SelectedValue);
        int year = int.Parse(ddlYear.SelectedValue);
        int month = int.Parse(ddlMonth.SelectedValue);

        DateTime monthStart = new DateTime(year, month, 1);
        DateTime today = DateTime.Today;

        // 2026-05 之前的签到记录为空（不生成行）
        if (monthStart < StartDate)
        {
            return;
        }

        int daysInMonth = DateTime.DaysInMonth(year, month);
        var attDict = new Dictionary<int, Attendance>();
        var realList = AttendanceBLL.QueryByMonth(empId, year, month);
        foreach (var a in realList)
            if (a != null) attDict[a.AttDate.Day] = a;

        // 当月截止日：超过今天的未来日期，不显示（因为还没开始签到，记录不存在）
        int maxDay = daysInMonth;
        if (monthStart.AddDays(daysInMonth - 1) > today)
        {
            if (monthStart > today)
            {
                // 整月是未来，不显示任何
                return;
            }
            maxDay = today.Day;
            if (year != today.Year || month != today.Month)
            {
                // 如果今天不在当前月（当前月在今天之前但 >= 2026-05），显示全月
                maxDay = daysInMonth;
            }
        }

        var list = new List<AttendanceViewRow>();
        for (int d = 1; d <= maxDay; d++)
        {
            var date = new DateTime(year, month, d);
            AttendanceViewRow row;
            if (attDict.ContainsKey(d))
            {
                var a = attDict[d];
                row = new AttendanceViewRow
                {
                    AttDate = date,
                    CheckInTimeStr = a.CheckInTime.HasValue ? a.CheckInTime.Value.ToString("HH:mm:ss") : "—",
                    CheckOutTimeStr = a.CheckOutTime.HasValue ? a.CheckOutTime.Value.ToString("HH:mm:ss") : "—",
                    WorkHours = a.CheckInTime.HasValue && a.CheckOutTime.HasValue
                        ? Math.Round((decimal)(a.CheckOutTime.Value - a.CheckInTime.Value).TotalHours, 2)
                        : 0,
                    Status = a.Status,
                    Remark = a.Remark
                };
            }
            else
            {
                // 周末不视为缺勤
                bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                // 当天有请假记录则视为请假
                bool onLeave = false;
                var leaves = LeaveRecordBLL.Query(empId, null, date, date, "已批准");
                if (leaves != null && leaves.Count > 0) onLeave = true;

                row = new AttendanceViewRow
                {
                    AttDate = date,
                    CheckInTimeStr = "—",
                    CheckOutTimeStr = "—",
                    WorkHours = 0,
                    Status = onLeave ? "请假" : (isWeekend ? "周末" : "缺勤"),
                    Remark = isWeekend ? "休息日" : ""
                };
            }
            list.Add(row);
        }
        _cachedList = list;
    }

    private void BindView()
    {
        if (string.IsNullOrEmpty(ddlEmp.SelectedValue))
        {
            lblMsg.Text = "暂无员工可选。";
            return;
        }
        int year = int.Parse(ddlYear.SelectedValue);
        int month = int.Parse(ddlMonth.SelectedValue);
        LoadData();

        if (rbList.Checked)
        {
            gvDetail.DataSource = _cachedList;
            gvDetail.DataBind();
        }

        if (rbCalendar.Checked)
        {
            RenderCalendar();
        }

        var emp = EmployeeBLL.GetById(int.Parse(ddlEmp.SelectedValue));
        var ms = new DateTime(year, month, 1);
        if (ms < StartDate)
            lblMsg.Text = $"{emp?.EmpName} {year}年{month}月：2026-05 之前无签到记录。";
        else
            lblMsg.Text = $"{emp?.EmpName} {year}年{month}月 共显示 {_cachedList.Count} 天（今日之后不显示）。";
    }

    private void RenderCalendar()
    {
        int year = int.Parse(ddlYear.SelectedValue);
        int month = int.Parse(ddlMonth.SelectedValue);
        litCalendarTitle.Text = $"{year} 年 {month} 月 考勤日历（{_cachedList.Count} 天）";

        var first = new DateTime(year, month, 1);
        int startOffset = (int)first.DayOfWeek; // 0=Sun
        int daysInMonth = DateTime.DaysInMonth(year, month);

        // 把考勤数据映射到天
        var map = new Dictionary<int, AttendanceViewRow>();
        foreach (var r in _cachedList) map[r.AttDate.Day] = r;

        var sb = new StringBuilder();
        sb.Append("<table class='table table-bordered table-condensed text-center' style='font-size:12px;'>");
        sb.Append("<thead><tr><th style='color:#d9534f;'>日</th><th>一</th><th>二</th><th>三</th><th>四</th><th>五</th><th style='color:#d9534f;'>六</th></tr></thead>");
        sb.Append("<tbody><tr>");
        for (int i = 0; i < startOffset; i++)
            sb.Append("<td style='background:#f9f9f9;'>&nbsp;</td>");
        for (int d = 1; d <= daysInMonth; d++)
        {
            int cellIndex = (startOffset + d - 1) % 7;
            if (cellIndex == 0 && d != 1) sb.Append("</tr><tr>");

            if (map.ContainsKey(d))
            {
                var row = map[d];
                string css = "bg-default", label = "", pop = "";
                switch (row.Status)
                {
                    case "正常": css = "background-color:#dff0d8;"; label = "<span class='label label-success'>正常</span>"; break;
                    case "迟到": css = "background-color:#fcf8e3;"; label = "<span class='label label-warning'>迟到</span>"; break;
                    case "早退": css = "background-color:#fcf8e3;"; label = "<span class='label label-warning'>早退</span>"; break;
                    case "缺勤": css = "background-color:#f2dede;"; label = "<span class='label label-danger'>缺勤</span>"; break;
                    case "请假": css = "background-color:#d9edf7;"; label = "<span class='label label-info'>请假</span>"; break;
                    case "周末": css = "background-color:#f5f5f5;"; label = "<span class='text-muted'>周末</span>"; break;
                }
                pop = $"<br/><small>{row.CheckInTimeStr} / {row.CheckOutTimeStr}</small>";
                sb.Append($"<td style='{css}vertical-align:top;height:80px;'><b>{d}</b><br/>{label}{pop}</td>");
            }
            else
            {
                // 未来日期 / 之前月：显示空白
                sb.Append($"<td style='background:#fafafa;vertical-align:top;height:80px;color:#bbb;'><b>{d}</b><br/><small>—</small></td>");
            }
        }
        int tail = 7 - ((startOffset + daysInMonth) % 7);
        if (tail < 7)
            for (int i = 0; i < tail; i++)
                sb.Append("<td style='background:#f9f9f9;'>&nbsp;</td>");
        sb.Append("</tr></tbody></table>");
        litCalendar.Text = sb.ToString();
    }

    protected void gvDetail_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvDetail.PageIndex = e.NewPageIndex;
        BindView();
    }

    public class AttendanceViewRow
    {
        public DateTime AttDate { get; set; }
        public string CheckInTimeStr { get; set; }
        public string CheckOutTimeStr { get; set; }
        public decimal WorkHours { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
    }
}
