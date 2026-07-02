using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.DataVisualization.Charting;
using HRMS.Model;
using HRMS.BLL;

public partial class LeaveManagement_LeaveStat : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindFilterDropdowns();
            BindData();
        }
    }

    private void BindFilterDropdowns()
    {
        ddlDepartmentFilter.DataSource = DepartmentBLL.GetAll();
        ddlDepartmentFilter.DataTextField = "DeptName";
        ddlDepartmentFilter.DataValueField = "DeptId";
        ddlDepartmentFilter.DataBind();
        ddlDepartmentFilter.Items.Insert(0, new ListItem("所有部门", ""));

        ddlLeaveTypeFilter.DataSource = LeaveTypeBLL.GetAll();
        ddlLeaveTypeFilter.DataTextField = "TypeName";
        ddlLeaveTypeFilter.DataValueField = "LeaveTypeId";
        ddlLeaveTypeFilter.DataBind();
        ddlLeaveTypeFilter.Items.Insert(0, new ListItem("所有类型", ""));

        int currentYear = DateTime.Now.Year;
        for (int i = currentYear - 5; i <= currentYear + 5; i++)
        {
            txtYearFilter.Items.Add(new ListItem(i.ToString(), i.ToString()));
        }
        txtYearFilter.SelectedValue = currentYear.ToString();
    }

    private void BindData()
    {
        int? empId = null;
        int? leaveTypeId = string.IsNullOrEmpty(ddlLeaveTypeFilter.SelectedValue) ? (int?)null : Convert.ToInt32(ddlLeaveTypeFilter.SelectedValue);
        DateTime? startDate = null;
        DateTime? endDate = null;
        string status = null;

        if (!string.IsNullOrEmpty(txtYearFilter.SelectedValue))
        {
            int selectedYear = Convert.ToInt32(txtYearFilter.SelectedValue);
            startDate = new DateTime(selectedYear, 1, 1);
            endDate = new DateTime(selectedYear, 12, 31);
        }

        List<LeaveRecord> leaveRecords = LeaveRecordBLL.Query(empId, leaveTypeId, startDate, endDate, status);

        if (!string.IsNullOrEmpty(ddlDepartmentFilter.SelectedValue))
        {
            int selectedDeptId = Convert.ToInt32(ddlDepartmentFilter.SelectedValue);
            var deptEmpIds = new HashSet<int>(EmployeeBLL.GetAll()
                .Where(emp => emp.DeptId == selectedDeptId)
                .Select(emp => emp.EmpId));
            leaveRecords = leaveRecords.Where(lr => deptEmpIds.Contains(lr.EmpId)).ToList();
        }

        gvLeaveStats.DataSource = leaveRecords;
        gvLeaveStats.DataBind();

        var chartData = leaveRecords.GroupBy(lr => LeaveTypeBLL.GetById(lr.LeaveTypeId)?.TypeName ?? "未知类型")
                                   .Select(g => new { LeaveType = g.Key, TotalDays = g.Sum(lr => lr.LeaveDays) })
                                   .ToList();

        Chart1.Series["Series1"].Points.Clear();
        Chart1.Series["Series1"].ChartType = SeriesChartType.Pie;
        foreach (var data in chartData)
        {
            Chart1.Series["Series1"].Points.AddXY(data.LeaveType, data.TotalDays);
        }
        Chart1.DataBind();
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        BindData();
    }
}
