using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using HRMS.BLL;
using HRMS.DAL;
using HRMS.Common;

public partial class Overtime_OvertimeStat : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindDepartments();
            BindEmployees();
            BindYears();
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

    private void BindEmployees()
    {
        var employees = EmployeeBLL.GetAll();
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
        ddlQuarter.SelectedValue = ((DateTime.Now.Month - 1) / 3 + 1).ToString();
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        int? deptId = null;
        if (!string.IsNullOrEmpty(ddlDepartment.SelectedValue))
        {
            deptId = int.Parse(ddlDepartment.SelectedValue);
        }

        int? empId = null;
        if (!string.IsNullOrEmpty(ddlEmployee.SelectedValue))
        {
            empId = int.Parse(ddlEmployee.SelectedValue);
        }

        int year = int.Parse(ddlYear.SelectedValue);
        int quarter = int.Parse(ddlQuarter.SelectedValue);

        DataTable dt = StatByDeptQuarter(deptId, empId, year, quarter);
        gvOvertimeStat.DataSource = dt;
        gvOvertimeStat.DataBind();
    }

    private DataTable StatByDeptQuarter(int? deptId, int? empId, int year, int quarter)
    {
        int startMonth = (quarter - 1) * 3 + 1;
        int endMonth = quarter * 3;
        DateTime startDate = new DateTime(year, startMonth, 1);
        DateTime endDate = new DateTime(year, endMonth, 1).AddMonths(1);

        StringBuilder sql = new StringBuilder();
        sql.Append(@"SELECT e.EmpId, e.EmpNo, e.EmpName, d.DeptName,
                    COUNT(o.OtId) AS OtCount,
                    COALESCE(SUM(o.OtHours), 0) AS TotalOtHours,
                    COALESCE(SUM(CASE WHEN o.OtType=N'工作日' THEN o.OtHours ELSE 0 END), 0) AS WorkdayHours,
                    COALESCE(SUM(CASE WHEN o.OtType=N'周末' THEN o.OtHours ELSE 0 END), 0) AS WeekendHours,
                    COALESCE(SUM(CASE WHEN o.OtType=N'节假日' THEN o.OtHours ELSE 0 END), 0) AS HolidayHours
                   FROM Employee e
                   LEFT JOIN Department d ON e.DeptId = d.DeptId
                   LEFT JOIN Overtime o ON e.EmpId = o.EmpId
                     AND o.OtDate >= @StartDate AND o.OtDate < @EndDate
                     AND o.Status = N'已批准'
                   WHERE 1=1");

        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date) { Value = startDate });
        parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date) { Value = endDate });

        if (deptId.HasValue)
        {
            sql.Append(" AND e.DeptId = @DeptId");
            parameters.Add(new SqlParameter("@DeptId", deptId.Value));
        }
        if (empId.HasValue)
        {
            sql.Append(" AND e.EmpId = @EmpId");
            parameters.Add(new SqlParameter("@EmpId", empId.Value));
        }

        sql.Append(" GROUP BY e.EmpId, e.EmpNo, e.EmpName, d.DeptName ORDER BY TotalOtHours DESC");

        return DBHelper.ExecuteDataTable(sql.ToString(), parameters.ToArray());
    }
}