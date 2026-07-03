using System;
using HRMS.BLL;
using HRMS.Model;

public partial class Department_DeptEdit : HRMS.Common.BasePage
{
    private string Mode
    {
        get { return Request.QueryString["mode"] ?? "add"; }
    }

    private int DeptId
    {
        get
        {
            int id;
            int.TryParse(Request.QueryString["id"], out id);
            return id;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            LoadDropdowns();

            if (Mode == "add")
            {
                litTitle.Text = "新增部门";
            }
            else if (Mode == "edit")
            {
                litTitle.Text = "编辑部门";
                LoadDepartment();
            }
        }
    }

    private void LoadDropdowns()
    {
        var allDepts = DepartmentBLL.GetAll();
        foreach (var d in allDepts)
            ddlParent.Items.Add(new System.Web.UI.WebControls.ListItem($"{d.DeptName} [{d.DeptId}]", d.DeptId.ToString()));

        var employees = EmployeeBLL.Query(null, null, null, null, "在职");
        foreach (var e in employees)
            ddlManager.Items.Add(new System.Web.UI.WebControls.ListItem($"{e.EmpName} ({e.EmpNo})", e.EmpId.ToString()));
    }

    private void LoadDepartment()
    {
        var dept = DepartmentBLL.GetById(DeptId);
        if (dept == null)
        {
            lblMsg.Text = "部门不存在！";
            return;
        }

        txtDeptName.Text = dept.DeptName;
        txtDescn.Text = dept.Descn;
        if (dept.ParentId.HasValue)
            ddlParent.SelectedValue = dept.ParentId.Value.ToString();
        if (dept.ManagerId.HasValue)
            ddlManager.SelectedValue = dept.ManagerId.Value.ToString();

        hidDeptId.Value = dept.DeptId.ToString();
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        var dept = new Department
        {
            DeptName = txtDeptName.Text.Trim(),
            // 按需求：所有部门平级，无上下级关系 → 强制 ParentId = null
            ParentId = null,
            ManagerId = string.IsNullOrEmpty(ddlManager.SelectedValue) ? (int?)null : int.Parse(ddlManager.SelectedValue),
            Descn = txtDescn.Text.Trim()
        };

        if (Mode == "add")
        {
            int newId = DepartmentBLL.Insert(dept);
            WriteLog.Write(CurrentUserName, "新增", $"新增部门 [{dept.DeptName}]");
            lblMsg.Text = $"部门添加成功，ID: {newId}";
            hidDeptId.Value = newId.ToString();
            litTitle.Text = "编辑部门";
        }
        else
        {
            dept.DeptId = DeptId;
            DepartmentBLL.Update(dept);
            WriteLog.Write(CurrentUserName, "修改", $"修改部门 [{dept.DeptName}]");
            lblMsg.Text = "部门信息已更新。";
        }
    }
}
