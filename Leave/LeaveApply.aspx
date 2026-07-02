<%@ Page Title="休假申请" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="LeaveApply.aspx.cs" Inherits="Leave_LeaveApply" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>休假申请</h2>
    <hr />

    <div class="panel panel-default no-print">
        <div class="panel-heading">休假申请</div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group">
                        <label>员工</label>
                        <asp:DropDownList runat="server" ID="ddlEmployee" CssClass="form-control" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>休假类型</label>
                        <asp:DropDownList runat="server" ID="ddlLeaveType" CssClass="form-control" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>开始日期 *</label>
                        <asp:TextBox runat="server" ID="txtStartDate" CssClass="form-control" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtStartDate" ValidationGroup="ApplyGrp"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>结束日期 *</label>
                        <asp:TextBox runat="server" ID="txtEndDate" CssClass="form-control" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEndDate" ValidationGroup="ApplyGrp"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>工作日天数</label>
                        <div class="input-group">
                            <asp:TextBox runat="server" ID="txtLeaveDays" CssClass="form-control" ReadOnly="true" />
                            <span class="input-group-btn">
                                <asp:Button runat="server" ID="btnCalc" Text="计算" OnClick="btnCalc_Click" CssClass="btn btn-default" CausesValidation="false" />
                            </span>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-10">
                    <div class="form-group">
                        <label>请假原因 *</label>
                        <asp:TextBox runat="server" ID="txtReason" CssClass="form-control" MaxLength="500" TextMode="MultiLine" Rows="2" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtReason" ValidationGroup="ApplyGrp"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-2">
                    <asp:Button runat="server" ID="btnSubmit" Text="提交申请" OnClick="btnSubmit_Click" ValidationGroup="ApplyGrp"
                        CausesValidation="true" CssClass="btn btn-primary btn-block" style="margin-top:25px;" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <p class="text-muted">
                        提示：提交休假申请后，可到 <a href="LeaveList.aspx">休假审批/查询</a> 查看状态，管理员或部门经理可审批。
                    </p>
                </div>
            </div>
        </div>
    </div>

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />
    <asp:ValidationSummary runat="server" ID="vsApply" CssClass="alert alert-danger" ValidationGroup="ApplyGrp"
        ShowSummary="true" HeaderText="⚠ 提交休假申请前请完善：" />
</asp:Content>
