using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using HRMS.BLL;

public partial class Report_AttendanceMonthly : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            foreach (var d in DepartmentBLL.GetAll())
                ddlDept.Items.Add(new ListItem(d.DeptName, d.DeptId.ToString()));
            int curY = DateTime.Now.Year;
            for (int y = curY - 3; y <= curY; y++)
                ddlYear.Items.Add(new ListItem(y + "年", y.ToString()));
            for (int m = 1; m <= 12; m++)
                ddlMonth.Items.Add(new ListItem(m + "月", m.ToString()));
            ddlYear.SelectedValue = curY.ToString();
            ddlMonth.SelectedValue = DateTime.Now.Month.ToString();
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
        int y = int.Parse(ddlYear.SelectedValue);
        int m = int.Parse(ddlMonth.SelectedValue);
        var dt = AttendanceBLL.StatByDeptAndMonth(deptId, y, m);
        gvStat.DataSource = dt;
        gvStat.DataBind();

        litTitle.Text = $"{y}年{m}月 月度考勤汇总表";
        litReportTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        litTotal.Text = dt.Rows.Count.ToString();
    }

    protected void gvStat_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvStat.PageIndex = e.NewPageIndex;
        DoStat();
    }
}
