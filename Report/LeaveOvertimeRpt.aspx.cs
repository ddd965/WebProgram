using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Report_LeaveOvertimeRpt : HRMS.Common.BasePage
{
    private List<LeaveOtRow> _lastRows;

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
            int y = DateTime.Now.Year;
            for (int i = y - 3; i <= y; i++)
                ddlYear.Items.Add(new ListItem(i + "年", i.ToString()));
            ddlYear.SelectedValue = y.ToString();
            DoGen();
        }
    }

    protected void btnGen_Click(object sender, EventArgs e) { DoGen(); }

    protected void btnExport_Click(object sender, EventArgs e)
    {
        try
        {
            DoGen();
            if (_lastRows == null || _lastRows.Count == 0)
            {
                lblMsg.Text = "⚠ 无可导出数据。"; return;
            }
            var sb = new StringBuilder();
            sb.AppendLine("分组,ColA,ColB,ColC,合计,次数,金额");
            foreach (var r in _lastRows)
            {
                sb.AppendFormat("\"{0}\",{1},{2},{3},{4},{5},{6}\r\n",
                    r.GroupKey, r.ColA, r.ColB, r.ColC, r.Total, r.Count, r.Amount.ToString("N2"));
            }
            byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
            byte[] buf = Encoding.UTF8.GetBytes(sb.ToString());
            Response.Clear();
            Response.ContentType = "text/csv; charset=utf-8";
            Response.AddHeader("Content-Disposition", $"attachment; filename=LeaveOtRpt_{DateTime.Now:yyyyMMdd_HHmm}.csv");
            Response.BinaryWrite(bom.Concat(buf).ToArray());
            Response.Flush(); Response.End();
            WriteLog.Write(CurrentUserName, "导出", $"导出休假/加班报表 {_lastRows.Count} 行");
        }
        catch (Exception ex) { lblMsg.Text = "⚠ 导出失败：" + ex.Message; }
    }

    private void DoGen()
    {
        int? deptId = string.IsNullOrEmpty(ddlDept.SelectedValue) ? (int?)null : int.Parse(ddlDept.SelectedValue);
        if (IsInRole("部门经理") && !IsInRole("管理员") && CurrentEmpId.HasValue)
        {
            var me = EmployeeBLL.GetById(CurrentEmpId.Value);
            if (me != null && me.DeptId.HasValue) deptId = me.DeptId.Value;
        }
        int year = int.Parse(ddlYear.SelectedValue);
        int month = int.Parse(ddlMonth.SelectedValue);
        string type = ddlRptType.SelectedValue;

        DateTime from = new DateTime(year, month == 0 ? 1 : month, 1);
        DateTime to = month == 0 ? new DateTime(year, 12, 31) : from.AddMonths(1).AddDays(-1);

        var emps = EmployeeBLL.GetAll();
        if (deptId.HasValue) emps = emps.Where(e => e.DeptId == deptId.Value).ToList();
        var empDict = emps.ToDictionary(x => x.EmpId);

        var rows = new List<LeaveOtRow>();
        if (type == "leave")
        {
            var leaves = LeaveRecordBLL.Query(null, null, from, to.AddDays(1), null);
            leaves = leaves.Where(l => empDict.ContainsKey(l.EmpId)).ToList();
            var typeCache = new Dictionary<int, LeaveType>();
            Func<int, string> typeName = id =>
            {
                if (typeCache.ContainsKey(id)) return typeCache[id]?.TypeName ?? "?";
                var t = LeaveTypeBLL.GetById(id);
                typeCache[id] = t;
                return t?.TypeName ?? "?";
            };
            foreach (var l in leaves)
            {
                var emp = empDict[l.EmpId];
                string key = $"{emp.EmpName} ({emp.EmpNo})";
                var r = rows.FirstOrDefault(x => x.GroupKey == key);
                if (r == null) { r = new LeaveOtRow { GroupKey = key }; rows.Add(r); }
                r.Count++; r.Total += l.LeaveDays;
                string tn = typeName(l.LeaveTypeId);
                if (tn == "年假" || tn == "事假") r.ColA += l.LeaveDays;
                else if (tn == "病假" || tn == "调休") r.ColB += l.LeaveDays;
                else r.ColC += l.LeaveDays;
            }
        }
        else
        {
            var ots = OvertimeBLL.Query(null, from, to.AddDays(1), null, "已批准");
            ots = ots.Where(o => empDict.ContainsKey(o.EmpId)).ToList();
            var baseCache = new Dictionary<int, decimal>();
            foreach (var o in ots)
            {
                var emp = empDict[o.EmpId];
                string key = $"{emp.EmpName} ({emp.EmpNo})";
                var r = rows.FirstOrDefault(x => x.GroupKey == key);
                if (r == null) { r = new LeaveOtRow { GroupKey = key }; rows.Add(r); }
                r.Count++; r.Total += o.OtHours;
                switch (o.OtType)
                {
                    case "工作日": r.ColA += o.OtHours; break;
                    case "周末": r.ColB += o.OtHours; break;
                    case "节假日": r.ColC += o.OtHours; break;
                }
                if (!baseCache.ContainsKey(emp.EmpId))
                {
                    var slist = SalaryBLL.QueryByEmpAndMonth(emp.EmpId, null, null);
                    baseCache[emp.EmpId] = slist.Count > 0 ? slist[slist.Count - 1].BaseSalary : 5000m;
                }
                r.Amount += OvertimeBLL.CalculateOvertimePay(baseCache[emp.EmpId], o.OtHours, o.OtType);
            }
        }
        rows = rows.OrderByDescending(x => x.Total).ToList();
        _lastRows = rows;

        gvRpt.DataSource = rows;
        gvRpt.DataBind();

        if (rows.Count > 0 && gvRpt.FooterRow != null)
        {
            gvRpt.FooterRow.Cells[0].Text = "合计";
            gvRpt.FooterRow.Cells[1].Text = rows.Sum(r => r.ColA).ToString("0.#");
            gvRpt.FooterRow.Cells[2].Text = rows.Sum(r => r.ColB).ToString("0.#");
            gvRpt.FooterRow.Cells[3].Text = rows.Sum(r => r.ColC).ToString("0.#");
            gvRpt.FooterRow.Cells[4].Text = rows.Sum(r => r.Total).ToString("0.#");
            gvRpt.FooterRow.Cells[5].Text = rows.Sum(r => r.Count).ToString();
            gvRpt.FooterRow.Cells[6].Text = "¥" + rows.Sum(r => r.Amount).ToString("N2");
        }

        lblMsg.Text = $"{(type == "leave" ? "休假" : "加班")}报表：{from:yyyy-MM-dd}~{to:yyyy-MM-dd}，共 {rows.Count} 位员工。";
        litGenTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        litOperator.Text = CurrentUserName;
    }

    public class LeaveOtRow
    {
        public string GroupKey { get; set; }
        public decimal ColA { get; set; }
        public decimal ColB { get; set; }
        public decimal ColC { get; set; }
        public decimal Total { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}
