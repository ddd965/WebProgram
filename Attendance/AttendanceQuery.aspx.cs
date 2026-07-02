using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HRMS.BLL;
using HRMS.Model;
using HRMS.DAL;
using HRMS.Common;

public partial class Attendance_AttendanceQuery : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindEmployees();
            BindYears();
            BindMonths();
        }
    }

    private void BindEmployees()
    {
        List<Employee> employees = EmployeeBLL.GetAll();
        foreach (var emp in employees)
        {
            ddlEmployee.Items.Add(new System.Web.UI.WebControls.ListItem(
                emp.EmpNo + " - " + emp.EmpName, emp.EmpId.ToString()));
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
        int empId;
        if (!int.TryParse(ddlEmployee.SelectedValue, out empId))
        {
            gvAttendance.DataSource = null;
            gvAttendance.DataBind();
            return;
        }

        int year = int.Parse(ddlYear.SelectedValue);
        int month = int.Parse(ddlMonth.SelectedValue);

        List<Attendance> list = AttendanceDAL.QueryByMonth(empId, year, month);
        DataTable dt = ToDataTable(list);
        gvAttendance.DataSource = dt;
        gvAttendance.DataBind();
    }

    private DataTable ToDataTable(List<Attendance> list)
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("AttDate", typeof(DateTime));
        dt.Columns.Add("CheckInTime", typeof(DateTime));
        dt.Columns.Add("CheckOutTime", typeof(DateTime));
        dt.Columns.Add("Status", typeof(string));
        dt.Columns.Add("Remark", typeof(string));

        foreach (var item in list)
        {
            DataRow row = dt.NewRow();
            row["AttDate"] = item.AttDate;
            row["CheckInTime"] = item.CheckInTime.HasValue ? (object)item.CheckInTime.Value : DBNull.Value;
            row["CheckOutTime"] = item.CheckOutTime.HasValue ? (object)item.CheckOutTime.Value : DBNull.Value;
            row["Status"] = item.Status;
            row["Remark"] = item.Remark ?? (object)DBNull.Value;
            dt.Rows.Add(row);
        }
        return dt;
    }
}