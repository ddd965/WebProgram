using System;
using System.Web.UI;

public partial class Account_Login : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            RegisterHyperLink.Visible = false;
        }
    }

    protected void LogIn(object sender, EventArgs e)
    {
        if (!IsValid) return;

        var user = HRMS.BLL.UserBLL.Login(UserName.Text.Trim(), Password.Text);
        if (user != null)
        {
            Session["UserId"] = user.UserId;
            Session["UserName"] = user.UserName;
            Session["RoleName"] = user.RoleName;
            Session["EmpId"] = user.EmpId;

            HRMS.BLL.WriteLog.Write(user.UserName, "登录", $"用户 {user.UserName} 成功登录系统");
            Response.Redirect("~/Default.aspx");
        }
        else
        {
            FailureText.Text = "用户名或密码错误，或账号已被锁定。";
            ErrorMessage.Visible = true;
        }
    }
}
