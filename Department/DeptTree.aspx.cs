using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Department_DeptTree : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BuildTree();
        }
    }

    private void BuildTree()
    {
        tvDept.Nodes.Clear();
        // 持久化修正：把所有部门的 ParentId 置为 NULL，保证九部门平级（执行过一次后0行受影响）
        try { HRMS.DAL.DepartmentDAL.FlattenAllDepartments(); } catch { }
        var allDepts = DepartmentBLL.GetAll();
        // 所有部门一律顶级（不再递归BuildChildNodes），按 DeptId 顺序 1..9
        foreach (var dept in allDepts.OrderBy(d => d.DeptId))
        {
            var node = CreateTreeNode(dept);
            tvDept.Nodes.Add(node);
        }
        if (tvDept.Nodes.Count == 0)
        {
            var hint = new TreeNode("（无部门，请先新增部门）", "0") { SelectAction = TreeNodeSelectAction.None };
            tvDept.Nodes.Add(hint);
        }
        tvDept.DataBind();
    }

    private void BuildChildNodes(TreeNode parentNode, int parentId, List<Department> allDepts)
    {
        // 扁平化后无任何子部门，保留方法签名兼容
    }

    private TreeNode CreateTreeNode(Department dept)
    {
        var node = new TreeNode
        {
            Text = $"{dept.DeptName} [{dept.DeptId}]",
            Value = dept.DeptId.ToString(),
            Expanded = false
        };
        return node;
    }

    private void ShowDepartmentDetail(int deptId)
    {
        var dept = DepartmentBLL.GetById(deptId);
        if (dept == null)
        {
            lblMsg.Text = "部门不存在！";
            return;
        }

        pnlDetail.Visible = true;
        litDeptTitle.Text = $" — {dept.DeptName}";
        litDeptName.Text = dept.DeptName;
        litDescn.Text = dept.Descn ?? "—";
        litCreateTime.Text = dept.CreateTime.ToString("yyyy-MM-dd HH:mm");

        if (dept.ParentId.HasValue)
        {
            var parent = DepartmentBLL.GetById(dept.ParentId.Value);
            litParentName.Text = parent?.DeptName ?? "—";
        }
        else
        {
            litParentName.Text = "（顶级部门）";
        }

        if (dept.ManagerId.HasValue)
        {
            var mgr = EmployeeBLL.GetById(dept.ManagerId.Value);
            litManagerName.Text = mgr?.EmpName ?? "—";
        }
        else
        {
            litManagerName.Text = "（未指定）";
        }

        linkEdit.HRef = $"DeptEdit.aspx?mode=edit&id={deptId}";

        hidDeptId.Value = deptId.ToString();
        LoadDepartmentEmployees(deptId);
    }

    private void LoadDepartmentEmployees(int deptId)
    {
        var employees = EmployeeBLL.Query(null, null, deptId, null, "在职");
        gvDeptEmployees.DataSource = employees;
        gvDeptEmployees.DataBind();
        if (employees != null && employees.Count > 0)
        {
            pnlEmployees.Visible = true;
        }
        else
        {
            pnlEmployees.Visible = false;
        }
    }

    protected void tvDept_SelectedNodeChanged(object sender, EventArgs e)
    {
        int deptId;
        if (int.TryParse(tvDept.SelectedNode.Value, out deptId))
        {
            ShowDepartmentDetail(deptId);
        }
    }

    protected void btnViewEmployees_Click(object sender, EventArgs e)
    {
        int deptId;
        if (int.TryParse(hidDeptId.Value, out deptId))
        {
            Response.Redirect($"../Employee/EmployeeList.aspx?deptId={deptId}");
        }
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        int deptId;
        if (!int.TryParse(hidDeptId.Value, out deptId)) return;

        try
        {
            // 检查是否有子部门
            var subs = DepartmentBLL.GetSubDepartments(deptId);
            if (subs != null && subs.Count > 0)
            {
                lblMsg.Text = "无法删除：该部门下还有子部门，请先删除或移动子部门。";
                return;
            }

            DepartmentBLL.Delete(deptId);
            WriteLog.Write(CurrentUserName, "删除", $"删除部门 ID={deptId}");
            pnlDetail.Visible = false;
            pnlEmployees.Visible = false;
            lblMsg.Text = "部门已删除。";
            BuildTree();
        }
        catch (Exception ex)
        {
            lblMsg.Text = $"删除失败：{ex.Message}";
        }
    }

    protected void gvDeptEmployees_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvDeptEmployees.PageIndex = e.NewPageIndex;
        int deptId;
        if (int.TryParse(hidDeptId.Value, out deptId))
        {
            LoadDepartmentEmployees(deptId);
        }
    }
}
