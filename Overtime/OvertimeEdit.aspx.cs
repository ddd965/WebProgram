using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using HRMS.Model;
using HRMS.BLL;
using HRMS.DAL;
using HRMS.Common;
using System.Data.SqlClient;

public partial class Overtime_OvertimeEdit : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindEmployees();
            BindGrid();
        }
    }

    private void BindEmployees()
    {
        List<Employee> employees = EmployeeBLL.GetAll();
        foreach (var emp in employees)
        {
            ddlEmployee.Items.Add(new ListItem(emp.EmpNo + " - " + emp.EmpName, emp.EmpId.ToString()));
        }
    }

    private void BindGrid()
    {
        DataTable dt = GetOvertimeWithEmpName();
        gvOvertime.DataSource = dt;
        gvOvertime.DataBind();
    }

    private DataTable GetOvertimeWithEmpName()
    {
        string sql = @"SELECT o.OtId, o.EmpId, e.EmpName, o.OtDate, o.StartTime, o.EndTime,
                       o.OtHours, o.OtType, o.Reason, o.Status, o.CreateTime
                       FROM Overtime o
                       JOIN Employee e ON o.EmpId = e.EmpId
                       ORDER BY o.OtDate DESC";
        return DBHelper.ExecuteDataTable(sql);
    }

    protected void gvOvertime_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gvOvertime.EditIndex = e.NewEditIndex;
        BindGrid();
    }

    protected void gvOvertime_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        int otId = Convert.ToInt32(gvOvertime.DataKeys[e.RowIndex].Values["OtId"]);
        int empId = Convert.ToInt32(gvOvertime.DataKeys[e.RowIndex].Values["EmpId"]);
        GridViewRow row = gvOvertime.Rows[e.RowIndex];

        DateTime otDate = Convert.ToDateTime(((TextBox)row.Cells[2].Controls[0]).Text);
        TimeSpan startTime = TimeSpan.Parse(((TextBox)row.FindControl("txtStartTime")).Text);
        TimeSpan endTime = TimeSpan.Parse(((TextBox)row.FindControl("txtEndTime")).Text);
        string otType = ((DropDownList)row.FindControl("ddlOtType")).SelectedValue;
        string status = ((DropDownList)row.FindControl("ddlStatus")).SelectedValue;
        string reason = ((TextBox)row.Cells[8].Controls[0]).Text;

        Overtime ot = new Overtime
        {
            OtId = otId,
            EmpId = empId,
            OtDate = otDate,
            StartTime = startTime,
            EndTime = endTime,
            OtType = otType,
            Status = status,
            Reason = reason
        };

        OvertimeBLL.Update(ot);
        WriteLog.Write(CurrentUserName, "修改加班", "修改加班记录 ID=" + otId);

        gvOvertime.EditIndex = -1;
        BindGrid();
    }

    protected void gvOvertime_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        gvOvertime.EditIndex = -1;
        BindGrid();
    }

    protected void gvOvertime_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        int otId = Convert.ToInt32(gvOvertime.DataKeys[e.RowIndex].Values["OtId"]);
        OvertimeBLL.Delete(otId);
        WriteLog.Write(CurrentUserName, "删除加班", "删除加班记录 ID=" + otId);
        BindGrid();
    }

    protected void btnInsert_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        int empId = int.Parse(ddlEmployee.SelectedValue);
        DateTime otDate = DateTime.Parse(txtOtDate.Text);
        TimeSpan startTime = TimeSpan.Parse(txtStartTime.Text);
        TimeSpan endTime = TimeSpan.Parse(txtEndTime.Text);
        string otType = ddlOtTypeNew.SelectedValue;
        string status = ddlStatusNew.SelectedValue;
        string reason = txtReason.Text.Trim();

        Overtime ot = new Overtime
        {
            EmpId = empId,
            OtDate = otDate,
            StartTime = startTime,
            EndTime = endTime,
            OtType = otType,
            Status = status,
            Reason = reason
        };

        OvertimeBLL.Insert(ot);
        WriteLog.Write(CurrentUserName, "新增加班", "新增加班记录 员工ID=" + empId);

        BindGrid();
        lblMsg.Text = "新增成功！自动计算加班小时：" + ot.OtHours;
    }
}