using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Attendance_AttendanceStat : HRMS.Common.BasePage
{
    private static readonly DateTime StartDate = new DateTime(2026, 5, 1);

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsInRole("管理员", "部门经理"))
        {
            Response.Redirect("~/Default.aspx");
            return;
        }
        if (!IsPostBack)
        {
            var depts = DepartmentBLL.GetAll();
            foreach (var d in depts)
                ddlDept.Items.Add(new ListItem(d.DeptName, d.DeptId.ToString()));

            int curYear = DateTime.Now.Year;
            int fromY = Math.Min(2026, curYear);
            int toY = Math.Max(2026, curYear) + 1;
            for (int y = fromY; y <= toY; y++)
                ddlYear.Items.Add(new ListItem(y + "年", y.ToString()));
            for (int m = 1; m <= 12; m++)
                ddlMonth.Items.Add(new ListItem(m + "月", m.ToString()));

            // 默认 2026-05 或今天所在月（若已经到了）
            var def = DateTime.Today < StartDate ? StartDate : DateTime.Today;
            ddlYear.SelectedValue = def.Year.ToString();
            ddlMonth.SelectedValue = def.Month.ToString();

            DoStat();
        }
    }

    protected void btnStat_Click(object sender, EventArgs e)
    {
        gvStat.PageIndex = 0;
        DoStat();
    }

    private void DoStat()
    {
        int? deptId = string.IsNullOrEmpty(ddlDept.SelectedValue) ? (int?)null : int.Parse(ddlDept.SelectedValue);
        // 部门经理只能看自己部门
        if (IsInRole("部门经理") && !IsInRole("管理员") && CurrentEmpId.HasValue)
        {
            var emp = EmployeeBLL.GetById(CurrentEmpId.Value);
            if (emp != null && emp.DeptId.HasValue)
            {
                deptId = emp.DeptId.Value;
                ddlDept.SelectedValue = deptId.ToString();
                ddlDept.Enabled = false;
            }
        }

        int year = int.Parse(ddlYear.SelectedValue);
        int month = int.Parse(ddlMonth.SelectedValue);
        var ms = new DateTime(year, month, 1);
        int daysInMonth = DateTime.DaysInMonth(year, month);

        // 取员工列表（按部门过滤）
        var emps = EmployeeBLL.Query(null, null, deptId, null, null);
        // 若没填在职状态，再过滤一遍部门
        if (!deptId.HasValue)
            emps = emps.Where(x => x.Status == "在职").ToList();
        else
            emps = emps.Where(x => x.Status == "在职" && x.DeptId == deptId.Value).ToList();

        // 部门经理视图可能没有 deptId 但要限制，这里重新按 deptId 处理
        if (deptId.HasValue)
            emps = emps.Where(x => x.DeptId == deptId.Value).ToList();
        else
            emps = emps.Where(x => x.Status == "在职").ToList();

        // 建部门ID->名字缓存
        var deptNameMap = new Dictionary<int, string>();
        foreach (var d in DepartmentBLL.GetAll())
            deptNameMap[d.DeptId] = d.DeptName;

        var dt = new DataTable();
        dt.Columns.Add("EmpNo", typeof(string));
        dt.Columns.Add("EmpName", typeof(string));
        dt.Columns.Add("DeptName", typeof(string));
        dt.Columns.Add("NormalDays", typeof(int));
        dt.Columns.Add("LateCount", typeof(int));
        dt.Columns.Add("EarlyCount", typeof(int));
        dt.Columns.Add("AbsentCount", typeof(int));
        dt.Columns.Add("LeaveCount", typeof(int));

        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        foreach (var emp in emps)
        {
            // 迟到/早退/缺勤 读取考勤统计数据（Attendance table）
            var attList = AttendanceBLL.QueryByMonth(emp.EmpId, year, month);
            int late = attList.Count(a => a.Status == "迟到");
            int early = attList.Count(a => a.Status == "早退");
            int absent = attList.Count(a => a.Status == "缺勤");
            // 注意：正常不计入 Attendance 的正常，因为 Leave 单独计算，最后 NormalDays = 当月总 - (late+early+absent+leave)

            // 请假天数读取休假统计数据（LeaveRecord，已批准）
            var leaves = LeaveRecordBLL.Query(emp.EmpId, null, from, to, "已批准");
            int leaveDays = 0;
            if (leaves != null)
            {
                foreach (var lv in leaves)
                {
                    leaveDays += CountLeaveDaysInMonth(lv, year, month);
                }
            }

            // 保证：正常+迟到+早退+缺勤+请假 = 当月总天数
            int normal = daysInMonth - late - early - absent - leaveDays;
            if (normal < 0) normal = 0; // 防御

            string deptName = emp.DeptId.HasValue && deptNameMap.ContainsKey(emp.DeptId.Value)
                ? deptNameMap[emp.DeptId.Value] : "未分配";

            dt.Rows.Add(emp.EmpNo, emp.EmpName, deptName, normal, late, early, absent, leaveDays);
        }

        gvStat.DataSource = dt;
        gvStat.DataBind();

        if (ms < StartDate)
            lblMsg.Text = $"{year}年{month}月：2026-05 之前无签到记录；已按「正常=当月{daysInMonth}天-迟到-早退-缺勤-请假」显示基础统计。共 {dt.Rows.Count} 位员工。";
        else
            lblMsg.Text = $"{year}年{month}月 共 {dt.Rows.Count} 位员工考勤统计（正常+迟到+早退+缺勤+请假 = 当月 {daysInMonth} 天）。";
    }

    /// <summary>
    /// 按请假单 overlap 当月日期的天数（含开始/结束两天）；不考虑周末/节假日，保持与明细页一致
    /// </summary>
    private int CountLeaveDaysInMonth(LeaveRecord lv, int year, int month)
    {
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var s = lv.StartDate < monthStart ? monthStart : lv.StartDate;
        var e = lv.EndDate > monthEnd ? monthEnd : lv.EndDate;
        if (e < s) return 0;
        return (int)(e - s).TotalDays + 1;
    }

    protected void gvStat_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvStat.PageIndex = e.NewPageIndex;
        DoStat();
    }
}
