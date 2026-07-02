using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using HRMS.Model;
using HRMS.BLL;

public partial class LeaveManagement_LeaveApply : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindEmployees();
            BindLeaveTypes();
        }
    }

    private void BindEmployees()
    {
        DropDownList ddlEmployee = (DropDownList)fvLeaveApply.FindControl("ddlEmployee");
        if (ddlEmployee != null)
        {
            ddlEmployee.DataSource = EmployeeBLL.GetAll();
            ddlEmployee.DataTextField = "EmpName";
            ddlEmployee.DataValueField = "EmpId";
            ddlEmployee.DataBind();
        }
    }

    private void BindLeaveTypes()
    {
        DropDownList ddlLeaveType = (DropDownList)fvLeaveApply.FindControl("ddlLeaveType");
        if (ddlLeaveType != null)
        {
            ddlLeaveType.DataSource = LeaveTypeBLL.GetAll();
            ddlLeaveType.DataTextField = "TypeName";
            ddlLeaveType.DataValueField = "LeaveTypeId";
            ddlLeaveType.DataBind();
        }
    }

    protected void fvLeaveApply_ItemInserting(object sender, FormViewInsertEventArgs e)
    {
        try
        {
            DropDownList ddlEmployee = (DropDownList)fvLeaveApply.FindControl("ddlEmployee");
            DropDownList ddlLeaveType = (DropDownList)fvLeaveApply.FindControl("ddlLeaveType");
            Calendar calStartDate = (Calendar)fvLeaveApply.FindControl("calStartDate");
            Calendar calEndDate = (Calendar)fvLeaveApply.FindControl("calEndDate");
            TextBox txtReason = (TextBox)fvLeaveApply.FindControl("txtReason");

            int empId = Convert.ToInt32(ddlEmployee.SelectedValue);
            int leaveTypeId = Convert.ToInt32(ddlLeaveType.SelectedValue);
            DateTime startDate = calStartDate.SelectedDate;
            DateTime endDate = calEndDate.SelectedDate;
            string reason = txtReason.Text;

            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue)
            {
                lblMessage.Text = "请选择休假开始日期和结束日期。";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
            if (startDate > endDate)
            {
                lblMessage.Text = "开始日期不能晚于结束日期。";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            decimal leaveDays = LeaveRecordBLL.CalculateLeaveDays(startDate, endDate);

            LeaveRecord lr = new LeaveRecord
            {
                EmpId = empId,
                LeaveTypeId = leaveTypeId,
                StartDate = startDate,
                EndDate = endDate,
                LeaveDays = leaveDays,
                Reason = reason,
                Status = "待审批",
                ApplyTime = DateTime.Now
            };

            LeaveRecordBLL.Insert(lr);
            lblMessage.Text = "休假申请已提交，等待审批。";
            lblMessage.ForeColor = System.Drawing.Color.Green;
        }
        catch (Exception ex)
        {
            lblMessage.Text = "提交申请失败: " + ex.Message;
            lblMessage.ForeColor = System.Drawing.Color.Red;
        }
    }
}
