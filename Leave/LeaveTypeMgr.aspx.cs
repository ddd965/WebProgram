using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Leave_LeaveTypeMgr : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsInRole("管理员"))
        {
            Response.Redirect("~/Default.aspx");
            return;
        }
        if (!IsPostBack)
        {
            BindData();
        }
    }

    private void BindData()
    {
        var list = LeaveTypeBLL.GetAll();
        gvLeaveType.DataSource = list;
        gvLeaveType.DataBind();
    }

    private void ClearForm()
    {
        txtTypeName.Text = "";
        txtDefaultDays.Text = "0";
        cbNeedApproval.Checked = true;
        txtDescn.Text = "";
        hidEditId.Value = "";
        litFormTitle.Text = "新增休假类型";
        btnCancel.Visible = false;
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        var lt = new LeaveType
        {
            TypeName = txtTypeName.Text.Trim(),
            DefaultDays = decimal.Parse(txtDefaultDays.Text),
            NeedApproval = cbNeedApproval.Checked,
            Descn = txtDescn.Text.Trim()
        };

        int editId;
        if (int.TryParse(hidEditId.Value, out editId) && editId > 0)
        {
            lt.LeaveTypeId = editId;
            LeaveTypeBLL.Update(lt);
            WriteLog.Write(CurrentUserName, "修改", $"修改休假类型 [{lt.TypeName}]");
            lblMsg.Text = "休假类型已更新。";
        }
        else
        {
            LeaveTypeBLL.Insert(lt);
            WriteLog.Write(CurrentUserName, "新增", $"新增休假类型 [{lt.TypeName}]");
            lblMsg.Text = "休假类型已添加。";
        }
        ClearForm();
        BindData();
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        ClearForm();
    }

    protected void gvLeaveType_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvLeaveType.PageIndex = e.NewPageIndex;
        BindData();
    }

    protected void gvLeaveType_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int id = Convert.ToInt32(e.CommandArgument);
        if (e.CommandName == "EditRow")
        {
            var lt = LeaveTypeBLL.GetById(id);
            if (lt != null)
            {
                hidEditId.Value = lt.LeaveTypeId.ToString();
                txtTypeName.Text = lt.TypeName;
                txtDefaultDays.Text = lt.DefaultDays.ToString();
                cbNeedApproval.Checked = lt.NeedApproval;
                txtDescn.Text = lt.Descn;
                litFormTitle.Text = "编辑休假类型";
                btnCancel.Visible = true;
            }
        }
        else if (e.CommandName == "DeleteRow")
        {
            try
            {
                LeaveTypeBLL.Delete(id);
                WriteLog.Write(CurrentUserName, "删除", $"删除休假类型 ID={id}");
                lblMsg.Text = "休假类型已删除。";
            }
            catch (Exception ex)
            {
                lblMsg.Text = $"删除失败：{ex.Message}";
                lblMsg.CssClass = "text-danger";
            }
            BindData();
        }
    }
}
