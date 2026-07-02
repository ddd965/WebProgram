using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Employee_EmployeeList : HRMS.Common.BasePage
{
    private List<Employee> _cacheList;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            LoadDropdowns();
            // ==== 修复：若 URL 传来 deptId（来自部门管理"查看该部门员工"按钮），自动预筛选 ====
            string deptIdQs = Request.QueryString["deptId"];
            if (!string.IsNullOrEmpty(deptIdQs))
            {
                var li = ddlDept.Items.FindByValue(deptIdQs);
                if (li != null) ddlDept.SelectedValue = deptIdQs;
            }
            BindData();
        }
    }

    private void LoadDropdowns()
    {
        var depts = DepartmentBLL.GetAll();
        foreach (var d in depts)
            ddlDept.Items.Add(new ListItem(d.DeptName, d.DeptId.ToString()));

        var positions = PositionBLL.GetAll();
        foreach (var p in positions)
            ddlPosition.Items.Add(new ListItem(p.PositionName, p.PositionId.ToString()));
    }

    private void BindData()
    {
        string empNo = txtEmpNo.Text.Trim();
        string empName = txtEmpName.Text.Trim();
        int? deptId = string.IsNullOrEmpty(ddlDept.SelectedValue) ? (int?)null : int.Parse(ddlDept.SelectedValue);
        int? positionId = string.IsNullOrEmpty(ddlPosition.SelectedValue) ? (int?)null : int.Parse(ddlPosition.SelectedValue);
        string status = ddlStatus.SelectedValue;

        _cacheList = EmployeeBLL.Query(empNo, empName, deptId, positionId, status);

        gvEmployee.DataSource = _cacheList;
        gvEmployee.DataBind();

        if (_cacheList.Count == 0)
            lblMsg.Text = "没有符合条件的记录。";
        else
            lblMsg.Text = $"共 {_cacheList.Count} 条记录。";
    }

    protected string GetDeptName(object deptId)
    {
        if (deptId == null || deptId == DBNull.Value) return "—";
        var d = DepartmentBLL.GetById(Convert.ToInt32(deptId));
        return d?.DeptName ?? "—";
    }

    protected string GetPositionName(object positionId)
    {
        if (positionId == null || positionId == DBNull.Value) return "—";
        var p = PositionBLL.GetById(Convert.ToInt32(positionId));
        return p?.PositionName ?? "—";
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        gvEmployee.PageIndex = 0;
        BindData();
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        txtEmpNo.Text = "";
        txtEmpName.Text = "";
        ddlDept.SelectedIndex = 0;
        ddlPosition.SelectedIndex = 0;
        ddlStatus.SelectedIndex = 0;
        gvEmployee.PageIndex = 0;
        BindData();
    }

    protected void gvEmployee_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvEmployee.PageIndex = e.NewPageIndex;
        BindData();
    }

    protected void gvEmployee_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "DeleteRow")
        {
            int empId = Convert.ToInt32(e.CommandArgument);
            EmployeeBLL.Delete(empId);
            WriteLog.Write(CurrentUserName, "删除", $"删除员工 ID={empId}");
            BindData();
            lblMsg.Text = "员工已删除。";
        }
    }
}
