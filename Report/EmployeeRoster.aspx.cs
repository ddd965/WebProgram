using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Report_EmployeeRoster : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            foreach (var d in DepartmentBLL.GetAll())
                ddlDept.Items.Add(new ListItem(d.DeptName, d.DeptId.ToString()));
            foreach (var p in PositionBLL.GetAll())
                ddlPosition.Items.Add(new ListItem(p.PositionName, p.PositionId.ToString()));
            DoSearch();
        }
    }

    protected string GetDeptName(object id)
    {
        if (id == null || id == DBNull.Value) return "—";
        var d = DepartmentBLL.GetById(Convert.ToInt32(id));
        return d?.DeptName ?? "—";
    }

    protected string GetPosName(object id)
    {
        if (id == null || id == DBNull.Value) return "—";
        var p = PositionBLL.GetById(Convert.ToInt32(id));
        return p?.PositionName ?? "—";
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        gvRoster.PageIndex = 0;
        DoSearch();
    }

    private void DoSearch()
    {
        int? deptId = string.IsNullOrEmpty(ddlDept.SelectedValue) ? (int?)null : int.Parse(ddlDept.SelectedValue);
        int? posId = string.IsNullOrEmpty(ddlPosition.SelectedValue) ? (int?)null : int.Parse(ddlPosition.SelectedValue);
        string status = ddlStatus.SelectedValue;

        var list = EmployeeBLL.Query(null, null, deptId, posId, status);
        gvRoster.DataSource = list;
        gvRoster.DataBind();

        litReportDate.Text = DateTime.Now.ToString("yyyy年MM月dd日");
        litTotal.Text = list.Count + " 人";
    }

    protected void gvRoster_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvRoster.PageIndex = e.NewPageIndex;
        DoSearch();
    }
}
