using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Overtime_OvertimeStat : HRMS.Common.BasePage
{
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
            int curY = DateTime.Now.Year;
            for (int y = curY - 3; y <= curY; y++)
                ddlYear.Items.Add(new ListItem(y + "年", y.ToString()));
            ddlYear.SelectedValue = curY.ToString();
            // 默认本月
            ddlPeriod.SelectedValue = DateTime.Now.Month.ToString();
            DoStat();
        }
    }

    protected void btnStat_Click(object sender, EventArgs e)
    {
        gvStat.PageIndex = 0;
        DoStat();
    }

    protected void btnWriteBack_Click(object sender, EventArgs e)
    {
        try
        {
            int? deptId = string.IsNullOrEmpty(ddlDept.SelectedValue) ? (int?)null : int.Parse(ddlDept.SelectedValue);
            if (IsInRole("部门经理") && !IsInRole("管理员") && CurrentEmpId.HasValue)
            {
                var me = EmployeeBLL.GetById(CurrentEmpId.Value);
                if (me != null && me.DeptId.HasValue) deptId = me.DeptId.Value;
            }
            int year = int.Parse(ddlYear.SelectedValue);
            int period = int.Parse(ddlPeriod.SelectedValue);
            string range = ddlRange.SelectedValue;
            string salaryMonth;
            if (range == "quarter")
            {
                // 取该季度最后一个月作为回写目标月（用户可选择任意月）
                int m = (period - 1) * 3 + 2; // 季度中间月
                salaryMonth = $"{year}-{m:00}";
                lblMsg.CssClass = "text-warning";
                lblMsg.Text = "⚠ 季度统计将回写到该季度的中间月工资单，请确认月份正确。继续操作：";
            }
            else
            {
                salaryMonth = $"{year}-{period:00}";
            }
            int n = OvertimeBLL.WriteBackOvertimePay(salaryMonth, deptId);
            WriteLog.Write(CurrentUserName, "修改", $"加班汇总回写工资 {salaryMonth}，更新{n}条");
            lblMsg.CssClass = "text-success";
            lblMsg.Text += $"✅ 已完成加班费回写工资：薪资月份 {salaryMonth}，共更新 {n} 条工资记录。";
            DoStat();
        }
        catch (Exception ex)
        {
            lblMsg.CssClass = "text-danger";
            lblMsg.Text = "⚠ 回写失败：" + ex.Message;
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
        int year = int.Parse(ddlYear.SelectedValue);
        int period = int.Parse(ddlPeriod.SelectedValue);
        string range = ddlRange.SelectedValue;

        DateTime s, ed;
        if (range == "month")
        {
            s = new DateTime(year, period, 1);
            ed = s.AddMonths(1).AddDays(-1);
        }
        else // quarter
        {
            int startMonth = (period - 1) * 3 + 1;
            s = new DateTime(year, startMonth, 1);
            ed = s.AddMonths(3).AddDays(-1);
            if (period > 4) period = 4;
        }

        var allOt = OvertimeBLL.Query(null, s, ed.AddDays(1), null, "已批准");
        if (deptId.HasValue)
        {
            allOt = allOt.Where(o =>
            {
                var emp = EmployeeBLL.GetById(o.EmpId);
                return emp != null && emp.DeptId == deptId.Value;
            }).ToList();
        }

        var groups = new Dictionary<string, OtStatRow>();
        var deptGroups = new Dictionary<string, OtDeptAcc>();
        var empSalaryBaseCache = new Dictionary<int, decimal>();
        var emps = EmployeeBLL.GetAll();

        foreach (var o in allOt)
        {
            var emp = emps.FirstOrDefault(e2 => e2.EmpId == o.EmpId);
            if (emp == null) continue;
            string key = $"{emp.EmpName} ({emp.EmpNo})";
            if (!groups.ContainsKey(key)) groups[key] = new OtStatRow { GroupKey = key };
            groups[key].记录数++;
            groups[key].总时长 += o.OtHours;
            switch (o.OtType)
            {
                case "工作日": groups[key].工作日 += o.OtHours; break;
                case "周末": groups[key].周末 += o.OtHours; break;
                case "节假日": groups[key].节假日 += o.OtHours; break;
            }
            if (!empSalaryBaseCache.ContainsKey(emp.EmpId))
            {
                var sals = SalaryBLL.QueryByEmpAndMonth(emp.EmpId, null, null);
                empSalaryBaseCache[emp.EmpId] = sals.Count > 0 ? sals[sals.Count - 1].BaseSalary : 5000m;
            }
            decimal baseS = empSalaryBaseCache[emp.EmpId];
            decimal pay = OvertimeBLL.CalculateOvertimePay(baseS, o.OtHours, o.OtType);
            groups[key].估算加班费 += pay;

            // 部门汇总
            string deptName = "未分配";
            if (emp.DeptId.HasValue)
            {
                var d = DepartmentBLL.GetById(emp.DeptId.Value);
                if (d != null) deptName = d.DeptName;
            }
            if (!deptGroups.ContainsKey(deptName)) deptGroups[deptName] = new OtDeptAcc { DeptName = deptName };
            deptGroups[deptName].TotalHours += o.OtHours;
            deptGroups[deptName].TotalPay += pay;
            deptGroups[deptName].EmpSet.Add(emp.EmpId);
        }

        gvStat.DataSource = groups.Values.ToList();
        gvStat.DataBind();

        // 绑定部门汇总
        var deptDt = new DataTable();
        deptDt.Columns.Add("Period", typeof(string));
        deptDt.Columns.Add("DeptName", typeof(string));
        deptDt.Columns.Add("EmpCount", typeof(int));
        deptDt.Columns.Add("TotalHours", typeof(decimal));
        deptDt.Columns.Add("TotalPay", typeof(decimal));
        string periodLabel = range == "month" ? $"{year}年{period}月" : $"{year}年Q{period}";
        var labels = new List<string>();
        var hours = new List<double>();
        var pays = new List<double>();
        foreach (var dg in deptGroups.Values)
        {
            deptDt.Rows.Add(periodLabel, dg.DeptName, dg.EmpSet.Count, Math.Round(dg.TotalHours, 1), Math.Round(dg.TotalPay, 2));
            labels.Add(dg.DeptName);
            hours.Add(Math.Round((double)dg.TotalHours, 2));
            pays.Add(Math.Round((double)dg.TotalPay, 2));
        }
        gvDept.DataSource = deptDt;
        gvDept.DataBind();

        // 序列化Chart数据
        var ser = new JavaScriptSerializer();
        hidLabels.Value = ser.Serialize(labels);
        hidHours.Value = ser.Serialize(hours);
        hidPay.Value = ser.Serialize(pays);

        lblMsg.CssClass = "text-info";
        lblMsg.Text = $"{s:yyyy-MM-dd} ~ {ed:yyyy-MM-dd} 共 {allOt.Count} 条已批准加班记录，涉及 {groups.Count} 位员工，{deptGroups.Count} 个部门。";
    }

    public class OtStatRow
    {
        public string GroupKey { get; set; }
        public decimal 工作日 { get; set; }
        public decimal 周末 { get; set; }
        public decimal 节假日 { get; set; }
        public decimal 总时长 { get; set; }
        public int 记录数 { get; set; }
        public decimal 估算加班费 { get; set; }
    }

    public class OtDeptAcc
    {
        public string DeptName;
        public decimal TotalHours;
        public decimal TotalPay;
        public HashSet<int> EmpSet = new HashSet<int>();
    }

    protected void gvStat_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvStat.PageIndex = e.NewPageIndex;
        DoStat();
    }
}
