using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Overtime_OvertimeList : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) BindData();
    }

    protected string GetEmpName(object empId)
    {
        if (empId == null || empId == DBNull.Value) return "—";
        var emp = EmployeeBLL.GetById(Convert.ToInt32(empId));
        return emp != null ? $"{emp.EmpName} ({emp.EmpNo})" : "—";
    }

    protected string FormatTime(object ts)
    {
        if (ts == null || ts == DBNull.Value) return "—";
        try { return ((TimeSpan)ts).ToString(@"hh\:mm"); }
        catch { return ts.ToString(); }
    }

    protected string GetStatusCss(string s)
    {
        switch (s)
        {
            case "待审批": return "label label-warning";
            case "已批准": return "label label-success";
            case "已拒绝": return "label label-danger";
            default: return "";
        }
    }

    protected bool CanApprove() => IsInRole("管理员", "部门经理");

    private void BindData()
    {
        DateTime? s = null, ed = null;
        DateTime ts;
        if (!string.IsNullOrEmpty(txtQStart.Text) && DateTime.TryParse(txtQStart.Text, out ts)) s = ts;
        if (!string.IsNullOrEmpty(txtQEnd.Text) && DateTime.TryParse(txtQEnd.Text, out ts)) ed = ts.AddDays(1);

        string type = ddlQType.SelectedValue;
        string status = ddlQStatus.SelectedValue;
        string q = (txtQEmp.Text ?? "").Trim();
        int? empId = null;

        List<Overtime> list;
        if (!IsInRole("管理员", "部门经理") && CurrentEmpId.HasValue)
            empId = CurrentEmpId.Value;

        if (IsInRole("部门经理") && !IsInRole("管理员") && CurrentEmpId.HasValue)
        {
            var me = EmployeeBLL.GetById(CurrentEmpId.Value);
            if (me != null && me.DeptId.HasValue)
            {
                int dept = me.DeptId.Value;
                var all = OvertimeBLL.Query(null, s, ed, type, status);
                list = all.Where(o =>
                {
                    var ee = EmployeeBLL.GetById(o.EmpId);
                    return ee != null && ee.DeptId == dept;
                }).ToList();
            }
            else list = new List<Overtime>();
        }
        else
        {
            list = OvertimeBLL.Query(empId, s, ed, type, status);
        }

        if (!string.IsNullOrEmpty(q))
        {
            var emps = EmployeeBLL.GetAll();
            list = list.Where(o =>
            {
                var emp = emps.FirstOrDefault(x => x.EmpId == o.EmpId);
                return emp != null && (emp.EmpName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                       || emp.EmpNo.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }).ToList();
        }

        gvOt.DataSource = list;
        try { gvOt.DataBind(); }
        catch (Exception ex)
        {
            lblMsg.CssClass = "text-danger";
            lblMsg.Text = "⚠ 绑定列表失败：" + ex.Message;
            return;
        }
        lblMsg.CssClass = "text-info";
        lblMsg.Text = $"共 {list.Count} 条加班记录。";
    }

    protected void btnSearch_Click(object sender, EventArgs e) { gvOt.PageIndex = 0; BindData(); }

    protected void gvOt_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvOt.PageIndex = e.NewPageIndex; BindData();
    }

    protected void gvOt_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        try
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "Approve" && CanApprove())
            {
                var ot = OvertimeBLL.GetById(id);
                if (ot != null) { ot.Status = "已批准"; OvertimeBLL.Update(ot); }
                WriteLog.Write(CurrentUserName, "审批", $"批准加班 ID={id}");
                lblMsg.Text = "已批准。";
            }
            else if (e.CommandName == "Reject" && CanApprove())
            {
                var ot = OvertimeBLL.GetById(id);
                if (ot != null) { ot.Status = "已拒绝"; OvertimeBLL.Update(ot); }
                WriteLog.Write(CurrentUserName, "审批", $"拒绝加班 ID={id}");
                lblMsg.Text = "已拒绝。";
            }
            else if (e.CommandName == "DeleteRow")
            {
                OvertimeBLL.Delete(id);
                WriteLog.Write(CurrentUserName, "删除", $"删除加班 ID={id}");
                lblMsg.Text = "已删除。";
            }
            lblMsg.CssClass = "text-info";
        }
        catch (Exception ex)
        {
            lblMsg.CssClass = "text-danger";
            lblMsg.Text = "⚠ 操作失败：" + ex.Message;
        }
        BindData();
    }
}
