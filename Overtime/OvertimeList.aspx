<%@ Page Title="加班列表/审批" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="OvertimeList.aspx.cs" Inherits="Overtime_OvertimeList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>加班列表 / 审批</h2>
    <hr />

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <div class="panel panel-default no-print">
        <div class="panel-heading">查询条件（特定员工加班查询）</div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group"><label>员工（姓名/工号）</label>
                        <asp:TextBox runat="server" ID="txtQEmp" CssClass="form-control" placeholder="模糊查询" />
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
                    <div class="form-group"><label>类型</label>
                        <asp:DropDownList runat="server" ID="ddlQType" CssClass="form-control">
                            <asp:ListItem Value="">全部</asp:ListItem>
                            <asp:ListItem Text="工作日" Value="工作日" />
                            <asp:ListItem Text="周末" Value="周末" />
                            <asp:ListItem Text="节假日" Value="节假日" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label>状态</label>
                        <asp:DropDownList runat="server" ID="ddlQStatus" CssClass="form-control">
                            <asp:ListItem Value="">全部</asp:ListItem>
                            <asp:ListItem Text="待审批" Value="待审批" />
                            <asp:ListItem Text="已批准" Value="已批准" />
                            <asp:ListItem Text="已拒绝" Value="已拒绝" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-1">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnSearch" Text="查询" OnClick="btnSearch_Click" CssClass="btn btn-primary btn-block" CausesValidation="false" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="table-responsive">
    <asp:GridView runat="server" ID="gvOt" CssClass="table table-striped table-bordered table-condensed"
        AutoGenerateColumns="False" DataKeyNames="OtId" OnRowCommand="gvOt_RowCommand"
        AllowPaging="True" PageSize="15" OnPageIndexChanging="gvOt_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="OtId" HeaderText="ID" ItemStyle-Width="50" />
            <asp:TemplateField HeaderText="员工">
                <ItemTemplate><%# GetEmpName(Eval("EmpId")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="OtDate" HeaderText="日期" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:TemplateField HeaderText="开始">
                <ItemTemplate><%# FormatTime(Eval("StartTime")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="结束">
                <ItemTemplate><%# FormatTime(Eval("EndTime")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="OtHours" HeaderText="时长(h)" DataFormatString="{0:0.0}" />
            <asp:BoundField DataField="OtType" HeaderText="类型" />
            <asp:BoundField DataField="Reason" HeaderText="事由" />
            <asp:TemplateField HeaderText="状态">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("Status") %>'
                        CssClass='<%# GetStatusCss(Eval("Status").ToString()) %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="操作" ItemStyle-Width="180">
                <ItemTemplate>
                    <asp:LinkButton runat="server" CommandName="Approve" CommandArgument='<%# Eval("OtId") %>' CausesValidation="false"
                        CssClass="btn btn-xs btn-success" Text="批准" Visible='<%# Eval("Status").ToString()=="待审批" && CanApprove() %>' />
                    <asp:LinkButton runat="server" CommandName="Reject" CommandArgument='<%# Eval("OtId") %>' CausesValidation="false"
                        CssClass="btn btn-xs btn-warning" Text="拒绝" Visible='<%# Eval("Status").ToString()=="待审批" && CanApprove() %>' />
                    <asp:LinkButton runat="server" CommandName="DeleteRow" CommandArgument='<%# Eval("OtId") %>' CausesValidation="false"
                        CssClass="btn btn-xs btn-danger" Text="删除"
                        OnClientClick="return confirm('确认删除该加班记录？')" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
    </asp:GridView>
    </div>
</asp:Content>
