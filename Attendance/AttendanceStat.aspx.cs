using System;
using System.Data;
using System.Data.SqlClient;
using HRMS.BLL;
using HRMS.DAL;
using HRMS.Common;

public partial class Attendance_AttendanceStat : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindDepartments();
            BindYears();
            BindMonths();
        }
    }

    private void BindDepartments()
    {
        var departments = DepartmentBLL.GetAll();
        foreach (var dept in departments)
        {
            ddlDepartment.Items.Add(new System.Web.UI.WebControls.ListItem(
                dept.DeptName, dept.DeptId.ToString()));
        }
    }

    private void BindYears()
    {
        int currentYear = DateTime.Now.Year;
        for (int y = currentYear - 2; y <= currentYear; y++)
        {
            ddlYear.Items.Add(new System.Web.UI.WebControls.ListItem(y.ToString(), y.ToString()));
        }
        ddlYear.SelectedValue = currentYear.ToString();
    }

    private void BindMonths()
    {
        int currentMonth = DateTime.Now.Month;
        for (int m = 1; m <= 12; m++)
        {
            ddlMonth.Items.Add(new System.Web.UI.WebControls.ListItem(m.ToString(), m.ToString()));
        }
        ddlMonth.SelectedValue = currentMonth.ToString();
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        int? deptId = null;
        if (!string.IsNullOrEmpty(ddlDepartment.SelectedValue))
        {
            deptId = int.Parse(ddlDepartment.SelectedValue);
        }

        int year = int.Parse(ddlYear.SelectedValue);
        int month = int.Parse(ddlMonth.SelectedValue);

        DataTable dt = AttendanceDAL.StatByDeptAndMonth(deptId, year, month);
        gvAttendanceStat.DataSource = dt;
        gvAttendanceStat.DataBind();
    }
}