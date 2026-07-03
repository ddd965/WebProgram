<%@ Page Title="考勤明细查询" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="AttendanceDetail.aspx.cs" Inherits="Attendance_AttendanceDetail" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>考勤明细查询（按月列表 + 日历视图）</h2>
    <hr />

    <div class="panel panel-default no-print">
        <div class="panel-heading">查询条件</div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group"><label>员工</label>
                        <asp:DropDownList runat="server" ID="ddlEmp" CssClass="form-control" />
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
                            <asp:ListItem Text="1月" Value="1" />
                            <asp:ListItem Text="2月" Value="2" />
                            <asp:ListItem Text="3月" Value="3" />
                            <asp:ListItem Text="4月" Value="4" />
                            <asp:ListItem Text="5月" Value="5" />
                            <asp:ListItem Text="6月" Value="6" />
                            <asp:ListItem Text="7月" Value="7" />
                            <asp:ListItem Text="8月" Value="8" />
                            <asp:ListItem Text="9月" Value="9" />
                            <asp:ListItem Text="10月" Value="10" />
                            <asp:ListItem Text="11月" Value="11" />
                            <asp:ListItem Text="12月" Value="12" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label>视图切换</label><br />
                        <label><asp:RadioButton runat="server" ID="rbList" GroupName="viewMode" Text="列表" Checked="true" AutoPostBack="true" OnCheckedChanged="rbViewMode_CheckedChanged" /></label>&nbsp;
                        <label><asp:RadioButton runat="server" ID="rbCalendar" GroupName="viewMode" Text="日历" AutoPostBack="true" OnCheckedChanged="rbViewMode_CheckedChanged" /></label>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnQuery" Text="查询" OnClick="btnQuery_Click" CssClass="btn btn-primary btn-block" />
                    </div>
                </div>
                <div class="col-md-1">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnPrint" Text="打印"
                            OnClientClick="window.print();return false;" CssClass="btn btn-info btn-block" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <%-- 列表视图 --%>
    <asp:Panel runat="server" ID="pnlList">
        <div class="table-responsive">
        <asp:GridView runat="server" ID="gvDetail" CssClass="table table-striped table-bordered table-condensed"
            AutoGenerateColumns="False" AllowPaging="True" PageSize="20" OnPageIndexChanging="gvDetail_PageIndexChanging">
            <Columns>
                <asp:BoundField DataField="AttDate" HeaderText="日期" DataFormatString="{0:yyyy-MM-dd ddd}" HtmlEncode="false" />
                <asp:BoundField DataField="CheckInTimeStr" HeaderText="签到时间" />
                <asp:BoundField DataField="CheckOutTimeStr" HeaderText="签退时间" />
                <asp:BoundField DataField="WorkHours" HeaderText="工时(h)" DataFormatString="{0:0.0}" />
                <asp:TemplateField HeaderText="状态">
                    <ItemTemplate>
                        <asp:Label runat="server" Text='<%# Eval("Status") %>'
                            CssClass='<%# GetStatusCss(Eval("Status").ToString()) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Remark" HeaderText="备注" />
            </Columns>
            <PagerStyle CssClass="pagination-ys no-print" HorizontalAlign="Center" />
        </asp:GridView>
        </div>
    </asp:Panel>

    <%-- 日历视图 --%>
    <asp:Panel runat="server" ID="pnlCalendar" Visible="false">
        <div class="panel panel-default" style="page-break-inside:avoid;">
            <div class="panel-heading">
                <asp:Literal runat="server" ID="litCalendarTitle" />
            </div>
            <div class="panel-body">
                <asp:Literal runat="server" ID="litCalendar" />
            </div>
            <div class="panel-footer">
                <span class="label label-success">正常</span>&nbsp;
                <span class="label label-warning">迟到</span>&nbsp;
                <span class="label label-warning">早退</span>&nbsp;
                <span class="label label-danger">缺勤</span>&nbsp;
                <span class="label label-info">请假</span>&nbsp;
                <span class="label label-default">未签到</span>&nbsp;
                <span class="label" style="background-color:#f5f5f5;color:#666;border:1px solid #ddd;">周末</span>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
