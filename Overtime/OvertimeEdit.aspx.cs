using System;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Overtime_OvertimeEdit : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            try
            {
                var emps = EmployeeBLL.Query(null, null, null, null, "在职");
                ddlEmployee.Items.Clear();
                ddlEmployee.Items.Add(new ListItem("", ""));
                foreach (var emp in emps)
                    ddlEmployee.Items.Add(new ListItem($"{emp.EmpName} ({emp.EmpNo})", emp.EmpId.ToString()));

                if (!IsInRole("管理员", "部门经理"))
                {
                    if (CurrentEmpId.HasValue)
                    {
                        var li = ddlEmployee.Items.FindByValue(CurrentEmpId.Value.ToString());
                        if (li != null) ddlEmployee.SelectedValue = CurrentEmpId.Value.ToString();
                        ddlEmployee.Enabled = false;
                    }
                }
                else if (IsInRole("部门经理") && !IsInRole("管理员") && CurrentEmpId.HasValue)
                {
                    var me = EmployeeBLL.GetById(CurrentEmpId.Value);
                    if (me != null && me.DeptId.HasValue)
                    {
                        int dept = me.DeptId.Value;
                        for (int i = ddlEmployee.Items.Count - 1; i >= 1; i--)
                        {
                            int eid = int.Parse(ddlEmployee.Items[i].Value);
                            var ee = EmployeeBLL.GetById(eid);
                            if (ee == null || ee.DeptId != dept) ddlEmployee.Items.RemoveAt(i);
                        }
                    }
                }
                txtOtDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                lblMsg.CssClass = "text-danger";
                lblMsg.Text = "⚠ 加载员工列表失败：" + ex.Message;
            }
        }
    }

    protected void btnCalc_Click(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(txtStartTime.Text) || string.IsNullOrEmpty(txtEndTime.Text)) return;
            TimeSpan st, et;
            if (!TimeSpan.TryParse(txtStartTime.Text, out st) || !TimeSpan.TryParse(txtEndTime.Text, out et))
            {
                lblMsg.CssClass = "text-danger";
                lblMsg.Text = "⚠ 时间格式不正确。";
                return;
            }
            if (et <= st)
            {
                lblMsg.CssClass = "text-danger";
                lblMsg.Text = "⚠ 结束时间必须晚于开始时间！";
                return;
            }
            txtOtHours.Text = Math.Round((decimal)(et - st).TotalHours, 1).ToString("0.0");
            lblMsg.CssClass = "text-info";
            lblMsg.Text = $"已自动计算加班小时：{txtOtHours.Text} h";
        }
        catch (Exception ex)
        {
            lblMsg.CssClass = "text-danger";
            lblMsg.Text = "⚠ 计算时长失败：" + ex.Message;
        }
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        Page.Validate("OtGrp");
        if (!Page.IsValid)
        {
            lblMsg.CssClass = "text-danger";
            lblMsg.Text = "⚠ 请完善上方红色标注的必填项后再提交。";
            return;
        }
        try
        {
            lblMsg.CssClass = "text-info";
            int empId = int.Parse(ddlEmployee.SelectedValue);
            DateTime d;
            TimeSpan st, et;
            if (!DateTime.TryParse(txtOtDate.Text, out d)) { lblMsg.Text = "⚠ 日期格式错误！"; lblMsg.CssClass = "text-danger"; return; }
            if (!TimeSpan.TryParse(txtStartTime.Text, out st)) { lblMsg.Text = "⚠ 开始时间格式错误！"; lblMsg.CssClass = "text-danger"; return; }
            if (!TimeSpan.TryParse(txtEndTime.Text, out et)) { lblMsg.Text = "⚠ 结束时间格式错误！"; lblMsg.CssClass = "text-danger"; return; }
            if (et <= st)
            {
                lblMsg.CssClass = "text-danger";
                lblMsg.Text = "⚠ 结束时间必须晚于开始时间！";
                return;
            }
            var ot = new Overtime
            {
                EmpId = empId,
                OtDate = d,
                StartTime = st,
                EndTime = et,
                OtHours = Math.Round((decimal)(et - st).TotalHours, 1),
                OtType = ddlOtType.SelectedValue,
                Reason = (txtReason.Text ?? "").Trim(),
                Status = "待审批"
            };
            OvertimeBLL.Insert(ot);
            var emp = EmployeeBLL.GetById(empId);
            WriteLog.Write(CurrentUserName, "新增", $"登记加班：{emp?.EmpName} {d:yyyy-MM-dd} {ot.OtHours}小时({ot.OtType})");
            lblMsg.CssClass = "text-success";
            lblMsg.Text = $"✅ 加班已提交，共 {ot.OtHours} 小时（待审批）。";
            txtReason.Text = ""; txtOtHours.Text = "";
        }
        catch (Exception ex)
        {
            lblMsg.CssClass = "text-danger";
            lblMsg.Text = "⚠ 提交失败：" + ex.Message;
        }
    }
}
