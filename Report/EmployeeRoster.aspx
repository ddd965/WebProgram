<%@ Page Title="员工花名册报表" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="EmployeeRoster.aspx.cs" Inherits="Report_EmployeeRoster" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>员工花名册报表</h2>
    <hr />

    <style>
        .roster-photo { width: 60px; height: 60px; object-fit: cover; border:1px solid #ddd; }
        @@media print {
            .roster-photo { width: 50px; height: 50px; }
        }
    </style>

    <div class="panel panel-default no-print">
        <div class="panel-heading">筛选条件</div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group"><label>部门</label>
                        <asp:DropDownList runat="server" ID="ddlDept" CssClass="form-control">
                            <asp:ListItem Value="">全部部门</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group"><label>职位</label>
                        <asp:DropDownList runat="server" ID="ddlPosition" CssClass="form-control">
                            <asp:ListItem Value="">全部职位</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label>状态</label>
                        <asp:DropDownList runat="server" ID="ddlStatus" CssClass="form-control">
                            <asp:ListItem Value="">全部</asp:ListItem>
                            <asp:ListItem Text="在职" Value="在职" Selected="True" />
                            <asp:ListItem Text="离职" Value="离职" />
                            <asp:ListItem Text="试用期" Value="试用期" />
                            <asp:ListItem Text="实习期" Value="实习期" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnSearch" Text="筛选" OnClick="btnSearch_Click" CssClass="btn btn-primary btn-block" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnPrint" Text="打印花名册"
                            OnClientClick="window.print();return false;" CssClass="btn btn-info btn-block" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="report-header text-center" style="page-break-inside:avoid;">
        <h3>员工花名册</h3>
        <p style="color:#666;">
            统计日期：<asp:Literal runat="server" ID="litReportDate" />
            &nbsp;&nbsp;|&nbsp;&nbsp;
            总人数：<asp:Literal runat="server" ID="litTotal" />
        </p>
    </div>

    <div class="table-responsive">
    <asp:GridView runat="server" ID="gvRoster" CssClass="table table-striped table-bordered table-condensed"
        AutoGenerateColumns="False" AllowPaging="True" PageSize="50"
        OnPageIndexChanging="gvRoster_PageIndexChanging">
        <Columns>
            <asp:TemplateField HeaderText="头像" ItemStyle-Width="70">
                <ItemTemplate>
                    <asp:Image runat="server" AlternateText="无" CssClass="roster-photo"
                        ImageUrl='<%# Eval("PhotoPath") == null || Eval("PhotoPath").ToString()==""
                            ? "data:image/svg+xml;utf8,<svg xmlns=%22http://www.w3.org/2000/svg%22 width=%2260%22 height=%2260%22><rect fill=%22%23eee%22 width=%2260%22 height=%2260%22/><text x=%2230%22 y=%2235%22 text-anchor=%22middle%22 fill=%22%23aaa%22 font-size=%2220%22>?</text></svg>"
                            : Eval("PhotoPath") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="EmpId" HeaderText="ID" ItemStyle-Width="40" />
            <asp:BoundField DataField="EmpNo" HeaderText="工号" />
            <asp:BoundField DataField="EmpName" HeaderText="姓名" />
            <asp:BoundField DataField="Gender" HeaderText="性别" ItemStyle-Width="40" />
            <asp:BoundField DataField="Birthday" HeaderText="生日" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:TemplateField HeaderText="部门">
                <ItemTemplate><%# GetDeptName(Eval("DeptId")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="职位">
                <ItemTemplate><%# GetPosName(Eval("PositionId")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Phone" HeaderText="电话" />
            <asp:BoundField DataField="Email" HeaderText="邮箱" />
            <asp:BoundField DataField="HireDate" HeaderText="入职日期" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="Status" HeaderText="状态" />
        </Columns>
        <PagerStyle CssClass="pagination-ys no-print" HorizontalAlign="Center" />
    </asp:GridView>
    </div>
</asp:Content>
