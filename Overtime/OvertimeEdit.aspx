<%@ Page Title="加班录入" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="OvertimeEdit.aspx.cs" Inherits="Overtime_OvertimeEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>加班登记（自动计算 OtHours）</h2>
    <hr />

    <div class="panel panel-default no-print">
        <div class="panel-heading">加班录入</div>
        <div class="panel-body">
            <asp:ValidationSummary runat="server" ID="vsOt" ValidationGroup="OtGrp"
                CssClass="alert alert-danger" HeaderText="⚠ 提交前请完善：" ShowSummary="true" />
            <div class="row">
                <div class="col-md-2">
                    <div class="form-group">
                        <label>员工 *</label>
                        <asp:DropDownList runat="server" ID="ddlEmployee" CssClass="form-control" ValidationGroup="OtGrp" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlEmployee" ValidationGroup="OtGrp"
                            CssClass="text-danger" ErrorMessage="请选择员工" Display="Dynamic" InitialValue="" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>加班日期 *</label>
                        <asp:TextBox runat="server" ID="txtOtDate" CssClass="form-control" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtOtDate" ValidationGroup="OtGrp"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>开始时间 *</label>
                        <asp:TextBox runat="server" ID="txtStartTime" CssClass="form-control" TextMode="Time" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtStartTime" ValidationGroup="OtGrp"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>结束时间 *</label>
                        <asp:TextBox runat="server" ID="txtEndTime" CssClass="form-control" TextMode="Time" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEndTime" ValidationGroup="OtGrp"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>加班类型 *</label>
                        <asp:DropDownList runat="server" ID="ddlOtType" CssClass="form-control">
                            <asp:ListItem Text="工作日" Value="工作日" Selected="True" />
                            <asp:ListItem Text="周末" Value="周末" />
                            <asp:ListItem Text="节假日" Value="节假日" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>加班小时（自动计算）</label>
                        <asp:TextBox runat="server" ID="txtOtHours" CssClass="form-control" ReadOnly="true" />
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-9">
                    <div class="form-group">
                        <label>加班事由 *</label>
                        <asp:TextBox runat="server" ID="txtReason" CssClass="form-control" MaxLength="500" TextMode="MultiLine" Rows="2" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtReason" ValidationGroup="OtGrp"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnCalc" Text="计算时长" OnClick="btnCalc_Click"
                            CssClass="btn btn-default" CausesValidation="false" />
                        <asp:Button runat="server" ID="btnSave" Text="提交加班" OnClick="btnSave_Click"
                            CssClass="btn btn-primary" ValidationGroup="OtGrp" CausesValidation="true" />
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <p class="text-muted">
                        提示：加班提交后进入「待审批」状态，管理员/部门经理可到
                        <a href="OvertimeList.aspx">加班列表/审批</a> 中审核，
                        汇总/回写加班费到工资请到 <a href="OvertimeStat.aspx">月度/季度加班汇总</a>。
                    </p>
                </div>
            </div>
        </div>
    </div>

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />
</asp:Content>
