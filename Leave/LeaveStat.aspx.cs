using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Leave_LeaveStat : HRMS.Common.BasePage
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

            int curYear = DateTime.Now.Year;
            for (int y = curYear - 3; y <= curYear; y++)
                ddlYear.Items.Add(new ListItem(y + "年", y.ToString()));
            ddlYear.SelectedValue = curYear.ToString();

            DoStat();
        }
    }

    protected void btnStat_Click(object sender, EventArgs e)
    {
        gvStat.PageIndex = 0;
        DoStat();
    }

    private void DoStat()
    {
        int? deptId = string.IsNullOrEmpty(ddlDept.SelectedValue) ? (int?)null : int.Parse(ddlDept.SelectedValue);
        // 部门经理只能看本部门
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
        string dim = ddlDimension.SelectedValue;

        var all = LeaveRecordBLL.Query(null, null, new DateTime(year, 1, 1), new DateTime(year, 12, 31), null);
        if (deptId.HasValue)
        {
            all = all.Where(l =>
            {
                var emp = EmployeeBLL.GetById(l.EmpId);
                return emp != null && emp.DeptId == deptId.Value;
            }).ToList();
        }

        var groups = new Dictionary<string, StatRow>();
        if (dim == "month")
        {
            for (int m = 1; m <= 12; m++)
                groups[m + "月"] = new StatRow { GroupKey = m + "月" };
        }

        foreach (var l in all)
        {
            string key = "";
            if (dim == "dept")
            {
                var emp = EmployeeBLL.GetById(l.EmpId);
                if (emp != null && emp.DeptId.HasValue)
                {
                    var d = DepartmentBLL.GetById(emp.DeptId.Value);
                    key = d?.DeptName ?? "未分配";
                }
                else key = "未分配部门";
            }
            else if (dim == "type")
            {
                var t = LeaveTypeBLL.GetById(l.LeaveTypeId);
                key = t?.TypeName ?? "未知类型";
            }
            else if (dim == "month")
            {
                key = l.StartDate.Month + "月";
            }
            else // emp
            {
                var emp = EmployeeBLL.GetById(l.EmpId);
                key = emp != null ? $"{emp.EmpName} ({emp.EmpNo})" : "未知员工";
            }

            if (!groups.ContainsKey(key))
                groups[key] = new StatRow { GroupKey = key };
            groups[key].申请次数++;
            groups[key].总天数 += l.LeaveDays;
            if (l.Status == "已批准") groups[key].已批准天数 += l.LeaveDays;
            else if (l.Status == "待审批") groups[key].待审批天数 += l.LeaveDays;
        }

        var dt = new List<StatRow>(groups.Values);
        gvStat.DataSource = dt;
        gvStat.DataBind();

        // === 序列化 Chart 数据 ===
        var ser = new JavaScriptSerializer();
        var labels = dt.Select(x => x.GroupKey).ToList();
        var totals = dt.Select(x => Math.Round((double)x.总天数, 2)).ToList();
        var approved = dt.Select(x => Math.Round((double)x.已批准天数, 2)).ToList();
        hidLabels.Value = ser.Serialize(labels);
        hidTotalDays.Value = ser.Serialize(totals);
        hidApprovedDays.Value = ser.Serialize(approved);

        lblMsg.Text = $"共统计 {all.Count} 条休假记录，{dt.Count} 个分组。";
    }

    public class StatRow
    {
        public string GroupKey { get; set; }
        public int 申请次数 { get; set; }
        public decimal 已批准天数 { get; set; }
        public decimal 待审批天数 { get; set; }
        public decimal 总天数 { get; set; }
    }

    protected void gvStat_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvStat.PageIndex = e.NewPageIndex;
        DoStat();
    }
}
