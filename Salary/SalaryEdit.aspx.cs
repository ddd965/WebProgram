using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Salary_SalaryEdit : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsInRole("管理员", "部门经理"))
        {
            // 允许普通员工访问页面，但重定向到工资列表看自己的记录
            Response.Redirect("SalaryList.aspx");
            return;
        }
        if (!IsPostBack)
        {
            var emps = EmployeeBLL.Query(null, null, null, null, "在职");
            ddlEmployee.Items.Clear();
            foreach (var emp in emps)
                ddlEmployee.Items.Add(new ListItem($"{emp.EmpName} ({emp.EmpNo})", emp.EmpId.ToString()));

            if (IsInRole("部门经理") && !IsInRole("管理员") && CurrentEmpId.HasValue)
            {
                var me = EmployeeBLL.GetById(CurrentEmpId.Value);
                if (me != null && me.DeptId.HasValue)
                {
                    int dept = me.DeptId.Value;
                    for (int i = ddlEmployee.Items.Count - 1; i >= 0; i--)
                    {
                        int eid = int.Parse(ddlEmployee.Items[i].Value);
                        var ee = EmployeeBLL.GetById(eid);
                        if (ee == null || ee.DeptId != dept) ddlEmployee.Items.RemoveAt(i);
                    }
                }
            }
            txtSalaryMonth.Text = DateTime.Now.ToString("yyyy-MM");
        }
    }

    protected void ddlEmployee_SelectedIndexChanged(object sender, EventArgs e)
    {
        int empId;
        if (int.TryParse(ddlEmployee.SelectedValue, out empId))
        {
            var list = SalaryBLL.QueryByEmpAndMonth(empId, null, null);
            if (list.Count > 0)
            {
                var s = list[0];
                txtBase.Text = s.BaseSalary.ToString("0.##");
                txtPerf.Text = s.Performance.ToString("0.##");
                txtBonus.Text = s.Bonus.ToString("0.##");
                txtIns.Text = s.Insurance.ToString("0.##");
                txtFund.Text = s.Fund.ToString("0.##");
                txtTax.Text = s.Tax.ToString("0.##");
                txtDed.Text = s.Deduction.ToString("0.##");
                txtOtPay.Text = s.OvertimePay.ToString("0.##");
            }
            CalcNet();
        }
    }

    protected void btnCalc_Click(object sender, EventArgs e) { CalcNet(); }

    private void CalcNet()
    {
        decimal b = decimal.Parse(string.IsNullOrEmpty(txtBase.Text) ? "0" : txtBase.Text);
        decimal p = decimal.Parse(string.IsNullOrEmpty(txtPerf.Text) ? "0" : txtPerf.Text);
        decimal bo = decimal.Parse(string.IsNullOrEmpty(txtBonus.Text) ? "0" : txtBonus.Text);
        decimal op = decimal.Parse(string.IsNullOrEmpty(txtOtPay.Text) ? "0" : txtOtPay.Text);
        decimal ins = decimal.Parse(string.IsNullOrEmpty(txtIns.Text) ? "0" : txtIns.Text);
        decimal fu = decimal.Parse(string.IsNullOrEmpty(txtFund.Text) ? "0" : txtFund.Text);
        decimal tx = decimal.Parse(string.IsNullOrEmpty(txtTax.Text) ? "0" : txtTax.Text);
        decimal de = decimal.Parse(string.IsNullOrEmpty(txtDed.Text) ? "0" : txtDed.Text);
        decimal net = b + p + bo + op - ins - fu - tx - de;
        if (net < 0) net = 0;
        txtNet.Text = net.ToString("0.00");
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) { lblMsg.Text = "⚠ 请完善必填项。"; lblMsg.CssClass = "text-danger"; return; }
        try
        {
            lblMsg.CssClass = "text-info";
            int empId = int.Parse(ddlEmployee.SelectedValue);
            string month = (txtSalaryMonth.Text ?? "").Trim();
            CalcNet();
            var s = new Salary
            {
                EmpId = empId,
                SalaryMonth = month,
                BaseSalary = decimal.Parse(txtBase.Text),
                Performance = decimal.Parse(txtPerf.Text),
                Bonus = decimal.Parse(txtBonus.Text),
                OvertimePay = decimal.Parse(txtOtPay.Text),
                Insurance = decimal.Parse(txtIns.Text),
                Fund = decimal.Parse(txtFund.Text),
                Tax = decimal.Parse(txtTax.Text),
                Deduction = decimal.Parse(txtDed.Text),
                NetSalary = decimal.Parse(txtNet.Text),
                PayDate = string.IsNullOrEmpty(txtPayDate.Text) ? (DateTime?)null : DateTime.Parse(txtPayDate.Text),
                Remark = (txtRemark.Text ?? "").Trim()
            };
            var exists = SalaryBLL.QueryByEmpAndMonth(empId, month, month);
            if (exists.Count > 0)
            {
                s.SalaryId = exists[0].SalaryId;
                SalaryBLL.Update(s);
                WriteLog.Write(CurrentUserName, "修改", $"更新工资记录：{month} 员工ID={empId} 实发{s.NetSalary:C}");
                lblMsg.Text = $"已覆盖更新 {month} 工资记录，实发 ¥{s.NetSalary:N2}。";
            }
            else
            {
                SalaryBLL.Insert(s);
                WriteLog.Write(CurrentUserName, "新增", $"新增工资记录：{month} 员工ID={empId} 实发{s.NetSalary:C}");
                lblMsg.Text = $"已保存 {month} 工资记录，实发 ¥{s.NetSalary:N2}。";
            }
            lblMsg.CssClass = "text-success";
        }
        catch (Exception ex)
        {
            lblMsg.CssClass = "text-danger";
            lblMsg.Text = "⚠ 保存失败：" + ex.Message;
        }
    }
}
