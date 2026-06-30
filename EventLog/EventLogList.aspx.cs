using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class EventLog_EventLogList : HRMS.Common.BasePage
{
    private List<EventLog> _cacheList;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindData();
        }
    }

    private void BindData()
    {
        string userName = txtUserName.Text.Trim();
        DateTime? startDate = string.IsNullOrEmpty(txtStartDate.Text) ? (DateTime?)null : DateTime.Parse(txtStartDate.Text);
        DateTime? endDate = string.IsNullOrEmpty(txtEndDate.Text) ? (DateTime?)null : DateTime.Parse(txtEndDate.Text).AddDays(1);
        string eventName = ddlEventName.SelectedValue;

        _cacheList = EventLogBLL.Query(userName, startDate, endDate, eventName);

        gvEventLog.DataSource = _cacheList;
        gvEventLog.DataBind();

        lblMsg.Text = _cacheList.Count == 0 ? "没有符合条件的日志。" : $"共 {_cacheList.Count} 条日志。";
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        gvEventLog.PageIndex = 0;
        BindData();
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        txtUserName.Text = "";
        txtStartDate.Text = "";
        txtEndDate.Text = "";
        ddlEventName.SelectedIndex = 0;
        gvEventLog.PageIndex = 0;
        BindData();
    }

    protected void gvEventLog_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvEventLog.PageIndex = e.NewPageIndex;
        BindData();
    }

    protected void cbAll_CheckedChanged(object sender, EventArgs e)
    {
        bool checkAll = ((CheckBox)gvEventLog.HeaderRow.FindControl("cbAll")).Checked;
        for (int i = 0; i < gvEventLog.Rows.Count; i++)
        {
            var cb = (CheckBox)gvEventLog.Rows[i].FindControl("cbSelect");
            if (cb != null) cb.Checked = checkAll;
        }
    }

    protected void btnDeleteSelected_Click(object sender, EventArgs e)
    {
        int deletedCount = 0;
        for (int i = 0; i < gvEventLog.Rows.Count; i++)
        {
            var cb = (CheckBox)gvEventLog.Rows[i].FindControl("cbSelect");
            if (cb != null && cb.Checked)
            {
                var hid = (HiddenField)gvEventLog.Rows[i].FindControl("hidLogId");
                if (hid != null)
                {
                    long logId;
                    if (long.TryParse(hid.Value, out logId))
                    {
                        EventLogBLL.Delete(logId);
                        deletedCount++;
                    }
                }
            }
        }
        WriteLog.Write(CurrentUserName, "删除", $"批量删除 {deletedCount} 条日志");
        lblMsg.Text = $"已删除 {deletedCount} 条日志。";
        gvEventLog.PageIndex = 0;
        BindData();
    }

    protected void btnDeleteBefore6Month_Click(object sender, EventArgs e)
    {
        int deleted = EventLogBLL.DeleteOldLogs(6);
        WriteLog.Write(CurrentUserName, "删除", $"清理 {deleted} 条6个月前日志");
        lblMsg.Text = $"已清理 {deleted} 条旧日志。";
        gvEventLog.PageIndex = 0;
        BindData();
    }

    protected void gvEventLog_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "DeleteLog")
        {
            long logId;
            if (long.TryParse(e.CommandArgument.ToString(), out logId))
            {
                EventLogBLL.Delete(logId);
                WriteLog.Write(CurrentUserName, "删除", $"删除日志 ID={logId}");
                BindData();
                lblMsg.Text = "日志已删除。";
            }
        }
    }
}
