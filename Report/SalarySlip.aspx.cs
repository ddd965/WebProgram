using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

            // 员工池 = 非离职（含在职/试用期/实习期），避免试用期员工申请了加班却在报表里找不到
            //  ⚠ lambda 参数名用 emp 不用 e！因为 Page_Load 方法签名已有 EventArgs e（CS0136 变量重名）
            var emps = EmployeeBLL.Query(null, null, null, null, null).Where(emp => emp.Status != "离职").ToList();
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
        var salaryByEmp = new Dictionary<int, Salary>();
        foreach (var s in salaries)
            salaryByEmp[s.EmpId] = s;
        // ---- 兜底：如果该月完全没有任何工资记录（如：还没录2026-06），按员工生成演示工资条 ----
        // ---- 避免报表打印"共0张"，让用户直接看到打印效果 ----
        bool emptyMonth = salaries.Count == 0;

        // 员工池 = 非离职（含在职/试用期/实习期），筛选条件从 deptId/empId 再做二次裁剪
        List<Employee> empPool = EmployeeBLL.Query(null, null, null, null, null).Where(emp => emp.Status != "离职").ToList();
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
        int fakeId = 1;
        // 遍历员工池：有工资用真实，没工资用演示数据
        foreach (var emp in empPool)
        {
            if (deptId.HasValue && emp.DeptId != deptId.Value) continue;
            if (empId.HasValue && emp.EmpId != empId.Value) continue;

            Salary s;
            string remark;
            if (salaryByEmp.TryGetValue(emp.EmpId, out s))
            {
                remark = s.Remark;
            }
            else if (emptyMonth)
            {
                // 没该月工资时，按职位给演示薪资（基于5月真实数据范围）
                decimal bs = 10000m;
                if (emp.PositionId.HasValue)
                {
                    switch (emp.PositionId.Value)
                    {
                        case 1: bs = 20000m; break;    // 总经理
                        case 2: bs = 15000m; break;    // 部门经理
                        case 3: bs = 12000m; break;    // 高级工程师
                        case 4: bs = 11000m; break;    // 市场经理
                        case 5: bs = 10000m; break;    // 普通工程师
                        case 6: bs = 9000m; break;     // 专员/试用期
                        case 9: bs = 7500m; break;     // 助理/实习生
                        default: bs = 8500m; break;
                    }
                }
                decimal perf = Math.Round(bs * 0.20m, 0);
                decimal bonus = Math.Round(bs * 0.08m, 0);
                decimal otPay = 0m;
                // 查当月该员工加班小时（已批准）估算加班费
                try
                {
                    int y = int.Parse(month.Substring(0, 4));
                    int m = int.Parse(month.Substring(5, 2));
                    DateTime f = new DateTime(y, m, 1);
                    DateTime t = f.AddMonths(1).AddDays(-1);
                    var ots = OvertimeBLL.Query(emp.EmpId, f, t.AddDays(1), null, "已批准");
                    if (ots != null && ots.Count > 0)
                    {
                        decimal totalH = 0m;
                        foreach (var o in ots) totalH += o.OtHours;
                        otPay = OvertimeBLL.CalculateOvertimePay(bs, totalH, "工作日");
                    }
                }
                catch { }
                decimal ins = Math.Round(bs * 0.11m, 0);
                decimal fund = Math.Round(bs * 0.07m, 0);
                decimal tax = Math.Round(Math.Max((bs + perf + bonus + otPay - ins - fund - 5000m), 0m) * 0.03m, 0);
                decimal ded = 0m;
                decimal net = bs + perf + bonus + otPay - ins - fund - tax - ded;
                if (net < 0) net = 0;
                s = new Salary
                {
                    SalaryId = -fakeId++,
                    EmpId = emp.EmpId,
                    SalaryMonth = month,
                    BaseSalary = bs,
                    Performance = perf,
                    Bonus = bonus,
                    OvertimePay = otPay,
                    Insurance = ins,
                    Fund = fund,
                    Tax = tax,
                    Deduction = ded,
                    NetSalary = Math.Round(net, 2),
                    PayDate = null,
                    Remark = "（演示数据，未入库：请在 工资管理→工资录入 中创建真实" + month + "工资单）"
                };
                remark = s.Remark;
            }
            else
            {
                continue;   // 该月有部分工资记录，没记录的员工不显示
            }

            string deptName = emp.DeptId.HasValue
                ? (DepartmentBLL.GetById(emp.DeptId.Value)?.DeptName ?? "—") : "—";
            dt.Rows.Add(s.SalaryId, s.SalaryMonth, emp.EmpNo, emp.EmpName, deptName,
                s.BaseSalary, s.Performance, s.Bonus, s.OvertimePay,
                s.Insurance, s.Fund, s.Tax, s.Deduction, s.NetSalary,
                s.PayDate.HasValue ? s.PayDate.Value.ToString("yyyy-MM-dd") : "—",
                remark);
            count++;
        }

        rptSlips.DataSource = dt;
        rptSlips.DataBind();

        lblMsg.CssClass = "text-info";
        if (emptyMonth && count > 0)
            lblMsg.Text = month + " 暂无真实工资记录，已按员工薪资标准生成 " + count + " 张演示工资条（去 工资录入 页面录入后将显示真实数据）。";
        else
            lblMsg.Text = month + " 共生成 " + count + " 张工资条。";
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
