using System;
using System.Collections.Generic;
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

    /// <summary>
    /// 递归获取指定部门的所有后代部门 ID（含子/孙/...），用于防止循环父子引用
    /// </summary>
    private static HashSet<int> GetAllDescendantIds(int deptId)
    {
        var result = new HashSet<int>();
        var subs = DepartmentBLL.GetSubDepartments(deptId);
        if (subs == null) return result;
        foreach (var s in subs)
        {
            result.Add(s.DeptId);
            foreach (var subId in GetAllDescendantIds(s.DeptId))
                result.Add(subId);
        }
        return result;
    }

    private void LoadDropdowns()
    {
        var allDepts = DepartmentBLL.GetAll();
        // 编辑模式：上级部门下拉里移除当前部门自己和所有后代（防止循环引用）
        var forbiddenIds = Mode == "edit" ? GetAllDescendantIds(DeptId) : new HashSet<int>();
        if (Mode == "edit") forbiddenIds.Add(DeptId);

        foreach (var d in allDepts)
        {
            if (forbiddenIds.Contains(d.DeptId)) continue;
            ddlParent.Items.Add(new System.Web.UI.WebControls.ListItem($"{d.DeptName} [{d.DeptId}]", d.DeptId.ToString()));
        }

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
        {
            // 编辑模式若上级被移出下拉（比如因层级变更），依然要能正确回显
            var li = ddlParent.Items.FindByValue(dept.ParentId.Value.ToString());
            if (li != null)
                ddlParent.SelectedValue = dept.ParentId.Value.ToString();
            else
            {
                var parent = DepartmentBLL.GetById(dept.ParentId.Value);
                if (parent != null)
                {
                    ddlParent.Items.Add(new System.Web.UI.WebControls.ListItem(
                        $"{parent.DeptName} [{parent.DeptId}]（当前上级）", parent.DeptId.ToString()));
                    ddlParent.SelectedValue = parent.DeptId.ToString();
                }
            }
        }
        if (dept.ManagerId.HasValue)
            ddlManager.SelectedValue = dept.ManagerId.Value.ToString();

        hidDeptId.Value = dept.DeptId.ToString();
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        // 读用户选择的上级部门（不再强制平级，允许自由修改上下级）
        int? newParentId = string.IsNullOrEmpty(ddlParent.SelectedValue)
            ? (int?)null
            : int.Parse(ddlParent.SelectedValue);

        // 编辑模式：循环父子引用校验 —— 新上级不能是自己 / 自己的任何后代
        if (Mode == "edit")
        {
            if (newParentId == DeptId)
            {
                lblMsg.CssClass = "text-danger";
                lblMsg.Text = "⚠ 保存失败：上级部门不能选择当前部门自己。";
                return;
            }
            if (newParentId.HasValue && GetAllDescendantIds(DeptId).Contains(newParentId.Value))
            {
                lblMsg.CssClass = "text-danger";
                lblMsg.Text = "⚠ 保存失败：新上级是当前部门的下级部门，将造成循环引用，请选择其他上级。";
                return;
            }
        }

        var dept = new Department
        {
            DeptName = txtDeptName.Text.Trim(),
            ParentId = newParentId,    // ★ 改：用户自由选，不再硬编码 null
            ManagerId = string.IsNullOrEmpty(ddlManager.SelectedValue) ? (int?)null : int.Parse(ddlManager.SelectedValue),
            Descn = txtDescn.Text.Trim()
        };

        if (Mode == "add")
        {
            int newId = DepartmentBLL.Insert(dept);
            WriteLog.Write(CurrentUserName, "新增",
                $"新增部门 [{dept.DeptName}]，上级={(newParentId.HasValue ? newParentId.Value.ToString() : "顶级")}");
            lblMsg.CssClass = "text-info";
            lblMsg.Text = $"部门添加成功，ID: {newId}";
            hidDeptId.Value = newId.ToString();
            litTitle.Text = "编辑部门";
        }
        else
        {
            dept.DeptId = DeptId;
            DepartmentBLL.Update(dept);
            WriteLog.Write(CurrentUserName, "修改",
                $"修改部门 [{dept.DeptName}]，新上级={(newParentId.HasValue ? newParentId.Value.ToString() : "顶级")}");
            lblMsg.CssClass = "text-info";
            lblMsg.Text = "部门信息已更新。返回部门树即可看到层级变化。";
        }
    }
}
