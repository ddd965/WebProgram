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

            // ================================================================
            //  默认月 = 上个月完整数据（HR 惯例）
            //  例：今天 2026-07-03 → 默认 2026-06，
            //      6 月是完整月（30 天、22 工作日），用户想看的一定是上个月全量
            //      而不是 7 月才过了 3 天、几乎所有员工都"缺勤 3 天"的不完整视图
            //  最早不超过 StartDate(2026-05)
            // ================================================================
            var def = DateTime.Today.AddMonths(-1);
            if (def < StartDate) def = StartDate;
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

        // ======== 计算本月工作日(非周末)：5项和 = 工作日 ========
        // 规则：
        //  - 已完结月(或更早)：workDays = 整月工作日(总天数-周末天数)
        //  - 当前月未过完：   workDays = 本月1号~今天为止的工作日数
        //  - 未来月：         workDays = 0
        var today = DateTime.Today;
        int workDaysInFull = 0;
        for (int d = 1; d <= daysInMonth; d++)
        {
            var dateTmp = new DateTime(year, month, d);
            if (dateTmp.DayOfWeek != DayOfWeek.Saturday && dateTmp.DayOfWeek != DayOfWeek.Sunday)
                workDaysInFull++;
        }

        int workDaysDisplay;
        string stage;
        if (year < today.Year || (year == today.Year && month < today.Month))
        {
            workDaysDisplay = workDaysInFull;
            stage = "已完结月";
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
            workDaysDisplay = wd;
            stage = "当前月未过完，截至" + today.ToString("yyyy-MM-dd");
        }
        else
        {
            workDaysDisplay = 0;
            stage = "未来月";
        }

        string dayRuleMsg = stage + "：正常+迟到+早退+缺勤+请假 = 工作日 " + workDaysDisplay + " 天（总" + daysInMonth + "天-周末" + (daysInMonth - workDaysInFull) + "天）";

        // =========== 统计核心：调用 BLL 统一口径 ===========
        // （不再在页面重复实现，彻底避免 CS0136 dt 内外层重名 & 报表口径不一致）
        DataTable statDt = AttendanceBLL.StatMonthly(deptId, year, month);

        gvStat.DataSource = statDt;
        gvStat.DataBind();

        if (ms < StartDate)
            lblMsg.Text = year + "年" + month + "月：2026-05 之前无签到记录（" + dayRuleMsg + "）。共 " + statDt.Rows.Count + " 位员工。";
        else
            lblMsg.Text = year + "年" + month + "月 共 " + statDt.Rows.Count + " 位员工考勤统计（" + dayRuleMsg + "）。";
    }

    protected void gvStat_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvStat.PageIndex = e.NewPageIndex;
        DoStat();
    }
}
