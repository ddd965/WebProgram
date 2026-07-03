<%@ Page Title="休假/加班统计报表" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="LeaveOvertimeRpt.aspx.cs" Inherits="Report_LeaveOvertimeRpt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>休假 / 加班 统计报表</h2>
    <hr />

    <div class="panel panel-default no-print">
        <div class="panel-heading">查询条件</div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-2">
                    <div class="form-group"><label>类型</label>
                        <asp:DropDownList runat="server" ID="ddlRptType" CssClass="form-control">
                            <asp:ListItem Text="休假统计" Value="leave" Selected="True" />
                            <asp:ListItem Text="加班统计" Value="ot" />
                        </asp:DropDownList>
                    </div>
                </div>
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
                        <asp:DropDownList runat="server" ID="ddlMonth" CssClass="form-control">
                            <asp:ListItem Value="0">全年</asp:ListItem>
                            <asp:ListItem Value="1">1月</asp:ListItem>
                            <asp:ListItem Value="2">2月</asp:ListItem>
                            <asp:ListItem Value="3">3月</asp:ListItem>
                            <asp:ListItem Value="4">4月</asp:ListItem>
                            <asp:ListItem Value="5">5月</asp:ListItem>
                            <asp:ListItem Value="6">6月</asp:ListItem>
                            <asp:ListItem Value="7">7月</asp:ListItem>
                            <asp:ListItem Value="8">8月</asp:ListItem>
                            <asp:ListItem Value="9">9月</asp:ListItem>
                            <asp:ListItem Value="10">10月</asp:ListItem>
                            <asp:ListItem Value="11">11月</asp:ListItem>
                            <asp:ListItem Value="12">12月</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnGen" Text="生成报表" OnClick="btnGen_Click" CssClass="btn btn-primary" />
                        <asp:Button runat="server" ID="btnPrint" Text="在线打印"
                            OnClientClick="window.print();return false;" CssClass="btn btn-info" />
                        <asp:Button runat="server" ID="btnExport" Text="导出 CSV" OnClick="btnExport_Click" CssClass="btn btn-success" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <div class="table-responsive" style="page-break-inside:avoid;">
    <asp:GridView runat="server" ID="gvRpt" CssClass="table table-striped table-bordered table-condensed"
        AutoGenerateColumns="False" AllowPaging="False">
        <Columns>
            <asp:BoundField DataField="GroupKey" HeaderText="分组项（员工）" />
            <asp:BoundField DataField="ColA" HeaderText="正常/工作日(h或天)" DataFormatString="{0:0.#}" />
            <asp:BoundField DataField="ColB" HeaderText="周末/类型B" DataFormatString="{0:0.#}" />
            <asp:BoundField DataField="ColC" HeaderText="节假日/类型C" DataFormatString="{0:0.#}" />
            <asp:BoundField DataField="Total" HeaderText="合计" DataFormatString="{0:0.#}" ItemStyle-Font-Bold="true" />
            <asp:BoundField DataField="Count" HeaderText="次数" />
        </Columns>
        <FooterStyle BackColor="#f5f5f5" Font-Bold="true" />
    </asp:GridView>
    </div>

    <div class="text-muted small" style="margin-top:30px;">
        报表生成时间：<asp:Literal runat="server" ID="litGenTime" />
        &nbsp;&nbsp;|&nbsp;&nbsp;操作员：<asp:Literal runat="server" ID="litOperator" />
    </div>
</asp:Content>
