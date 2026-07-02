<%@ Page Title="月度考勤汇总报表" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="AttendanceMonthly.aspx.cs" Inherits="Report_AttendanceMonthly" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>月度考勤汇总报表</h2>
    <hr />

    <div class="panel panel-default no-print">
        <div class="panel-heading">统计条件</div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group"><label>部门</label>
                        <asp:DropDownList runat="server" ID="ddlDept" CssClass="form-control">
                            <asp:ListItem Value="">全部部门</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label>年份</label>
                        <asp:DropDownList runat="server" ID="ddlYear" CssClass="form-control" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label>月份</label>
                        <asp:DropDownList runat="server" ID="ddlMonth" CssClass="form-control" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnStat" Text="生成报表" OnClick="btnStat_Click" CssClass="btn btn-primary btn-block" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnPrint" Text="打印报表"
                            OnClientClick="window.print();return false;" CssClass="btn btn-info btn-block" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="report-header text-center" style="page-break-inside:avoid;">
        <h3><asp:Literal runat="server" ID="litTitle" /></h3>
        <p style="color:#666;">
            生成时间：<asp:Literal runat="server" ID="litReportTime" />
            &nbsp;&nbsp;|&nbsp;&nbsp;
            共 <asp:Literal runat="server" ID="litTotal" /> 位员工
        </p>
    </div>

    <div class="table-responsive">
    <asp:GridView runat="server" ID="gvStat" CssClass="table table-striped table-bordered table-condensed"
        AutoGenerateColumns="False" AllowPaging="True" PageSize="50"
        OnPageIndexChanging="gvStat_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="EmpNo" HeaderText="工号" />
            <asp:BoundField DataField="EmpName" HeaderText="姓名" />
            <asp:BoundField DataField="DeptName" HeaderText="部门" />
            <asp:BoundField DataField="NormalDays" HeaderText="正常出勤" ItemStyle-CssClass="text-success" />
            <asp:BoundField DataField="LateCount" HeaderText="迟到次数" ItemStyle-CssClass="text-warning" />
            <asp:BoundField DataField="EarlyCount" HeaderText="早退次数" ItemStyle-CssClass="text-warning" />
            <asp:BoundField DataField="AbsentCount" HeaderText="缺勤天数" ItemStyle-CssClass="text-danger" />
            <asp:BoundField DataField="LeaveCount" HeaderText="请假天数" ItemStyle-CssClass="text-info" />
            <asp:TemplateField HeaderText="异常合计">
                <ItemTemplate>
                    <asp:Literal runat="server"
                        Text='<%# (int)Eval("LateCount") + (int)Eval("EarlyCount") + (int)Eval("AbsentCount") + (int)Eval("LeaveCount") %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <FooterStyle BackColor="#f5f5f5" Font-Bold="true" />
        <PagerStyle CssClass="pagination-ys no-print" HorizontalAlign="Center" />
    </asp:GridView>
    </div>
</asp:Content>
