using System;

public partial class _Default : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["UserId"] != null)
        {
            btnLogin.Visible = false;
        }
    }
}
