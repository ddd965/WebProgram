using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using HRMS.Model;
using HRMS.BLL;

public partial class LeaveManagement_LeaveTypeMgr : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindLeaveTypes();
        }
    }

    private void BindLeaveTypes()
    {
        gvLeaveTypes.DataSource = LeaveTypeBLL.GetAll();
        gvLeaveTypes.DataBind();
    }

    protected void gvLeaveTypes_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gvLeaveTypes.EditIndex = e.NewEditIndex;
        BindLeaveTypes();
    }

    protected void gvLeaveTypes_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        int leaveTypeId = Convert.ToInt32(gvLeaveTypes.DataKeys[e.RowIndex].Value);
        GridViewRow row = gvLeaveTypes.Rows[e.RowIndex];

        LeaveType lt = new LeaveType
        {
            LeaveTypeId = leaveTypeId,
            TypeName = ((TextBox)row.Cells[1].Controls[0]).Text,
            DefaultDays = Convert.ToDecimal(((TextBox)row.Cells[2].Controls[0]).Text),
            NeedApproval = ((CheckBox)row.Cells[3].Controls[0]).Checked
        };
        LeaveTypeBLL.Update(lt);
        gvLeaveTypes.EditIndex = -1;
        BindLeaveTypes();
    }

    protected void gvLeaveTypes_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        gvLeaveTypes.EditIndex = -1;
        BindLeaveTypes();
    }

    protected void gvLeaveTypes_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        int leaveTypeId = Convert.ToInt32(gvLeaveTypes.DataKeys[e.RowIndex].Value);
        LeaveTypeBLL.Delete(leaveTypeId);
        BindLeaveTypes();
    }

    protected void dvLeaveType_ItemInserting(object sender, DetailsViewInsertEventArgs e)
    {
        LeaveType lt = new LeaveType
        {
            TypeName = e.Values["TypeName"].ToString(),
            DefaultDays = Convert.ToDecimal(e.Values["DefaultDays"]),
            NeedApproval = Convert.ToBoolean(e.Values["NeedApproval"])
        };
        LeaveTypeBLL.Insert(lt);
        BindLeaveTypes();
    }

    protected void dvLeaveType_ItemUpdating(object sender, DetailsViewUpdateEventArgs e)
    {
    }

    protected void dvLeaveType_ItemDeleting(object sender, DetailsViewDeleteEventArgs e)
    {
    }
}
