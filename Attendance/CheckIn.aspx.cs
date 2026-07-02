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

        var att = AttendanceBLL.GetByEmpAndDate(empId, DateTime.Today);
        if (att != null)
        {
            litCheckInTime.Text = att.CheckInTime.HasValue ? att.CheckInTime.Value.ToString("HH:mm:ss") : "—";
            litCheckOutTime.Text = att.CheckOutTime.HasValue ? att.CheckOutTime.Value.ToString("HH:mm:ss") : "—";
            SetStatusLabel(lblCheckInStatus, att.Status);
            SetStatusLabel(lblCheckOutStatus, att.Status);
            btnCheckIn.Enabled = !att.CheckInTime.HasValue || IsInRole("管理员");
            btnCheckOut.Enabled = att.CheckInTime.HasValue && (!att.CheckOutTime.HasValue || IsInRole("管理员"));
        }
        else
        {
            litCheckInTime.Text = "未签到";
            litCheckOutTime.Text = "未签退";
            btnCheckIn.Enabled = true;
            btnCheckOut.Enabled = false;
        }

        // 本月统计
        DateTime today = DateTime.Today;
        litMonth.Text = $"{today.Year}年{today.Month}月";
        var list = AttendanceBLL.QueryByMonth(empId, today.Year, today.Month);
        ltNormal.Text = list.Count(x => x.Status == "正常").ToString();
        ltLate.Text = list.Count(x => x.Status == "迟到").ToString();
        ltEarly.Text = list.Count(x => x.Status == "早退").ToString();
        ltAbsent.Text = list.Count(x => x.Status == "缺勤").ToString();
        ltLeave.Text = list.Count(x => x.Status == "请假").ToString();
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
