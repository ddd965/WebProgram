using System;

namespace HRMS.Common
{
    /// <summary>
    /// 所有业务页面的基类 — 负责 Session 登录验证
    /// 用法：业务 .aspx.cs 继承此类，而不是直接继承 System.Web.UI.Page
    /// </summary>
    public class BasePage : System.Web.UI.Page
    {
        /// <summary>
        /// 当前登录用户名
        /// </summary>
        public string CurrentUserName
        {
            get { return Session["UserName"]?.ToString(); }
        }

        /// <summary>
        /// 当前登录用户角色
        /// </summary>
        public string CurrentUserRole
        {
            get { return Session["RoleName"]?.ToString(); }
        }

        /// <summary>
        /// 当前登录用户关联的员工 ID
        /// </summary>
        public int? CurrentEmpId
        {
            get { return Session["EmpId"] as int?; }
        }

        /// <summary>
        /// 页面加载前自动校验登录状态，未登录则跳转到 Login.aspx
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // 跳过登录页自身
            string page = System.IO.Path.GetFileName(Request.Path).ToLower();
            if (page == "login.aspx") return;

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx");
            }
        }

        /// <summary>
        /// 检查当前用户是否属于指定角色
        /// </summary>
        protected bool IsInRole(params string[] roles)
        {
            var currentRole = CurrentUserRole;
            if (string.IsNullOrEmpty(currentRole)) return false;
            foreach (var role in roles)
            {
                if (currentRole == role) return true;
            }
            return false;
        }
    }
}
