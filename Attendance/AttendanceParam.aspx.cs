using System;
using HRMS.BLL;
using HRMS.Model;

public partial class Attendance_AttendanceParam : HRMS.Common.BasePage
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
            LoadSetting();
        }
    }

    private void LoadSetting()
    {
        var s = AttendanceSettingBLL.GetSetting();
        if (s != null)
        {
            txtWorkStart.Text = s.WorkStartTime.ToString(@"hh\:mm");
            txtWorkEnd.Text = s.WorkEndTime.ToString(@"hh\:mm");
            txtLateMinutes.Text = s.LateMinutes.ToString();
            txtEarlyMinutes.Text = s.EarlyMinutes.ToString();
            txtLunchStart.Text = s.LunchStart.ToString(@"hh\:mm");
            txtLunchEnd.Text = s.LunchEnd.ToString(@"hh\:mm");
            litUpdateTime.Text = $" <small class=\"text-muted\">最近更新：{s.UpdateTime:yyyy-MM-dd HH:mm}</small>";
        }
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        var setting = new AttendanceSetting
        {
            SettingId = 1,
            WorkStartTime = TimeSpan.Parse(txtWorkStart.Text),
            WorkEndTime = TimeSpan.Parse(txtWorkEnd.Text),
            LateMinutes = int.Parse(txtLateMinutes.Text),
            EarlyMinutes = int.Parse(txtEarlyMinutes.Text),
            LunchStart = TimeSpan.Parse(txtLunchStart.Text),
            LunchEnd = TimeSpan.Parse(txtLunchEnd.Text)
        };
        if (setting.WorkEndTime <= setting.WorkStartTime)
        {
            lblMsg.Text = "下班时间必须晚于上班时间！";
            lblMsg.CssClass = "text-danger";
            return;
        }

        AttendanceSettingBLL.Update(setting);
        WriteLog.Write(CurrentUserName, "修改", "更新考勤参数设置");
        lblMsg.Text = "考勤参数已更新。";
        lblMsg.CssClass = "text-info";
        LoadSetting();
    }
}
