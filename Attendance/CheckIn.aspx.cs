using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Attendance_CheckIn : HRMS.Common.BasePage
{
    private int CurrentCheckEmpId
    {
        get
        {
            int id;
            if (int.TryParse(ddlEmployee.SelectedValue, out id)) return id;
            return CurrentEmpId ?? 0;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!CurrentEmpId.HasValue && !IsInRole("管理员"))
        {
            Response.Redirect("~/Default.aspx");
            return;
        }
        if (!IsPostBack)
        {
            LoadEmployees();
            if (!IsInRole("管理员", "部门经理"))
            {
                ddlEmployee.Enabled = false;
            }
            RefreshUI();
        }
    }

    private void LoadEmployees()
    {
        ddlEmployee.Items.Clear();
        var emps = EmployeeBLL.Query(null, null, null, null, "在职");
        foreach (var emp in emps)
            ddlEmployee.Items.Add(new ListItem($"{emp.EmpName} ({emp.EmpNo})", emp.EmpId.ToString()));

        if (CurrentEmpId.HasValue)
        {
            var li = ddlEmployee.Items.FindByValue(CurrentEmpId.Value.ToString());
            if (li != null) ddlEmployee.SelectedValue = CurrentEmpId.Value.ToString();
        }
    }

    protected void ddlEmployee_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        int empId = CurrentCheckEmpId;
        if (empId == 0) return;
        var emp = EmployeeBLL.GetById(empId);
        if (emp == null) return;
        litEmployeeName.Text = $"{emp.EmpName} - {emp.EmpNo}";

        var now = DateTime.Now;
        litNow.Text = now.ToString("HH:mm:ss");
        litDate.Text = now.ToString("yyyy年MM月dd日 dddd");

        // 读取全局考勤参数（若未设置 → 默认 09:00上班 / 18:00下班 / 0 分钟容忍）
        var setting = AttendanceSettingBLL.GetSetting();
        var workStart = setting?.WorkStartTime ?? new TimeSpan(9, 0, 0);
        var workEnd = setting?.WorkEndTime ?? new TimeSpan(18, 0, 0);
        int lateMin = setting?.LateMinutes ?? 0;
        int earlyMin = setting?.EarlyMinutes ?? 0;

        var att = AttendanceBLL.GetByEmpAndDate(empId, DateTime.Today);
        if (att != null)
        {
            litCheckInTime.Text = att.CheckInTime.HasValue ? att.CheckInTime.Value.ToString("HH:mm:ss") : "—";
            litCheckOutTime.Text = att.CheckOutTime.HasValue ? att.CheckOutTime.Value.ToString("HH:mm:ss") : "—";

            // ★★ 两个标签独立判定 —— 不再共用同一个 att.Status ★★
            // 1) 签到状态：基于签到时间 vs 上班时间+容忍（请假/缺勤优先级最高）
            string inStatus;
            if (string.Equals(att.Status, "请假", StringComparison.Ordinal))
                inStatus = "请假";
            else if (string.Equals(att.Status, "缺勤", StringComparison.Ordinal))
                inStatus = "缺勤";
            else if (!att.CheckInTime.HasValue)
                inStatus = "未签到";
            else if (att.CheckInTime.Value.TimeOfDay > workStart.Add(new TimeSpan(0, lateMin, 0)))
                inStatus = "迟到";
            else
                inStatus = "正常";
            SetStatusLabel(lblCheckInStatus, inStatus);

            // 2) 签退状态：基于签退时间 vs 下班时间-容忍（请假/缺勤优先级最高）
            string outStatus;
            if (string.Equals(att.Status, "请假", StringComparison.Ordinal))
                outStatus = "请假";
            else if (string.Equals(att.Status, "缺勤", StringComparison.Ordinal))
                outStatus = "缺勤";
            else if (!att.CheckOutTime.HasValue)
                outStatus = "未签退";
            else if (att.CheckOutTime.Value.TimeOfDay < workEnd.Add(new TimeSpan(0, -earlyMin, 0)))
                outStatus = "早退";
            else
                outStatus = "正常";
            SetStatusLabel(lblCheckOutStatus, outStatus);

            btnCheckIn.Enabled = !att.CheckInTime.HasValue || IsInRole("管理员");
            btnCheckOut.Enabled = att.CheckInTime.HasValue && (!att.CheckOutTime.HasValue || IsInRole("管理员"));
        }
        else
        {
            // 今天还没考勤：也要显示状态标签（不再是空 span 看不见）
            litCheckInTime.Text = "未签到";
            litCheckOutTime.Text = "未签退";
            SetStatusLabel(lblCheckInStatus, "未签到");
            SetStatusLabel(lblCheckOutStatus, "未签退");

            btnCheckIn.Enabled = true;
            btnCheckOut.Enabled = false;
        }

        // ★ 本月统计 — 与 AttendanceStat.aspx 月度考勤统计 100% 同一口径方法
        //   保证：正常+迟到+早退+缺勤+请假 = 当月有效工作日数（工作日=总天数-周末，当前月=至今天工作日）；
        //        周末不计；工作日无考勤无批准请假 → 缺勤；有批准请假重叠天 → 请假。
        DateTime today = DateTime.Today;
        litMonth.Text = $"{today.Year}年{today.Month}月";
        var bd = AttendanceBLL.CalcMonthlyForEmp(empId, today.Year, today.Month);
        ltNormal.Text = bd.NormalDays.ToString();
        ltLate.Text   = bd.LateCount.ToString();
        ltEarly.Text  = bd.EarlyCount.ToString();
        ltAbsent.Text = bd.AbsentCount.ToString();
        ltLeave.Text  = bd.LeaveCount.ToString();
    }

    private void SetStatusLabel(Label lbl, string status)
    {
        lbl.Text = status;
        switch (status)
        {
            case "正常": lbl.CssClass = "label label-success"; break;
            case "迟到":
            case "早退": lbl.CssClass = "label label-warning"; break;
            case "缺勤": lbl.CssClass = "label label-danger"; break;
            case "请假": lbl.CssClass = "label label-info"; break;
            default: lbl.CssClass = "label label-default"; break;
        }
    }

    protected void btnCheckIn_Click(object sender, EventArgs e)
    {
        int empId = CurrentCheckEmpId;
        if (empId == 0) return;
        AttendanceBLL.CheckIn(empId);
        var emp = EmployeeBLL.GetById(empId);
        WriteLog.Write(CurrentUserName, "签到", $"员工 {emp?.EmpName} 于 {DateTime.Now:HH:mm:ss} 签到");
        lblMsg.Text = "签到成功！";
        RefreshUI();
    }

    protected void btnCheckOut_Click(object sender, EventArgs e)
    {
        int empId = CurrentCheckEmpId;
        if (empId == 0) return;
        bool ok = AttendanceBLL.CheckOut(empId);
        if (!ok)
        {
            lblMsg.Text = "请先签到再签退！";
            lblMsg.CssClass = "text-danger";
            return;
        }
        var emp = EmployeeBLL.GetById(empId);
        WriteLog.Write(CurrentUserName, "签退", $"员工 {emp?.EmpName} 于 {DateTime.Now:HH:mm:ss} 签退");
        lblMsg.Text = "签退成功！";
        lblMsg.CssClass = "text-info";
        RefreshUI();
    }
}
