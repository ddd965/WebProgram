<%@ Page Title="部门人员清单报表" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="DeptRoster.aspx.cs" Inherits="Report_DeptRoster" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>部门人员清单报表</h2>
    <hr />

    <style>
        .dept-block { page-break-inside: avoid; margin-bottom: 30px; }
        .dept-block h3 {
            border-left: 5px solid #337ab7;
            padding-left: 10px;
            margin-top: 0;
            color: #333;
        }
        @media print {
            .dept-block { page-break-after: always; }
        }
    </style>

    <div class="panel panel-default no-print">
        <div class="panel-heading">筛选条件</div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group"><label>部门</label>
                        <asp:DropDownList runat="server" ID="ddlDept" CssClass="form-control">
                            <asp:ListItem Value="">全部部门（分组显示）</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group"><label>员工状态</label>
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
                        <asp:Button runat="server" ID="btnGen" Text="生成清单" OnClick="btnGen_Click" CssClass="btn btn-primary btn-block" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnPrint" Text="打印" OnClientClick="window.print();return false;" CssClass="btn btn-info btn-block" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnCsv" Text="导出 CSV" OnClick="btnCsv_Click" CssClass="btn btn-success btn-block" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="report-header text-center" style="page-break-inside:avoid;">
        <h3>部门人员清单</h3>
        <p style="color:#666;">
            打印日期：<asp:Literal runat="server" ID="litReportDate" />
            &nbsp;&nbsp;|&nbsp;&nbsp;
            部门数：<asp:Literal runat="server" ID="litDeptCount" />
            &nbsp;&nbsp;|&nbsp;&nbsp;
            总人数：<asp:Literal runat="server" ID="litTotal" />
        </p>
    </div>

    <asp:Repeater runat="server" ID="rptDepts" OnItemDataBound="rptDepts_ItemDataBound">
        <ItemTemplate>
            <div class="dept-block panel panel-default">
                <div class="panel-heading">
                    <h3>
                        <asp:Literal runat="server" ID="litDeptName" />
                        <small style="margin-left:15px;">
                            部门经理：<asp:Literal runat="server" ID="litManager" />
                            &nbsp;&nbsp;|&nbsp;&nbsp;
                            人数：<asp:Literal runat="server" ID="litCount" />
                        </small>
                    </h3>
                </div>
                <div class="panel-body" style="padding:10px;">
                    <asp:GridView runat="server" ID="gvEmps" CssClass="table table-striped table-bordered table-condensed"
                        AutoGenerateColumns="False" ShowHeaderWhenEmpty="True" Width="100%">
                        <EmptyDataTemplate><p class="text-muted" style="margin:15px 0 0;">（该部门暂无员工）</p></EmptyDataTemplate>
                        <Columns>
                            <asp:BoundField DataField="EmpNo" HeaderText="工号" ItemStyle-Width="90" />
                            <asp:BoundField DataField="EmpName" HeaderText="姓名" />
                            <asp:BoundField DataField="Gender" HeaderText="性别" ItemStyle-Width="50" />
                            <asp:TemplateField HeaderText="职位">
                                <ItemTemplate><%# GetPositionName(Eval("PositionId")) %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Phone" HeaderText="电话" />
                            <asp:BoundField DataField="Email" HeaderText="邮箱" />
                            <asp:BoundField DataField="HireDate" HeaderText="入职日期" DataFormatString="{0:yyyy-MM-dd}" ItemStyle-Width="100" />
                            <asp:BoundField DataField="Status" HeaderText="状态" ItemStyle-Width="70" />
                        </Columns>
                        <PagerStyle CssClass="no-print" />
                    </asp:GridView>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />
</asp:Content>
