<%@ Page Title="员工管理" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="EmployeeList.aspx.cs" Inherits="Employee_EmployeeList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>员工管理</h2>
    <hr />

    <div class="panel panel-default">
        <div class="panel-heading">查询条件</div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group">
                        <label>工号</label>
                        <asp:TextBox runat="server" ID="txtEmpNo" CssClass="form-control" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>姓名</label>
                        <asp:TextBox runat="server" ID="txtEmpName" CssClass="form-control" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>部门</label>
                        <asp:DropDownList runat="server" ID="ddlDept" CssClass="form-control">
                            <asp:ListItem Value="">全部</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>状态</label>
                        <asp:DropDownList runat="server" ID="ddlStatus" CssClass="form-control">
                            <asp:ListItem Value="">全部</asp:ListItem>
                            <asp:ListItem Text="在职" Value="在职" />
                            <asp:ListItem Text="离职" Value="离职" />
                            <asp:ListItem Text="试用期" Value="试用期" />
                            <asp:ListItem Text="实习期" Value="实习期" />
                        </asp:DropDownList>
                    </div>
                </div>
            </div>
            <asp:Button runat="server" ID="btnSearch" Text="查询" OnClick="btnSearch_Click" CssClass="btn btn-primary" />
            <asp:Button runat="server" ID="btnReset" Text="重置" OnClick="btnReset_Click" CssClass="btn btn-default" />
            <a href="EmployeeEdit.aspx?mode=add" class="btn btn-success">新增员工</a>
        </div>
    </div>

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <div class="table-responsive">
    <asp:GridView runat="server" ID="gvEmployee" CssClass="table table-striped table-bordered table-condensed"
        AutoGenerateColumns="False" DataKeyNames="EmpId"
        OnRowCommand="gvEmployee_RowCommand"
        AllowPaging="True" PageSize="10" OnPageIndexChanging="gvEmployee_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="EmpId" HeaderText="ID" ItemStyle-Width="50" />
            <asp:BoundField DataField="EmpNo" HeaderText="工号" />
            <asp:BoundField DataField="EmpName" HeaderText="姓名" />
            <asp:BoundField DataField="Gender" HeaderText="性别" ItemStyle-Width="50" />
            <asp:TemplateField HeaderText="部门">
                <ItemTemplate><%# GetDeptName(Eval("DeptId")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="职位">
                <ItemTemplate><%# GetPositionName(Eval("PositionId")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Phone" HeaderText="电话" />
            <asp:BoundField DataField="HireDate" HeaderText="入职日期" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="Status" HeaderText="状态" />
            <asp:TemplateField HeaderText="操作" ItemStyle-Width="180">
                <ItemTemplate>
                    <a href='EmployeeEdit.aspx?mode=edit&id=<%# Eval("EmpId") %>' class="btn btn-xs btn-info">编辑</a>
                    <asp:LinkButton runat="server" CommandName="DeleteRow" CommandArgument='<%# Eval("EmpId") %>'
                        CssClass="btn btn-xs btn-danger" Text="删除"
                        OnClientClick="return confirm('确认删除该员工？级联将清除其账号、休假、考勤、加班、工资记录。')" />
                    <%# Eval("Status").ToString() == "在职"
                        ? "<a href='EmployeeEdit.aspx?mode=resign&id=" + Eval("EmpId") + "' class='btn btn-xs btn-warning'>离职</a>"
                        : "" %>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
    </asp:GridView>
    </div>
</asp:Content>
