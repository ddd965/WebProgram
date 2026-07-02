<%@ Page Title="考勤参数设置" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="AttendanceParam.aspx.cs" Inherits="Attendance_AttendanceParam" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>考勤参数设置（全局单例）</h2>
    <hr />

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <div class="row">
        <div class="col-md-8">
            <div class="form-horizontal">
                <div class="form-group">
                    <label class="col-md-3 control-label">上班时间 *</label>
                    <div class="col-md-5">
                        <asp:TextBox runat="server" ID="txtWorkStart" CssClass="form-control" TextMode="Time" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtWorkStart"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">下班时间 *</label>
                    <div class="col-md-5">
                        <asp:TextBox runat="server" ID="txtWorkEnd" CssClass="form-control" TextMode="Time" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtWorkEnd"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">迟到宽限（分钟）*</label>
                    <div class="col-md-5">
                        <asp:TextBox runat="server" ID="txtLateMinutes" CssClass="form-control" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLateMinutes"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                        <asp:RangeValidator runat="server" ControlToValidate="txtLateMinutes" MinimumValue="0" MaximumValue="120"
                            Type="Integer" CssClass="text-danger" ErrorMessage="0~120分钟" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">早退宽限（分钟）*</label>
                    <div class="col-md-5">
                        <asp:TextBox runat="server" ID="txtEarlyMinutes" CssClass="form-control" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEarlyMinutes"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                        <asp:RangeValidator runat="server" ControlToValidate="txtEarlyMinutes" MinimumValue="0" MaximumValue="120"
                            Type="Integer" CssClass="text-danger" ErrorMessage="0~120分钟" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">午休开始 *</label>
                    <div class="col-md-5">
                        <asp:TextBox runat="server" ID="txtLunchStart" CssClass="form-control" TextMode="Time" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLunchStart"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-md-3 control-label">午休结束 *</label>
                    <div class="col-md-5">
                        <asp:TextBox runat="server" ID="txtLunchEnd" CssClass="form-control" TextMode="Time" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLunchEnd"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-md-offset-3 col-md-5">
                        <asp:Button runat="server" ID="btnSave" Text="保存参数" OnClick="btnSave_Click" CssClass="btn btn-primary" />
                        <asp:Literal runat="server" ID="litUpdateTime" />
                    </div>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="panel panel-info">
                <div class="panel-heading">说明</div>
                <div class="panel-body">
                    <ul>
                        <li>考勤参数为全局单例，对所有员工生效。</li>
                        <li>上班时间 + 迟到宽限 = 迟到判定时间。</li>
                        <li>下班时间 - 早退宽限 = 早退判定时间。</li>
                        <li>系统仅对状态为「正常」的签到签退记录判定迟到/早退。</li>
                        <li>如当日状态已判定为「请假」，则保持请假状态。</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
