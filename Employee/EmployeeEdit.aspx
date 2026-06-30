<%@ Page Title="员工编辑" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="EmployeeEdit.aspx.cs" Inherits="Employee_EmployeeEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><asp:Literal runat="server" ID="litTitle" /></h2>
    <hr />

    <asp:Label runat="server" ID="lblMsg" CssClass="text-danger" />

    <div class="row">
        <div class="col-md-8">
            <div class="form-horizontal">
                <div class="form-group">
                    <label class="col-md-3 control-label">工号 *</label>
                    <div class="col-md-9">
                        <asp:TextBox runat="server" ID="txtEmpNo" CssClass="form-control" MaxLength="20" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmpNo"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">姓名 *</label>
                    <div class="col-md-9">
                        <asp:TextBox runat="server" ID="txtEmpName" CssClass="form-control" MaxLength="50" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmpName"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">性别 *</label>
                    <div class="col-md-9">
                        <asp:RadioButtonList runat="server" ID="rblGender" RepeatDirection="Horizontal" CssClass="form-control-static">
                            <asp:ListItem Text="男" Value="男" Selected="True" />
                            <asp:ListItem Text="女" Value="女" />
                        </asp:RadioButtonList>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">出生日期</label>
                    <div class="col-md-9">
                        <asp:TextBox runat="server" ID="txtBirthday" CssClass="form-control" TextMode="Date" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">身份证号</label>
                    <div class="col-md-9">
                        <asp:TextBox runat="server" ID="txtIdCard" CssClass="form-control" MaxLength="18" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtIdCard"
                            CssClass="text-danger" Display="Dynamic"
                            ValidationExpression="(^\d{17}[\dXx]$)?" ErrorMessage="身份证号格式不正确" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">入职日期 *</label>
                    <div class="col-md-9">
                        <asp:TextBox runat="server" ID="txtHireDate" CssClass="form-control" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtHireDate"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">所属部门</label>
                    <div class="col-md-9">
                        <asp:DropDownList runat="server" ID="ddlDept" CssClass="form-control">
                            <asp:ListItem Value="">-- 请选择 --</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">职位</label>
                    <div class="col-md-9">
                        <asp:DropDownList runat="server" ID="ddlPosition" CssClass="form-control">
                            <asp:ListItem Value="">-- 请选择 --</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">电话 *</label>
                    <div class="col-md-9">
                        <asp:TextBox runat="server" ID="txtPhone" CssClass="form-control" MaxLength="20" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPhone"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">邮箱 *</label>
                    <div class="col-md-9">
                        <asp:TextBox runat="server" ID="txtEmail" CssClass="form-control" MaxLength="80" TextMode="Email" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">地址</label>
                    <div class="col-md-9">
                        <asp:TextBox runat="server" ID="txtAddress" CssClass="form-control" MaxLength="255" TextMode="MultiLine" Rows="2" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">状态</label>
                    <div class="col-md-9">
                        <asp:DropDownList runat="server" ID="ddlStatus" CssClass="form-control">
                            <asp:ListItem Text="在职" Value="在职" />
                            <asp:ListItem Text="离职" Value="离职" />
                            <asp:ListItem Text="试用期" Value="试用期" />
                            <asp:ListItem Text="实习期" Value="实习期" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-md-offset-3 col-md-9">
                        <asp:Button runat="server" ID="btnSave" Text="保存" OnClick="btnSave_Click" CssClass="btn btn-primary" />
                        <a href="EmployeeList.aspx" class="btn btn-default">返回列表</a>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="panel panel-default">
                <div class="panel-heading">员工照片</div>
                <div class="panel-body text-center">
                    <asp:Image runat="server" ID="imgPhoto" CssClass="img-thumbnail" style="max-width:200px;max-height:200px;" AlternateText="暂无照片" />
                    <hr />
                    <asp:FileUpload runat="server" ID="fuPhoto" CssClass="form-control" />
                    <small class="text-muted">支持 jpg/png/gif，大小不超过 2MB</small>
                    <br />
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="fuPhoto"
                        CssClass="text-danger" Display="Dynamic"
                        ValidationExpression="^.*\.(jpg|JPG|jpeg|JPEG|png|PNG|gif|GIF)$"
                        ErrorMessage="仅支持 jpg/png/gif 格式" />
                </div>
            </div>
        </div>
    </div>

    <asp:HiddenField runat="server" ID="hidMode" />
    <asp:HiddenField runat="server" ID="hidEmpId" />
    <asp:HiddenField runat="server" ID="hidOldPhoto" />
</asp:Content>
