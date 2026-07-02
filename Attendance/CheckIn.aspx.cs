using System;
using System.Data;
using System.Data.SqlClient;
using HRMS.BLL;
using HRMS.DAL;
using HRMS.Common;

public partial class Attendance_CheckIn : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            lblCurrentTime.Text = "当前时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (CurrentEmpId.HasValue)
            {
                var emp = EmployeeBLL.GetById(CurrentEmpId.Value);
                lblEmpInfo.Text = "员工：" + emp.EmpName + "（" + emp.EmpNo + "）";
            }
            else
            {
                lblEmpInfo.Text = "未关联员工信息";
            }

            BindTodayRecord();
        }
    }

    protected void btnCheckIn_Click(object sender, EventArgs e)
    {
        if (!CurrentEmpId.HasValue)
        {
            lblError.Text = "当前用户未关联员工，无法打卡！";
            return;
        }

        AttendanceBLL.CheckIn(CurrentEmpId.Value);
        WriteLog.Write(CurrentUserName, "签到", "员工签到打卡");
        lblStatus.Text = "签到成功！时间：" + DateTime.Now.ToString("HH:mm:ss");
        lblError.Text = "";
        BindTodayRecord();
    }

    protected void btnCheckOut_Click(object sender, EventArgs e)
    {
        if (!CurrentEmpId.HasValue)
        {
            lblError.Text = "当前用户未关联员工，无法打卡！";
            return;
        }

        bool result = AttendanceBLL.CheckOut(CurrentEmpId.Value);
        if (result)
        {
            WriteLog.Write(CurrentUserName, "签退", "员工签退打卡");
            lblStatus.Text = "签退成功！时间：" + DateTime.Now.ToString("HH:mm:ss");
            lblError.Text = "";
        }
        else
        {
            lblError.Text = "签退失败：今天还没有签到记录！";
        }
        BindTodayRecord();
    }

    private void BindTodayRecord()
    {
        if (!CurrentEmpId.HasValue) return;

        string sql = @"SELECT e.EmpName, a.AttDate, a.CheckInTime, a.CheckOutTime, a.Status
                       FROM Attendance a
                       JOIN Employee e ON a.EmpId = e.EmpId
                       WHERE a.EmpId = @EmpId AND a.AttDate = @AttDate
                       ORDER BY a.AttDate";

        var dt = DBHelper.ExecuteDataTable(sql,
            new SqlParameter[] {
                new SqlParameter("@EmpId", CurrentEmpId.Value),
                new SqlParameter("@AttDate", DateTime.Today)
            });

        gvTodayRecord.DataSource = dt;
        gvTodayRecord.DataBind();
    }
}