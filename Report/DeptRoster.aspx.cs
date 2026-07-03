using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using HRMS.BLL;
using HRMS.Model;

public partial class Report_DeptRoster : HRMS.Common.BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            LoadDropdowns();
            litReportDate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            GenerateRoster();
        }
    }

    private void LoadDropdowns()
    {
        var depts = DepartmentBLL.GetAll();
        ddlDept.Items.Clear();
        ddlDept.Items.Add(new ListItem("全部部门（分组显示）", ""));
        foreach (var d in depts)
            ddlDept.Items.Add(new ListItem(d.DeptName, d.DeptId.ToString()));
    }

    protected string GetPositionName(object positionId)
    {
        if (positionId == null || positionId == DBNull.Value) return "—";
        var p = PositionBLL.GetById(Convert.ToInt32(positionId));
        return p == null ? "—" : (p.PositionName ?? "—");
    }

    private List<Department> _deptList;
    private Dictionary<int, List<Employee>> _empByDept;
    private List<Employee> _unassigned;

    private void BuildData(out int totalCount)
    {
        totalCount = 0;
        string status = ddlStatus.SelectedValue;
        int? singleDept = null;
        if (!string.IsNullOrEmpty(ddlDept.SelectedValue))
            singleDept = int.Parse(ddlDept.SelectedValue);

        var allDepts = DepartmentBLL.GetAll();
        if (singleDept.HasValue)
            _deptList = allDepts.Where(d => d.DeptId == singleDept.Value).ToList();
        else
            _deptList = allDepts;

        var allEmps = EmployeeBLL.Query(null, null, null, null,
            string.IsNullOrEmpty(status) ? null : status);

        _empByDept = new Dictionary<int, List<Employee>>();
        _unassigned = new List<Employee>();
        foreach (var emp in allEmps)
        {
            if (emp.DeptId.HasValue && _deptList.Exists(d => d.DeptId == emp.DeptId.Value))
            {
                if (!_empByDept.ContainsKey(emp.DeptId.Value))
                    _empByDept[emp.DeptId.Value] = new List<Employee>();
                _empByDept[emp.DeptId.Value].Add(emp);
            }
            else if (!singleDept.HasValue)
            {
                _unassigned.Add(emp);
            }
        }
        totalCount = allEmps.Count;
    }

    private void GenerateRoster()
    {
        int total;
        BuildData(out total);
        litTotal.Text = total.ToString();
        litDeptCount.Text = (_deptList.Count + (_unassigned.Count > 0 ? 1 : 0)).ToString();

        var source = _deptList.Cast<object>().ToList();
        if (_unassigned.Count > 0)
        {
            source.Add(new Department
            {
                DeptId = 0,
                DeptName = "未分配部门",
                Descn = "暂无所属部门的员工",
                ParentId = null,
                ManagerId = null,
                CreateTime = DateTime.Now
            });
        }
        rptDepts.DataSource = source;
        rptDepts.DataBind();
    }

    protected void rptDepts_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

        var dept = e.Item.DataItem as Department;
        if (dept == null) return;

        var litName = e.Item.FindControl("litDeptName") as Literal;
        var litMgr = e.Item.FindControl("litManager") as Literal;
        var litCnt = e.Item.FindControl("litCount") as Literal;
        var gv = e.Item.FindControl("gvEmps") as GridView;
        if (litName == null || litMgr == null || litCnt == null || gv == null) return;

        litName.Text = dept.DeptName + " [" + dept.DeptId + "]";

        if (dept.ManagerId.HasValue && dept.ManagerId.Value > 0)
        {
            var mgr = EmployeeBLL.GetById(dept.ManagerId.Value);
            litMgr.Text = mgr != null ? (mgr.EmpName + " (" + mgr.EmpNo + ")") : "—";
        }
        else
        {
            litMgr.Text = "（未指定）";
        }

        List<Employee> emps;
        if (dept.DeptId == 0)
            emps = _unassigned;
        else
            emps = _empByDept.ContainsKey(dept.DeptId) ? _empByDept[dept.DeptId] : new List<Employee>();

        litCnt.Text = emps.Count.ToString();
        gv.DataSource = emps.OrderBy(x => x.EmpNo).ToList();
        gv.DataBind();
    }

    protected void btnGen_Click(object sender, EventArgs e)
    {
        GenerateRoster();
    }

    protected void btnCsv_Click(object sender, EventArgs e)
    {
        int total;
        BuildData(out total);
        var sb = new StringBuilder();
        sb.AppendLine("部门,工号,姓名,性别,职位,电话,邮箱,入职日期,状态");

        Action<Department, List<Employee>> writeDept = (dept, emps) =>
        {
            foreach (var emp in emps.OrderBy(x => x.EmpNo))
            {
                string pos = "—";
                if (emp.PositionId.HasValue)
                {
                    var p = PositionBLL.GetById(emp.PositionId.Value);
                    if (p != null) pos = p.PositionName;
                }
                sb.AppendFormat("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\"\r\n",
                    dept.DeptName,
                    emp.EmpNo, emp.EmpName, emp.Gender, pos,
                    emp.Phone, emp.Email,
                    emp.HireDate.ToString("yyyy-MM-dd"),
                    emp.Status);
            }
        };
        foreach (var d in _deptList)
        {
            var list = _empByDept.ContainsKey(d.DeptId) ? _empByDept[d.DeptId] : new List<Employee>();
            writeDept(d, list);
        }
        if (_unassigned.Count > 0)
            writeDept(new Department { DeptName = "未分配部门" }, _unassigned);

        byte[] buf = Encoding.UTF8.GetBytes(sb.ToString());
        byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
        Response.Clear();
        Response.ContentType = "text/csv; charset=utf-8";
        Response.AddHeader("Content-Disposition",
            "attachment; filename=DeptRoster_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv");
        Response.BinaryWrite(bom);
        Response.BinaryWrite(buf);
        Response.End();
    }
}
