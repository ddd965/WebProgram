using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Leave_LeaveList : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            LoadDropdowns();
            BindData();
        }
    }

    private void LoadDropdowns()
    {
        var types = LeaveTypeBLL.GetAll();
        ddlQType.Items.Clear();
        ddlQType.Items.Add(new ListItem("全部", ""));
        foreach (var t in types)
            ddlQType.Items.Add(new ListItem(t.TypeName, t.LeaveTypeId.ToString()));
    }

    protected string GetEmpName(object empId)
    {
        if (empId == null || empId == DBNull.Value) return "—";
        var emp = EmployeeBLL.GetById(Convert.ToInt32(empId));
        return emp != null ? emp.EmpName + " (" + emp.EmpNo + ")" : "—";
    }

    protected string GetTypeName(object typeId)
    {
        if (typeId == null || typeId == DBNull.Value) return "—";
        var t = LeaveTypeBLL.GetById(Convert.ToInt32(typeId));
        return t?.TypeName ?? "—";
    }

    protected string GetStatusCss(string status)
    {
        switch (status)
        {
            case "待审批": return "label label-warning";
            case "已批准": return "label label-success";
            case "已拒绝": return "label label-danger";
            case "已取消": return "label label-default";
            default: return "";
        }
    }

    protected bool CanApprove() { return IsInRole("管理员", "部门经理"); }

    private void BindData()
    {
        string status = ddlQStatus.SelectedValue;
        int? typeId = string.IsNullOrEmpty(ddlQType.SelectedValue) ? (int?)null : int.Parse(ddlQType.SelectedValue);
        DateTime? start = null, end = null;
        try
        {
            DateTime ts;
            if (!string.IsNullOrEmpty(txtQStart.Text) && DateTime.TryParse(txtQStart.Text, out ts)) start = ts;
            if (!string.IsNullOrEmpty(txtQEnd.Text) && DateTime.TryParse(txtQEnd.Text, out ts)) end = ts.AddDays(1);
        }
        catch { lblMsg.Text = "⚠ 日期格式不正确。"; lblMsg.CssClass = "text-danger"; }

        int? empId = null;
        if (!IsInRole("管理员", "部门经理") && CurrentEmpId.HasValue)
            empId = CurrentEmpId.Value;

        var all = LeaveRecordBLL.Query(empId, typeId, start, end, status);

        if (IsInRole("部门经理") && !IsInRole("管理员") && CurrentEmpId.HasValue)
        {
            var me = EmployeeBLL.GetById(CurrentEmpId.Value);
            if (me != null && me.DeptId.HasValue)
            {
                int dept = me.DeptId.Value;
                var filtered = new List<LeaveRecord>();
                foreach (var l in all)
                {
                    var em = EmployeeBLL.GetById(l.EmpId);
                    if (em != null && em.DeptId == dept) filtered.Add(l);
                }
                all = filtered;
            }
        }
        gvLeave.DataSource = all;
        gvLeave.DataBind();
        lblMsg.Text = $"共 {all.Count} 条记录。";
        lblMsg.CssClass = "text-info";
    }

    protected void btnSearch_Click(object sender, EventArgs e) { gvLeave.PageIndex = 0; BindData(); }

    protected void gvLeave_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvLeave.PageIndex = e.NewPageIndex; BindData();
    }

    protected void gvLeave_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int leaveId = Convert.ToInt32(e.CommandArgument);
        int? approver = CurrentEmpId;
        switch (e.CommandName)
        {
            case "Approve":
                if (approver.HasValue && CanApprove())
                {
                    LeaveRecordBLL.Approve(leaveId, "已批准", approver.Value);
                    WriteLog.Write(CurrentUserName, "审批", $"批准休假 ID={leaveId}");
                    lblMsg.Text = "已批准。";
                }
                break;
            case "Reject":
                if (approver.HasValue && CanApprove())
                {
                    LeaveRecordBLL.Approve(leaveId, "已拒绝", approver.Value);
                    WriteLog.Write(CurrentUserName, "审批", $"拒绝休假 ID={leaveId}");
                    lblMsg.Text = "已拒绝。";
                }
                break;
            case "CancelRow":
                LeaveRecordBLL.Approve(leaveId, "已取消", approver ?? 0);
                WriteLog.Write(CurrentUserName, "修改", $"取消休假 ID={leaveId}");
                lblMsg.Text = "已取消。";
                break;
            case "DeleteRow":
                LeaveRecordBLL.Delete(leaveId);
                WriteLog.Write(CurrentUserName, "删除", $"删除休假 ID={leaveId}");
                lblMsg.Text = "已删除。";
                break;
        }
        BindData();
    }
}
