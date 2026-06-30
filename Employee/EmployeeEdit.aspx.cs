using System;
using System.IO;
using HRMS.BLL;
using HRMS.Model;

public partial class Employee_EmployeeEdit : HRMS.Common.BasePage
{
    private string Mode
    {
        get { return Request.QueryString["mode"] ?? "add"; }
    }

    private int EmpId
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
                litTitle.Text = "新增员工";
                txtHireDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            }
            else if (Mode == "edit" || Mode == "resign")
            {
                litTitle.Text = Mode == "resign" ? "员工离职确认" : "编辑员工信息";
                LoadEmployee();
                if (Mode == "resign")
                    DisableForm();
            }
        }
    }

    private void LoadDropdowns()
    {
        var depts = DepartmentBLL.GetAll();
        foreach (var d in depts)
            ddlDept.Items.Add(new System.Web.UI.WebControls.ListItem(d.DeptName, d.DeptId.ToString()));

        var positions = PositionBLL.GetAll();
        foreach (var p in positions)
            ddlPosition.Items.Add(new System.Web.UI.WebControls.ListItem($"{p.PositionName} (Lv.{p.Level})", p.PositionId.ToString()));
    }

    private void LoadEmployee()
    {
        var emp = EmployeeBLL.GetById(EmpId);
        if (emp == null)
        {
            lblMsg.Text = "员工不存在！";
            return;
        }

        txtEmpNo.Text = emp.EmpNo;
        txtEmpName.Text = emp.EmpName;
        rblGender.SelectedValue = emp.Gender;
        txtBirthday.Text = emp.Birthday?.ToString("yyyy-MM-dd");
        txtIdCard.Text = emp.IdCard;
        txtHireDate.Text = emp.HireDate.ToString("yyyy-MM-dd");
        if (emp.DeptId.HasValue) ddlDept.SelectedValue = emp.DeptId.Value.ToString();
        if (emp.PositionId.HasValue) ddlPosition.SelectedValue = emp.PositionId.Value.ToString();
        txtPhone.Text = emp.Phone;
        txtEmail.Text = emp.Email;
        txtAddress.Text = emp.Address;
        if (emp.Status != null) ddlStatus.SelectedValue = emp.Status;

        if (!string.IsNullOrEmpty(emp.PhotoPath))
        {
            imgPhoto.ImageUrl = emp.PhotoPath;
            hidOldPhoto.Value = emp.PhotoPath;
        }

        hidEmpId.Value = emp.EmpId.ToString();
        hidMode.Value = Mode;
    }

    private void DisableForm()
    {
        txtEmpNo.Enabled = false;
        txtEmpName.Enabled = false;
        rblGender.Enabled = false;
        txtBirthday.Enabled = false;
        txtIdCard.Enabled = false;
        txtHireDate.Enabled = false;
        ddlDept.Enabled = false;
        ddlPosition.Enabled = false;
        txtPhone.Enabled = false;
        txtEmail.Enabled = false;
        txtAddress.Enabled = false;
        fuPhoto.Enabled = false;
        btnSave.Text = "确认离职";
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        string mode = hidMode.Value;
        if (string.IsNullOrEmpty(mode)) mode = Mode;

        // 离职处理
        if (mode == "resign")
        {
            int empId;
            if (int.TryParse(hidEmpId.Value, out empId) && empId > 0)
            {
                EmployeeBLL.SetResigned(empId);
                WriteLog.Write(CurrentUserName, "修改", $"将员工 ID={empId} 状态设为离职");
                Response.Redirect("EmployeeList.aspx");
            }
            return;
        }

        // 照片处理
        string photoPath = hidOldPhoto.Value ?? "";
        if (fuPhoto.HasFile)
        {
            if (fuPhoto.PostedFile.ContentLength > 2 * 1024 * 1024)
            {
                lblMsg.Text = "照片不能超过 2MB！";
                return;
            }
            string ext = Path.GetExtension(fuPhoto.FileName).ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif")
            {
                lblMsg.Text = "仅支持 jpg/png/gif 格式！";
                return;
            }
            string uploadDir = Server.MapPath("~/Uploads/Photos/");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
            string fileName = Guid.NewGuid().ToString("N") + ext;
            fuPhoto.SaveAs(Path.Combine(uploadDir, fileName));
            photoPath = "~/Uploads/Photos/" + fileName;
        }

        var emp = new Employee
        {
            EmpNo = txtEmpNo.Text.Trim(),
            EmpName = txtEmpName.Text.Trim(),
            Gender = rblGender.SelectedValue,
            Birthday = string.IsNullOrEmpty(txtBirthday.Text) ? (DateTime?)null : DateTime.Parse(txtBirthday.Text),
            IdCard = txtIdCard.Text.Trim(),
            HireDate = DateTime.Parse(txtHireDate.Text),
            DeptId = string.IsNullOrEmpty(ddlDept.SelectedValue) ? (int?)null : int.Parse(ddlDept.SelectedValue),
            PositionId = string.IsNullOrEmpty(ddlPosition.SelectedValue) ? (int?)null : int.Parse(ddlPosition.SelectedValue),
            Phone = txtPhone.Text.Trim(),
            Email = txtEmail.Text.Trim(),
            Address = txtAddress.Text.Trim(),
            PhotoPath = photoPath,
            Status = ddlStatus.SelectedValue
        };

        if (mode == "add")
        {
            int newId = EmployeeBLL.Insert(emp);
            WriteLog.Write(CurrentUserName, "新增", $"新增员工 [{emp.EmpNo}] {emp.EmpName}");
            lblMsg.Text = $"员工添加成功，ID: {newId}";
            hidEmpId.Value = newId.ToString();
            hidMode.Value = "edit";
            litTitle.Text = "编辑员工信息";
        }
        else
        {
            emp.EmpId = EmpId;
            EmployeeBLL.Update(emp);
            WriteLog.Write(CurrentUserName, "修改", $"修改员工 [{emp.EmpNo}] {emp.EmpName}");
            lblMsg.Text = "员工信息已更新。";
        }
    }
}
