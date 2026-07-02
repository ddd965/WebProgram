<%@ Page Title="事件日志管理" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="EventLogList.aspx.cs" Inherits="EventLog_EventLogList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>事件日志管理</h2>
    <hr />

    <div class="panel panel-default">
        <div class="panel-heading">查询条件</div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group">
                        <label>用户名（模糊）</label>
                        <asp:TextBox runat="server" ID="txtUserName" CssClass="form-control" placeholder="输入用户名" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>事件名</label>
                        <asp:DropDownList runat="server" ID="ddlEventName" CssClass="form-control">
                            <asp:ListItem Value="">全部</asp:ListItem>
                            <asp:ListItem Text="登录" Value="登录" />
                            <asp:ListItem Text="登出" Value="登出" />
                            <asp:ListItem Text="新增" Value="新增" />
                            <asp:ListItem Text="修改" Value="修改" />
                            <asp:ListItem Text="删除" Value="删除" />
                            <asp:ListItem Text="审批" Value="审批" />
                            <asp:ListItem Text="导出" Value="导出" />
                            <asp:ListItem Text="签到" Value="签到" />
                            <asp:ListItem Text="签退" Value="签退" />
                            <asp:ListItem Text="登录失败" Value="登录失败" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>开始日期</label>
                        <asp:TextBox runat="server" ID="txtStartDate" CssClass="form-control" TextMode="Date" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>结束日期</label>
                        <asp:TextBox runat="server" ID="txtEndDate" CssClass="form-control" TextMode="Date" />
                    </div>
                </div>
            </div>
            <asp:Button runat="server" ID="btnSearch" Text="查询" OnClick="btnSearch_Click" CssClass="btn btn-primary" />
            <asp:Button runat="server" ID="btnReset" Text="重置" OnClick="btnReset_Click" CssClass="btn btn-default" />
            <div class="btn-group pull-right">
                <asp:Button runat="server" ID="btnDeleteBefore6Month" Text="清理6个月前日志"
                    OnClick="btnDeleteBefore6Month_Click" CssClass="btn btn-warning"
                    OnClientClick="return confirm('确认删除6个月前的所有日志？此操作不可恢复。')" />
                <asp:Button runat="server" ID="btnDeleteSelected" Text="批量删除选中" OnClick="btnDeleteSelected_Click"
                    CssClass="btn btn-danger" OnClientClick="return confirm('确认删除选中的日志？')" />
            </div>
        </div>
    </div>

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <div class="table-responsive">
    <asp:GridView runat="server" ID="gvEventLog" CssClass="table table-striped table-bordered table-condensed"
        AutoGenerateColumns="False" DataKeyNames="LogId"
        AllowPaging="True" PageSize="15" OnPageIndexChanging="gvEventLog_PageIndexChanging"
        OnRowCommand="gvEventLog_RowCommand">
        <Columns>
            <asp:TemplateField HeaderStyle-Width="30">
                <HeaderTemplate>
                    <asp:CheckBox runat="server" ID="cbAll" AutoPostBack="true" OnCheckedChanged="cbAll_CheckedChanged" />
                </HeaderTemplate>
                <ItemTemplate>
                    <asp:CheckBox runat="server" ID="cbSelect" />
                    <asp:HiddenField runat="server" ID="hidLogId" Value='<%# Eval("LogId") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="LogId" HeaderText="ID" ItemStyle-Width="60" />
            <asp:BoundField DataField="UserName" HeaderText="用户" />
            <asp:BoundField DataField="EventName" HeaderText="事件名" />
            <asp:BoundField DataField="Description" HeaderText="描述" />
            <asp:BoundField DataField="IPAddress" HeaderText="IP地址" />
            <asp:BoundField DataField="EventTime" HeaderText="时间" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
            <asp:TemplateField HeaderText="操作" ItemStyle-Width="60">
                <ItemTemplate>
                    <asp:LinkButton runat="server" CommandName="DeleteLog" CommandArgument='<%# Eval("LogId") %>'
                        CssClass="btn btn-xs btn-danger" Text="删除"
                        OnClientClick="return confirm('确认删除该日志？')" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
    </asp:GridView>
    </div>
</asp:Content>
