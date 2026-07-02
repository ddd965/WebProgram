<%@ Page Title="休假类型设置" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="LeaveTypeMgr.aspx.cs" Inherits="Leave_LeaveTypeMgr" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>休假类型设置</h2>
    <hr />

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <div class="row">
        <div class="col-md-5">
            <div class="panel panel-default">
                <div class="panel-heading"><asp:Literal runat="server" ID="litFormTitle" Text="新增休假类型" /></div>
                <div class="panel-body">
                    <div class="form-horizontal">
                        <div class="form-group">
                            <label class="col-md-4 control-label">类型名称 *</label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="txtTypeName" CssClass="form-control" MaxLength="50" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtTypeName"
                                    CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="col-md-4 control-label">年度默认天数 *</label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="txtDefaultDays" CssClass="form-control" Text="0" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDefaultDays"
                                    CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                                <asp:RangeValidator runat="server" ControlToValidate="txtDefaultDays" MinimumValue="0" MaximumValue="365"
                                    Type="Double" CssClass="text-danger" ErrorMessage="天数应在 0-365 之间" Display="Dynamic" />
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="col-md-4 control-label">是否需要审批</label>
                            <div class="col-md-8">
                                <asp:CheckBox runat="server" ID="cbNeedApproval" Checked="true" CssClass="form-control-static" />
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="col-md-4 control-label">描述</label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="txtDescn" CssClass="form-control" MaxLength="255" TextMode="MultiLine" Rows="2" />
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="col-md-offset-4 col-md-8">
                                <asp:Button runat="server" ID="btnSave" Text="保存" OnClick="btnSave_Click" CssClass="btn btn-primary" />
                                <asp:Button runat="server" ID="btnCancel" Text="取消" OnClick="btnCancel_Click" CssClass="btn btn-default" Visible="false" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-md-7">
            <div class="table-responsive">
            <asp:GridView runat="server" ID="gvLeaveType" CssClass="table table-striped table-bordered table-condensed"
                AutoGenerateColumns="False" DataKeyNames="LeaveTypeId"
                OnRowCommand="gvLeaveType_RowCommand"
                AllowPaging="True" PageSize="10" OnPageIndexChanging="gvLeaveType_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="LeaveTypeId" HeaderText="ID" ItemStyle-Width="50" />
                    <asp:BoundField DataField="TypeName" HeaderText="类型名称" />
                    <asp:BoundField DataField="DefaultDays" HeaderText="默认天数" ItemStyle-Width="80" />
                    <asp:TemplateField HeaderText="需审批" ItemStyle-Width="60">
                        <ItemTemplate>
                            <asp:CheckBox runat="server" Checked='<%# Eval("NeedApproval") %>' Enabled="false" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Descn" HeaderText="描述" />
                    <asp:TemplateField HeaderText="操作" ItemStyle-Width="120">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CommandName="EditRow" CommandArgument='<%# Eval("LeaveTypeId") %>'
                                CssClass="btn btn-xs btn-info" Text="编辑" />
                            <asp:LinkButton runat="server" CommandName="DeleteRow" CommandArgument='<%# Eval("LeaveTypeId") %>'
                                CssClass="btn btn-xs btn-danger" Text="删除"
                                OnClientClick="return confirm('删除该休假类型将清空其下所有休假记录，确定删除？')" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
            </asp:GridView>
            </div>
        </div>
    </div>

    <asp:HiddenField runat="server" ID="hidEditId" />
</asp:Content>
