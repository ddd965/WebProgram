using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Salary_SalaryStat : HRMS.Common.BasePage
{
    private List<SalaryStatRow> _lastResult;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsInRole("管理员", "部门经理"))
        {
            Response.Redirect("~/Default.aspx");
            return;
        }
        if (!IsPostBack)
        {
            var depts = DepartmentBLL.GetAll();
            foreach (var d in depts)
                ddlDept.Items.Add(new ListItem(d.DeptName, d.DeptId.ToString()));
            var now = DateTime.Now;
            txtMonthFrom.Text = new DateTime(now.Year, 1, 1).ToString("yyyy-MM");
            txtMonthTo.Text = now.ToString("yyyy-MM");
            DoStat();
        }
    }

    protected void btnStat_Click(object sender, EventArgs e)
    {
        gvStat.PageIndex = 0;
        DoStat();
    }

    protected void btnExport_Click(object sender, EventArgs e)
    {
        try
        {
            DoStat();
            if (_lastResult == null || _lastResult.Count == 0)
            {
                lblMsg.CssClass = "text-warning";
                lblMsg.Text = "⚠ 没有可导出的数据。";
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine("分组,员工数,基本工资合计,绩效合计,奖金合计,加班费合计,扣款合计,实发合计,人均实发");
            foreach (var r in _lastResult)
            {
                sb.AppendFormat("\"{0}\",{1},{2},{3},{4},{5},{6},{7},{8}\r\n",
                    r.Group, r.EmpCount, r.TotalBase, r.TotalPerf, r.TotalBonus,
                    r.TotalOtPay, r.TotalDeduct, r.TotalNet, r.AvgNet);
            }
            // 汇总行
            var t = _lastResult.Aggregate(new SalaryStatRow { Group = "合计" }, (acc, x) =>
            {
                acc.TotalBase += x.TotalBase;
                acc.TotalPerf += x.TotalPerf;
                acc.TotalBonus += x.TotalBonus;
                acc.TotalOtPay += x.TotalOtPay;
                acc.TotalDeduct += x.TotalDeduct;
                acc.TotalNet += x.TotalNet;
                acc.EmpCount += x.EmpCount;
                return acc;
            });
            t.AvgNet = t.EmpCount > 0 ? Math.Round(t.TotalNet / t.EmpCount, 2) : 0;
            sb.AppendFormat("\"{0}\",{1},{2},{3},{4},{5},{6},{7},{8}\r\n",
                t.Group, t.EmpCount, t.TotalBase, t.TotalPerf, t.TotalBonus,
                t.TotalOtPay, t.TotalDeduct, t.TotalNet, t.AvgNet);

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            // 加 BOM 让 Excel 正确识别UTF-8
            byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
            byte[] output = bom.Concat(buffer).ToArray();
            Response.Clear();
            Response.ContentType = "text/csv; charset=utf-8";
            Response.AddHeader("Content-Disposition", $"attachment; filename=SalarySummary_{DateTime.Now:yyyyMMdd_HHmm}.csv");
            Response.BinaryWrite(output);
            Response.Flush();
            Response.End();
            WriteLog.Write(CurrentUserName, "导出", $"导出工资汇总 CSV {_lastResult.Count} 行");
        }
        catch (Exception ex)
        {
            lblMsg.CssClass = "text-danger";
            lblMsg.Text = "⚠ 导出失败：" + ex.Message;
        }
    }

    private void DoStat()
    {
        int? deptId = string.IsNullOrEmpty(ddlDept.SelectedValue) ? (int?)null : int.Parse(ddlDept.SelectedValue);
        if (IsInRole("部门经理") && !IsInRole("管理员") && CurrentEmpId.HasValue)
        {
            var me = EmployeeBLL.GetById(CurrentEmpId.Value);
            if (me != null && me.DeptId.HasValue)
            {
                deptId = me.DeptId.Value;
                var li = ddlDept.Items.FindByValue(deptId.Value.ToString());
                if (li != null) { ddlDept.SelectedValue = deptId.Value.ToString(); ddlDept.Enabled = false; }
            }
        }
        string mf = txtMonthFrom.Text.Trim();
        string mt = txtMonthTo.Text.Trim();
        string groupBy = ddlGroup.SelectedValue;

        var allSalaries = SalaryBLL.GetAll();
        // 过滤月份区间
        if (!string.IsNullOrEmpty(mf))
            allSalaries = allSalaries.Where(s => string.Compare(s.SalaryMonth, mf, StringComparison.Ordinal) >= 0).ToList();
        if (!string.IsNullOrEmpty(mt))
            allSalaries = allSalaries.Where(s => string.Compare(s.SalaryMonth, mt, StringComparison.Ordinal) <= 0).ToList();
        // 过滤部门
        if (deptId.HasValue)
        {
            var emps = EmployeeBLL.GetAll();
            allSalaries = allSalaries.Where(s =>
            {
                var emp = emps.FirstOrDefault(x => x.EmpId == s.EmpId);
                return emp != null && emp.DeptId == deptId.Value;
            }).ToList();
        }

        var dict = new Dictionary<string, SalaryStatRow>();
        var empsAll = EmployeeBLL.GetAll();
        foreach (var s in allSalaries)
        {
            string key;
            if (groupBy == "dept")
            {
                var emp = empsAll.FirstOrDefault(x => x.EmpId == s.EmpId);
                if (emp != null && emp.DeptId.HasValue)
                {
                    var d = DepartmentBLL.GetById(emp.DeptId.Value);
                    key = d != null ? d.DeptName : "未分配";
                }
                else key = "未分配部门";
            }
            else
            {
                key = s.SalaryMonth;
            }
            if (!dict.ContainsKey(key))
            {
                dict[key] = new SalaryStatRow { Group = key };
            }
            var r = dict[key];
            r.TotalBase += s.BaseSalary;
            r.TotalPerf += s.Performance;
            r.TotalBonus += s.Bonus;
            r.TotalOtPay += s.OvertimePay;
            r.TotalDeduct += s.Insurance + s.Fund + s.Tax + s.Deduction;
            r.TotalNet += s.NetSalary;
            r.EmpSet.Add(s.EmpId);
        }
        var rows = dict.Values.ToList();
        // 排序：历史模式按月份升序；部门模式按实发合计降序
        if (groupBy == "history")
            rows = rows.OrderBy(x => x.Group).ToList();
        else
            rows = rows.OrderByDescending(x => x.TotalNet).ToList();

        foreach (var r in rows)
        {
            r.EmpCount = r.EmpSet.Count;
            r.AvgNet = r.EmpCount > 0 ? Math.Round(r.TotalNet / r.EmpCount, 2) : 0;
            r.TotalBase = Math.Round(r.TotalBase, 2);
            r.TotalPerf = Math.Round(r.TotalPerf, 2);
            r.TotalBonus = Math.Round(r.TotalBonus, 2);
            r.TotalOtPay = Math.Round(r.TotalOtPay, 2);
            r.TotalDeduct = Math.Round(r.TotalDeduct, 2);
            r.TotalNet = Math.Round(r.TotalNet, 2);
        }
        _lastResult = rows;
        gvStat.DataSource = rows;
        try
        {
            gvStat.DataBind();
            // 添加Footer合计
            if (rows.Count > 0 && gvStat.FooterRow != null)
            {
                gvStat.FooterRow.Cells[0].Text = "合计";
                gvStat.FooterRow.Cells[1].Text = rows.Sum(r => r.EmpCount).ToString();
                gvStat.FooterRow.Cells[2].Text = rows.Sum(r => r.TotalBase).ToString("N2");
                gvStat.FooterRow.Cells[3].Text = rows.Sum(r => r.TotalPerf).ToString("N2");
                gvStat.FooterRow.Cells[4].Text = rows.Sum(r => r.TotalBonus).ToString("N2");
                gvStat.FooterRow.Cells[5].Text = rows.Sum(r => r.TotalOtPay).ToString("N2");
                gvStat.FooterRow.Cells[6].Text = rows.Sum(r => r.TotalDeduct).ToString("N2");
                gvStat.FooterRow.Cells[7].Text = rows.Sum(r => r.TotalNet).ToString("N2");
                gvStat.FooterRow.Cells[7].ForeColor = System.Drawing.Color.FromArgb(0x2b, 0x8c, 0xbe);
                var totalEmp = rows.Sum(r => r.EmpCount);
                var totalNet = rows.Sum(r => r.TotalNet);
                gvStat.FooterRow.Cells[8].Text = totalEmp > 0 ? Math.Round(totalNet / totalEmp, 2).ToString("N2") : "0.00";
            }
        }
        catch { /* 忽略绑定问题 */ }

        // 序列化Chart数据
        var ser = new JavaScriptSerializer();
        var labels = rows.Select(x => x.Group).ToList();
        var nets = rows.Select(x => Math.Round((double)x.TotalNet, 2)).ToList();
        var bases = rows.Select(x => Math.Round((double)x.TotalBase, 2)).ToList();
        hidLabels.Value = ser.Serialize(labels);
        hidNets.Value = ser.Serialize(nets);
        hidBases.Value = ser.Serialize(bases);

        lblMsg.CssClass = "text-info";
        lblMsg.Text = $"{mf ?? "*"} ~ {mt ?? "*"} 共 {allSalaries.Count} 条工资单，{rows.Count} 个{(groupBy == "dept" ? "部门" : "发薪月")}。";
    }

    public class SalaryStatRow
    {
        public string Group { get; set; }
        public int EmpCount { get; set; }
        public decimal TotalBase { get; set; }
        public decimal TotalPerf { get; set; }
        public decimal TotalBonus { get; set; }
        public decimal TotalOtPay { get; set; }
        public decimal TotalDeduct { get; set; }
        public decimal TotalNet { get; set; }
        public decimal AvgNet { get; set; }
        public HashSet<int> EmpSet = new HashSet<int>();
    }

    protected void gvStat_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvStat.PageIndex = e.NewPageIndex;
        DoStat();
    }
}
