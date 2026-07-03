using System;
using System.Web.UI;

public partial class Account_Login : Page
{
    private const string CaptchaSessionKey = "LoginCaptchaAnswer";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            RegisterHyperLink.Visible = false;
            GenerateCaptcha();
        }
    }

    private void GenerateCaptcha()
    {
        var rnd = new Random(Guid.NewGuid().GetHashCode());
        int a = rnd.Next(3, 20);
        int b = rnd.Next(2, 15);
        int op = rnd.Next(0, 3); // 0=+, 1=-, 2=x
        int answer;
        string expr;
        switch (op)
        {
            case 1:
                if (a < b) { int t = a; a = b; b = t; }
                answer = a - b;
                expr = a + "  -  " + b + "  =  ?";
                break;
            case 2:
                a = rnd.Next(2, 10);
                b = rnd.Next(2, 10);
                answer = a * b;
                expr = a + "  x  " + b + "  =  ?";
                break;
            default:
                answer = a + b;
                expr = a + "  +  " + b + "  =  ?";
                break;
        }
        Session[CaptchaSessionKey] = answer.ToString();
        litCaptcha.Text = expr;
    }

    protected void lbtnRefreshCaptcha_Click(object sender, EventArgs e)
    {
        GenerateCaptcha();
        txtCaptcha.Text = "";
    }

    protected void LogIn(object sender, EventArgs e)
    {
        if (!IsValid) return;

        // 验证码校验
        string expected = Session[CaptchaSessionKey] as string;
        string input = (txtCaptcha.Text ?? "").Trim();
        if (string.IsNullOrEmpty(expected) || expected != input)
        {
            FailureText.Text = "验证码错误，请重新计算后输入。";
            ErrorMessage.Visible = true;
            GenerateCaptcha();  // 失败换一题
            txtCaptcha.Text = "";
            return;
        }
        // 一次性验证码，立即过期
        Session[CaptchaSessionKey] = null;

        var user = HRMS.BLL.UserBLL.Login(UserName.Text.Trim(), Password.Text);
        if (user != null)
        {
            Session["UserId"] = user.UserId;
            Session["UserName"] = user.UserName;
            Session["RoleName"] = user.RoleName;
            Session["EmpId"] = user.EmpId;

            HRMS.BLL.WriteLog.Write(user.UserName, "登录", "用户 " + user.UserName + " 成功登录系统");
            Response.Redirect("~/Default.aspx");
        }
        else
        {
            // 登录失败也写日志
            HRMS.BLL.WriteLog.Write(UserName.Text.Trim(), "登录失败", "账号 " + UserName.Text.Trim() + " 尝试登录，密码错误或账号已锁定");
            FailureText.Text = "用户名或密码错误，或账号已被锁定。";
            ErrorMessage.Visible = true;
            GenerateCaptcha();  // 失败换一题
            txtCaptcha.Text = "";
        }
    }
}
