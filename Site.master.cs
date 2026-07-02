using System;
using System.Web.UI;

public partial class SiteMaster : MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string userName = Session["UserName"]?.ToString();
            string roleName = Session["RoleName"]?.ToString();

            if (string.IsNullOrEmpty(userName))
            {
                // 未登录：只显示登录链接
                litUserInfo.Text = "";
                navEmployee.Visible = false;
                navDepartment.Visible = false;
                navLeave.Visible = false;
                navAttendance.Visible = false;
                navOvertime.Visible = false;
                navSalary.Visible = false;
                navReport.Visible = false;
                navEventLog.Visible = false;
                btnLogout.Visible = false;
            }
            else
            {
                litUserInfo.Text = $"你好，{userName}（{roleName}）";

                // 根据角色显示菜单
                bool isAdmin = roleName == "管理员";
                bool isManager = roleName == "部门经理" || isAdmin;

                navEmployee.Visible = isAdmin;
                navDepartment.Visible = isAdmin;
                navEventLog.Visible = isAdmin;
                // 考勤/休假/加班对所有登录用户可见
                // 工资/报表对管理员和部门经理可见
                navSalary.Visible = isManager;
                navReport.Visible = isManager;
            }
        }
    }

    protected void btnLogout_Click(object sender, EventArgs e)
    {
        string userName = Session["UserName"]?.ToString();
        if (!string.IsNullOrEmpty(userName))
        {
            HRMS.BLL.WriteLog.Write(userName, "登出", $"用户 {userName} 退出系统");
        }
        Session.Abandon();
        Response.Redirect("~/Default.aspx");
    }
}
