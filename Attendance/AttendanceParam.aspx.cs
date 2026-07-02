using System;
using System.Web.UI.WebControls;
using HRMS.Model;
using HRMS.BLL;
using HRMS.Common;

public partial class Attendance_AttendanceParam : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindSetting();
        }
    }

    private void BindSetting()
    {
        var setting = AttendanceSettingBLL.GetSetting();
        dvSetting.DataSource = new[] { setting };
        dvSetting.DataBind();
    }

    protected void dvSetting_ItemUpdating(object sender, DetailsViewUpdateEventArgs e)
    {
        var setting = new AttendanceSetting
        {
            WorkStartTime = TimeSpan.Parse(e.NewValues["WorkStartTime"].ToString()),
            WorkEndTime = TimeSpan.Parse(e.NewValues["WorkEndTime"].ToString()),
            LateMinutes = Convert.ToInt32(e.NewValues["LateMinutes"]),
            EarlyMinutes = Convert.ToInt32(e.NewValues["EarlyMinutes"]),
            LunchStart = TimeSpan.Parse(e.NewValues["LunchStart"].ToString()),
            LunchEnd = TimeSpan.Parse(e.NewValues["LunchEnd"].ToString())
        };

        AttendanceSettingBLL.Update(setting);

        // 写操作日志
        WriteLog.Write(CurrentUserName, "修改参数", "更新考勤参数设置");

        dvSetting.ChangeMode(DetailsViewMode.ReadOnly);
        BindSetting();
        lblMsg.Text = "考勤参数更新成功！";
    }
}