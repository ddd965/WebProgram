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
            int fromY = Math.Min(2026, curY);
            int toY = Math.Max(2026, curY) + 1;
            for (int y = fromY; y <= toY; y++)
                ddlYear.Items.Add(new ListItem(y + "年", y.ToString()));

            // ============================================================
            //  默认月 = 上个月（今天是 7/3 → 6 月，含完整 22 工作日+加班数据）
            //  本月刚开始，数据不全，用户看统计 99% 是看上个月完整数据（HR 惯例）
            // ============================================================
            var def = DateTime.Today.AddMonths(-1);
            ddlYear.SelectedValue = def.Year.ToString();

            RebindPeriodOptions();
            if (ddlRange.SelectedValue == "month")
                ddlPeriod.SelectedValue = def.Month.ToString();
            else
            {
                int q = (def.Month - 1) / 3 + 1;
                ddlPeriod.SelectedValue = q.ToString();
            }

            UpdatePeriodLabel();
            DoStat();
        }
    }

    /// <summary>
    /// 按当前 ddlRange 重新绑定 ddlPeriod 的可选项：
    ///  - 月度 → 1月~12月
    ///  - 季度 → Q1~Q4
    /// </summary>
    private void RebindPeriodOptions()
    {
        ddlPeriod.Items.Clear();
        if (ddlRange.SelectedValue == "quarter")
        {
            for (int q = 1; q <= 4; q++)
                ddlPeriod.Items.Add(new ListItem("第" + q + "季度 (Q" + q + ")", q.ToString()));
        }
        else
        {
            for (int m = 1; m <= 12; m++)
                ddlPeriod.Items.Add(new ListItem(m + "月", m.ToString()));
        }
    }

    private void UpdatePeriodLabel()
    {
        if (ddlRange.SelectedValue == "quarter")
            lblPeriodLabel.InnerText = "季度（1~4）";
        else
            lblPeriodLabel.InnerText = "月份";
    }

    protected void ddlRange_SelectedIndexChanged(object sender, EventArgs e)
    {
        RebindPeriodOptions();
        ddlPeriod.SelectedIndex = 0;
        UpdatePeriodLabel();
        DoStat();
    }

    protected void btnStat_Click(object sender, EventArgs e)
    {
        UpdatePeriodLabel();
        DoStat();
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
        int rawPeriod;
        if (!int.TryParse(ddlPeriod.SelectedValue, out rawPeriod)) rawPeriod = 1;
        string range = ddlRange.SelectedValue;

        DateTime s, ed;
        string periodLabel;
        if (range == "month")
        {
            if (rawPeriod < 1) rawPeriod = 1;
            if (rawPeriod > 12) rawPeriod = 12;
            s = new DateTime(year, rawPeriod, 1);
            ed = s.AddMonths(1).AddDays(-1);
            periodLabel = year + "年" + rawPeriod + "月";
        }
        else // quarter
        {
            int quarter = (rawPeriod >= 1 && rawPeriod <= 4) ? rawPeriod : 1;
            int startMonth = (quarter - 1) * 3 + 1;
            s = new DateTime(year, startMonth, 1);
            ed = s.AddMonths(3).AddDays(-1);
            periodLabel = year + "年Q" + quarter;
        }

        // 有效员工 = 非离职（含在职/试用期/实习期/休长假等）
        var emps = EmployeeBLL.GetAll().Where(e => e.Status != "离职");
        if (deptId.HasValue) emps = emps.Where(e => e.DeptId == deptId.Value);
        var empList = emps.ToList();
        var empDict = new Dictionary<int, Employee>();
        foreach (var emp in empList)
            if (!empDict.ContainsKey(emp.EmpId)) empDict[emp.EmpId] = emp;

        // ========== 查询已批准加班（不再吞异常 → 有错误直接红字暴露） ==========
        List<Overtime> allOt;
        try
        {
            allOt = OvertimeBLL.Query(null, s, ed.AddDays(1), null, "已批准") ?? new List<Overtime>();
        }
        catch (Exception ex)
        {
            lblMsg.CssClass = "text-danger";
            lblMsg.Text = "❌ 加班数据查询失败：" + ex.Message;
            gvStat.DataSource = null; gvStat.DataBind();
            gvDept.DataSource = null; gvDept.DataBind();
            hidLabels.Value = hidHours.Value = "[]";
            return;
        }
        allOt = allOt.Where(o => empDict.ContainsKey(o.EmpId)).ToList();

        // ---- 按员工汇总（只显示有加班数据的员工，避免大堆 0 行视觉混乱） ----
        var groups = new Dictionary<int, OtStatRow>();
        foreach (var o in allOt)
        {
            Employee emp;
            if (!empDict.TryGetValue(o.EmpId, out emp)) continue;
            OtStatRow row;
            if (!groups.TryGetValue(emp.EmpId, out row))
            {
                row = new OtStatRow
                {
                    GroupKey = emp.EmpName + " (" + emp.EmpNo + ")",
                    EmpId = emp.EmpId
                };
                groups[emp.EmpId] = row;
            }
            row.RecordCount++;
            row.TotalHours += o.OtHours;
            switch (o.OtType)
            {
                case "工作日": row.Workday += o.OtHours; break;
                case "周末": row.Weekend += o.OtHours; break;
                case "节假日": row.Holiday += o.OtHours; break;
                default: row.Workday += o.OtHours; break;
            }
        }

        gvStat.DataSource = groups.Values.OrderByDescending(x => x.TotalHours).ToList();
        gvStat.DataBind();

        // ================================================================
        // 按部门汇总（KEY 用 DeptId，不用 DeptName 字符串！
        //            防止不同 DeptId 起了同一个部门名导致人数混加）
        // ================================================================
        var allDepts = DepartmentBLL.GetAll();
        var deptById = new Dictionary<int, Department>();
        foreach (var d in allDepts) deptById[d.DeptId] = d;

        var deptGroups = new Dictionary<int, OtDeptAcc>();
        Action<int, string> ensureDept = (id, name) =>
        {
            if (!deptGroups.ContainsKey(id))
                deptGroups[id] = new OtDeptAcc { DeptId = id, DeptName = name };
        };

        if (!deptId.HasValue)
        {
            foreach (var d in allDepts)
                ensureDept(d.DeptId, d.DeptName);
        }
        else
        {
            var d = deptById.ContainsKey(deptId.Value) ? deptById[deptId.Value] : null;
            string nm = d != null ? d.DeptName : "未分配";
            int id = d != null ? d.DeptId : -1;
            ensureDept(id, nm);
        }
        ensureDept(-1, "未分配");

        // 把已查询到的加班记录，按部门累加
        foreach (var o in allOt)
        {
            Employee emp = empDict[o.EmpId];
            int did = emp.DeptId.HasValue && deptById.ContainsKey(emp.DeptId.Value)
                ? emp.DeptId.Value : -1;
            string dnm = did == -1 ? "未分配" : deptById[did].DeptName;
            ensureDept(did, dnm);
            deptGroups[did].TotalHours += o.OtHours;
            deptGroups[did].EmpSet.Add(emp.EmpId);
        }

        var deptDt = new DataTable();
        deptDt.Columns.Add("Period", typeof(string));
        deptDt.Columns.Add("DeptName", typeof(string));
        deptDt.Columns.Add("DeptEmpCount", typeof(int));   // 该部门总在职人数（含试用期/实习期）
        deptDt.Columns.Add("OtEmpCount", typeof(int));     // 该周期内有加班记录的人数
        deptDt.Columns.Add("TotalHours", typeof(decimal));
        var labels = new List<string>();
        var hours = new List<double>();

        foreach (var dg in deptGroups.Values.OrderBy(x => x.DeptName))
        {
            int deptEmpCount;
            if (dg.DeptId == -1)
                deptEmpCount = empList.Count(ep => !ep.DeptId.HasValue || !deptById.ContainsKey(ep.DeptId.Value));
            else
                deptEmpCount = empList.Count(ep => ep.DeptId.HasValue && ep.DeptId.Value == dg.DeptId);
            if (deptEmpCount == 0) deptEmpCount = dg.EmpSet.Count;

            int otEmpCount = dg.EmpSet.Count;
            deptDt.Rows.Add(periodLabel, dg.DeptName, deptEmpCount, otEmpCount, Math.Round(dg.TotalHours, 1));
            labels.Add(dg.DeptName);
            hours.Add(Math.Round((double)dg.TotalHours, 2));
        }
        gvDept.DataSource = deptDt;
        gvDept.DataBind();

        var ser = new JavaScriptSerializer();
        hidLabels.Value = ser.Serialize(labels);
        hidHours.Value = ser.Serialize(hours);

        // ---- 顶部提示 ----
        lblMsg.CssClass = allOt.Count == 0 ? "text-warning" : "text-info";
        if (allOt.Count == 0)
        {
            lblMsg.Text = "⚠ " + s.ToString("yyyy-MM-dd") + " ~ " + ed.ToString("yyyy-MM-dd")
                + "：无任何「已批准」加班记录。若本月已提交申请尚未审批，请先在加班管理页面审批通过。";
        }
        else
        {
            int deptActiveCount = deptGroups.Values.Count(x => x.EmpSet.Count > 0);
            lblMsg.Text = s.ToString("yyyy-MM-dd") + " ~ " + ed.ToString("yyyy-MM-dd")
                + " 共 " + allOt.Count + " 条「已批准」加班记录，涉及 "
                + groups.Count + " 位员工 / " + deptActiveCount + " 个部门（共 " + deptGroups.Count + " 个部门）。";
        }
    }

    public class OtStatRow
    {
        public int EmpId { get; set; }
        public string GroupKey { get; set; }
        public decimal Workday { get; set; }   // 工作日
        public decimal Weekend { get; set; }   // 周末
        public decimal Holiday { get; set; }   // 节假日
        public decimal TotalHours { get; set; } // 总时长
        public int RecordCount { get; set; }    // 次数
    }

    public class OtDeptAcc
    {
        public int DeptId;           // KEY=DeptId（不用 DeptName 字符串防重复名混加）
        public string DeptName;
        public decimal TotalHours;
        public HashSet<int> EmpSet = new HashSet<int>();
    }
}
