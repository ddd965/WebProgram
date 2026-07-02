using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Salary_SalaryList : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            var emps = EmployeeBLL.Query(null, null, null, null, null);
            foreach (var emp in emps)
                ddlQEmp.Items.Add(new ListItem($"{emp.EmpName} ({emp.EmpNo})", emp.EmpId.ToString()));

            if (!IsInRole("管理员", "部门经理") && CurrentEmpId.HasValue)
            {
                var li = ddlQEmp.Items.FindByValue(CurrentEmpId.Value.ToString());
                if (li != null) { ddlQEmp.SelectedValue = CurrentEmpId.Value.ToString(); }
                ddlQEmp.Enabled = false;
            }
            BindData();
        }
    }

    protected string GetEmpName(object empId)
    {
        if (empId == null || empId == DBNull.Value) return "—";
        var emp = EmployeeBLL.GetById(Convert.ToInt32(empId));
        return emp != null ? $"{emp.EmpName} ({emp.EmpNo})" : "—";
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        gvSalary.PageIndex = 0; BindData();
    }

    private void BindData()
    {
        string from = (txtQFrom.Text ?? "").Trim();
        string to = (txtQTo.Text ?? "").Trim();
        List<Salary> list;

        if (IsInRole("部门经理") && !IsInRole("管理员") && CurrentEmpId.HasValue)
        {
            var me = EmployeeBLL.GetById(CurrentEmpId.Value);
            var emps = EmployeeBLL.GetAll();
            IEnumerable<Salary> q = SalaryBLL.GetAll();
            if (me != null && me.DeptId.HasValue)
            {
                int dept = me.DeptId.Value;
                q = q.Where(s =>
                {
                    var ee = emps.FirstOrDefault(x => x.EmpId == s.EmpId);
                    return ee != null && ee.DeptId == dept;
                });
            }
            if (!string.IsNullOrEmpty(from)) q = q.Where(x => string.Compare(x.SalaryMonth, from, StringComparison.Ordinal) >= 0);
            if (!string.IsNullOrEmpty(to)) q = q.Where(x => string.Compare(x.SalaryMonth, to, StringComparison.Ordinal) <= 0);
            if (!string.IsNullOrEmpty(ddlQEmp.SelectedValue))
                q = q.Where(x => x.EmpId == int.Parse(ddlQEmp.SelectedValue));
            list = q.OrderByDescending(x => x.SalaryMonth).ThenBy(x => x.EmpId).ToList();
        }
        else if (string.IsNullOrEmpty(ddlQEmp.SelectedValue))
        {
            if (!IsInRole("管理员", "部门经理") && CurrentEmpId.HasValue)
            {
                list = SalaryBLL.QueryByEmpAndMonth(CurrentEmpId.Value, from, to);
            }
            else
            {
                var all = SalaryBLL.GetAll();
                IEnumerable<Salary> q = all;
                if (!string.IsNullOrEmpty(from)) q = q.Where(x => string.Compare(x.SalaryMonth, from, StringComparison.Ordinal) >= 0);
                if (!string.IsNullOrEmpty(to)) q = q.Where(x => string.Compare(x.SalaryMonth, to, StringComparison.Ordinal) <= 0);
                list = q.OrderByDescending(x => x.SalaryMonth).ThenBy(x => x.EmpId).ToList();
            }
        }
        else
        {
            int empId = int.Parse(ddlQEmp.SelectedValue);
            list = SalaryBLL.QueryByEmpAndMonth(empId, from, to);
        }

        gvSalary.DataSource = list;
        gvSalary.DataBind();
        lblMsg.CssClass = "text-info";
        lblMsg.Text = $"共 {list.Count} 条工资记录。";
    }

    protected void gvSalary_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvSalary.PageIndex = e.NewPageIndex; BindData();
    }

    protected void gvSalary_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "DeleteRow")
        {
            int id = Convert.ToInt32(e.CommandArgument);
            SalaryBLL.Delete(id);
            WriteLog.Write(CurrentUserName, "删除", $"删除工资记录 ID={id}");
            lblMsg.Text = "已删除。";
            BindData();
        }
    }
}
