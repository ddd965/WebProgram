<%@ Page Title="休假审批/查询" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="LeaveList.aspx.cs" Inherits="Leave_LeaveList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>休假审批 / 查询</h2>
    <hr />

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <div class="panel panel-default no-print">
        <div class="panel-heading">查询条件</div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group"><label>审批状态</label>
                        <asp:DropDownList runat="server" ID="ddlQStatus" CssClass="form-control">
                            <asp:ListItem Value="">全部</asp:ListItem>
                            <asp:ListItem Text="待审批" Value="待审批" />
                            <asp:ListItem Text="已批准" Value="已批准" />
                            <asp:ListItem Text="已拒绝" Value="已拒绝" />
                            <asp:ListItem Text="已取消" Value="已取消" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group"><label>休假类型</label>
                        <asp:DropDownList runat="server" ID="ddlQType" CssClass="form-control">
                            <asp:ListItem Value="">全部</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label>起始日期</label>
                        <asp:TextBox runat="server" ID="txtQStart" CssClass="form-control" TextMode="Date" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label>结束日期</label>
                        <asp:TextBox runat="server" ID="txtQEnd" CssClass="form-control" TextMode="Date" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnSearch" Text="查询" OnClick="btnSearch_Click" CssClass="btn btn-primary btn-block" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="table-responsive">
    <asp:GridView runat="server" ID="gvLeave" CssClass="table table-striped table-bordered table-condensed"
        AutoGenerateColumns="False" DataKeyNames="LeaveId"
        OnRowCommand="gvLeave_RowCommand"
        AllowPaging="True" PageSize="15" OnPageIndexChanging="gvLeave_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="LeaveId" HeaderText="ID" ItemStyle-Width="50" />
            <asp:TemplateField HeaderText="员工">
                <ItemTemplate><%# GetEmpName(Eval("EmpId")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="休假类型">
                <ItemTemplate><%# GetTypeName(Eval("LeaveTypeId")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="StartDate" HeaderText="开始" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="EndDate" HeaderText="结束" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="LeaveDays" HeaderText="天数" ItemStyle-Width="60" />
            <asp:BoundField DataField="Reason" HeaderText="原因" />
            <asp:TemplateField HeaderText="状态" ItemStyle-Width="70">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("Status") %>'
                        CssClass='<%# GetStatusCss(Eval("Status").ToString()) %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="操作" ItemStyle-Width="170">
                <ItemTemplate>
                    <asp:LinkButton runat="server" CommandName="Approve" CommandArgument='<%# Eval("LeaveId") %>' CausesValidation="false"
                        CssClass="btn btn-xs btn-success" Text="批准" Visible='<%# Eval("Status").ToString()=="待审批" && CanApprove() %>' />
                    <asp:LinkButton runat="server" CommandName="Reject" CommandArgument='<%# Eval("LeaveId") %>' CausesValidation="false"
                        CssClass="btn btn-xs btn-warning" Text="拒绝" Visible='<%# Eval("Status").ToString()=="待审批" && CanApprove() %>' />
                    <asp:LinkButton runat="server" CommandName="CancelRow" CommandArgument='<%# Eval("LeaveId") %>' CausesValidation="false"
                        CssClass="btn btn-xs btn-default" Text="取消" Visible='<%# Eval("Status").ToString()=="待审批" %>' />
                    <asp:LinkButton runat="server" CommandName="DeleteRow" CommandArgument='<%# Eval("LeaveId") %>' CausesValidation="false"
                        CssClass="btn btn-xs btn-danger" Text="删除"
                        OnClientClick="return confirm('确认删除？')" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
    </asp:GridView>
    </div>
</asp:Content>
