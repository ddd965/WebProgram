using System;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Leave_LeaveApply : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // === PRG 成功提示回显 ===
            if (Session["LeaveApplyMsg"] != null)
            {
                lblMsg.Text = Session["LeaveApplyMsg"].ToString();
                lblMsg.CssClass = "text-success";
                Session.Remove("LeaveApplyMsg");
            }
            LoadDropdowns();
            // 普通用户只能选择自己
            if (!IsInRole("管理员", "部门经理"))
            {
                if (CurrentEmpId.HasValue)
                {
                    var li = ddlEmployee.Items.FindByValue(CurrentEmpId.Value.ToString());
                    if (li != null) ddlEmployee.SelectedValue = CurrentEmpId.Value.ToString();
                    ddlEmployee.Enabled = false;
                }
            }
        }
    }

    private void LoadDropdowns()
    {
        var emps = EmployeeBLL.Query(null, null, null, null, "在职");
        ddlEmployee.Items.Clear();
        foreach (var emp in emps)
            ddlEmployee.Items.Add(new ListItem($"{emp.EmpName} ({emp.EmpNo})", emp.EmpId.ToString()));

        var types = LeaveTypeBLL.GetAll();
        ddlLeaveType.Items.Clear();
        foreach (var t in types)
            ddlLeaveType.Items.Add(new ListItem(t.TypeName, t.LeaveTypeId.ToString()));
    }

    protected void btnCalc_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txtStartDate.Text) || string.IsNullOrEmpty(txtEndDate.Text)) return;
        DateTime s, ed;
        if (!DateTime.TryParse(txtStartDate.Text, out s) || !DateTime.TryParse(txtEndDate.Text, out ed))
        {
            lblMsg.CssClass = "text-danger";
            lblMsg.Text = "⚠ 请选择正确的起止日期。";
            return;
        }
        if (ed < s)
        {
            lblMsg.CssClass = "text-danger";
            lblMsg.Text = "⚠ 结束日期不能早于开始日期！";
            return;
        }
        txtLeaveDays.Text = LeaveRecordBLL.CalculateLeaveDays(s, ed).ToString();
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        try
        {
            Page.Validate("ApplyGrp");
            if (!Page.IsValid)
            {
                lblMsg.CssClass = "text-danger";
                lblMsg.Text = "⚠ 请完善申请单上方红色标注的必填项后再提交。";
                return;
            }
            lblMsg.CssClass = "text-info";
            int empId;
            if (!int.TryParse(ddlEmployee.SelectedValue, out empId))
            {
                lblMsg.Text = "⚠ 请选择员工！";
                lblMsg.CssClass = "text-danger";
                return;
            }
            int typeId = int.Parse(ddlLeaveType.SelectedValue);
            DateTime s = DateTime.Parse(txtStartDate.Text);
            DateTime ed = DateTime.Parse(txtEndDate.Text);
            if (ed < s)
            {
                lblMsg.CssClass = "text-danger";
                lblMsg.Text = "⚠ 结束日期不能早于开始日期！";
                return;
            }
            decimal days = LeaveRecordBLL.CalculateLeaveDays(s, ed);

            var lr = new LeaveRecord
            {
                EmpId = empId,
                LeaveTypeId = typeId,
                StartDate = s,
                EndDate = ed,
                LeaveDays = days,
                Reason = (txtReason.Text ?? "").Trim(),
                Status = "待审批"
            };
            LeaveRecordBLL.Insert(lr);
            WriteLog.Write(CurrentUserName, "新增", $"提交休假申请：员工ID={empId}，{s:yyyy-MM-dd}~{ed:yyyy-MM-dd} 共{days}天");

            // ===== PRG 模式 (Post-Redirect-Get)：成功后重定向，避免刷新浏览器重复提交 =====
            Session["LeaveApplyMsg"] = $"✅ 休假申请已提交，共计 {days} 个工作日。状态：待审批。";
            Response.Redirect(Request.RawUrl, true);
        }
        catch (Exception ex)
        {
            lblMsg.CssClass = "text-danger";
            lblMsg.Text = "⚠ 提交失败：" + ex.Message;
        }
    }
}
