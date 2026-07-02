using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Report_SalarySlip : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsInRole("管理员", "部门经理"))
        {
            // 普通员工只看自己的工资条
        }
        if (!IsPostBack)
        {
            foreach (var d in DepartmentBLL.GetAll())
                ddlDept.Items.Add(new ListItem(d.DeptName, d.DeptId.ToString()));

            var emps = EmployeeBLL.Query(null, null, null, null, "在职");
            foreach (var emp in emps)
                ddlEmp.Items.Add(new ListItem($"{emp.EmpName} ({emp.EmpNo})", emp.EmpId.ToString()));

            txtMonth.Text = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");

            // 普通用户：只看自己
            if (!IsInRole("管理员", "部门经理") && CurrentEmpId.HasValue)
            {
                var li = ddlEmp.Items.FindByValue(CurrentEmpId.Value.ToString());
                if (li != null) { ddlEmp.SelectedValue = CurrentEmpId.Value.ToString(); }
                ddlDept.Enabled = false;
                ddlEmp.Enabled = false;
            }

            if (!string.IsNullOrEmpty(txtMonth.Text))
            {
                DoGen();
            }
        }
    }

    protected void btnGen_Click(object sender, EventArgs e)
    {
        DoGen();
    }

    private void DoGen()
    {
        string month = txtMonth.Text.Trim();
        if (string.IsNullOrEmpty(month))
        {
            lblMsg.Text = "请输入薪资月份！";
            lblMsg.CssClass = "text-danger";
            return;
        }
        int? deptId = string.IsNullOrEmpty(ddlDept.SelectedValue) ? (int?)null : int.Parse(ddlDept.SelectedValue);
        int? empId = string.IsNullOrEmpty(ddlEmp.SelectedValue) ? (int?)null : int.Parse(ddlEmp.SelectedValue);

        // 部门经理只能看自己的部门
        if (IsInRole("部门经理") && !IsInRole("管理员") && CurrentEmpId.HasValue)
        {
            var me = EmployeeBLL.GetById(CurrentEmpId.Value);
            if (me != null && me.DeptId.HasValue)
            {
                deptId = me.DeptId.Value;
            }
        }
        // 普通员工只能看自己
        if (!IsInRole("管理员", "部门经理") && CurrentEmpId.HasValue)
        {
            empId = CurrentEmpId.Value;
        }

        // 取该月所有工资 + join 员工
        var salaries = SalaryBLL.GetAll().FindAll(s => s.SalaryMonth == month);
        var dt = new DataTable();
        dt.Columns.Add("SalaryId", typeof(int));
        dt.Columns.Add("SalaryMonth", typeof(string));
        dt.Columns.Add("EmpNo", typeof(string));
        dt.Columns.Add("EmpName", typeof(string));
        dt.Columns.Add("DeptName", typeof(string));
        dt.Columns.Add("BaseSalary", typeof(decimal));
        dt.Columns.Add("Performance", typeof(decimal));
        dt.Columns.Add("Bonus", typeof(decimal));
        dt.Columns.Add("OvertimePay", typeof(decimal));
        dt.Columns.Add("Insurance", typeof(decimal));
        dt.Columns.Add("Fund", typeof(decimal));
        dt.Columns.Add("Tax", typeof(decimal));
        dt.Columns.Add("Deduction", typeof(decimal));
        dt.Columns.Add("NetSalary", typeof(decimal));
        dt.Columns.Add("PayDateStr", typeof(string));
        dt.Columns.Add("Remark", typeof(string));

        int count = 0;
        foreach (var s in salaries)
        {
            var emp = EmployeeBLL.GetById(s.EmpId);
            if (emp == null) continue;
            if (deptId.HasValue && emp.DeptId != deptId.Value) continue;
            if (empId.HasValue && emp.EmpId != empId.Value) continue;
            string deptName = emp.DeptId.HasValue
                ? (DepartmentBLL.GetById(emp.DeptId.Value)?.DeptName ?? "—") : "—";
            dt.Rows.Add(s.SalaryId, s.SalaryMonth, emp.EmpNo, emp.EmpName, deptName,
                s.BaseSalary, s.Performance, s.Bonus, s.OvertimePay,
                s.Insurance, s.Fund, s.Tax, s.Deduction, s.NetSalary,
                s.PayDate.HasValue ? s.PayDate.Value.ToString("yyyy-MM-dd") : "—",
                s.Remark);
            count++;
        }

        rptSlips.DataSource = dt;
        rptSlips.DataBind();

        lblMsg.CssClass = "text-info";
        lblMsg.Text = $"{month} 共生成 {count} 张工资条。";
    }

    protected void rptSlips_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
        {
            var row = (DataRowView)e.Item.DataItem;
            var lit = (Literal)e.Item.FindControl("litRemark");
            lit.Text = string.IsNullOrEmpty(row["Remark"].ToString()) ? "无" : row["Remark"].ToString();
        }
    }
}
