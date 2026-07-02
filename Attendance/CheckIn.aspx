<%@ Page Title="考勤打卡" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="CheckIn.aspx.cs" Inherits="Attendance_CheckIn" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>当日考勤打卡</h2>
    <hr />

    <div class="row">
        <div class="col-md-8">
            <div class="jumbotron" style="text-align:center;padding:30px;">
                <h3><asp:Literal runat="server" ID="litEmployeeName" /></h3>
                <h1 style="font-size:56px;margin:20px 0;"><asp:Literal runat="server" ID="litNow" /></h1>
                <h4 style="color:#999;margin-bottom:25px;"><asp:Literal runat="server" ID="litDate" /></h4>

                <div class="row">
                    <div class="col-md-6">
                        <asp:Button runat="server" ID="btnCheckIn" Text="签到" OnClick="btnCheckIn_Click"
                            CssClass="btn btn-success btn-lg btn-block" style="font-size:24px;padding:20px;" />
                        <p style="margin-top:10px;">
                            签到时间：<strong><asp:Literal runat="server" ID="litCheckInTime" /></strong>
                            <asp:Label runat="server" ID="lblCheckInStatus" CssClass="label" style="margin-left:8px;" />
                        </p>
                    </div>
                    <div class="col-md-6">
                        <asp:Button runat="server" ID="btnCheckOut" Text="签退" OnClick="btnCheckOut_Click"
                            CssClass="btn btn-primary btn-lg btn-block" style="font-size:24px;padding:20px;" />
                        <p style="margin-top:10px;">
                            签退时间：<strong><asp:Literal runat="server" ID="litCheckOutTime" /></strong>
                            <asp:Label runat="server" ID="lblCheckOutStatus" CssClass="label" style="margin-left:8px;" />
                        </p>
                    </div>
                </div>
            </div>

            <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />
        </div>
        <div class="col-md-4">
            <div class="panel panel-default no-print">
                <div class="panel-heading">员工切换（管理员/经理）</div>
                <div class="panel-body">
                    <div class="form-group">
                        <asp:DropDownList runat="server" ID="ddlEmployee" CssClass="form-control" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlEmployee_SelectedIndexChanged" />
                    </div>
                    <small class="text-muted">普通用户只能查看并打卡自己的考勤。</small>
                </div>
            </div>
            <div class="panel panel-default">
                <div class="panel-heading">本月考勤统计（<asp:Literal runat="server" ID="litMonth" />）</div>
                <div class="panel-body">
                    <table class="table table-condensed">
                        <tr><td>正常</td><td><strong class="text-success"><asp:Literal runat="server" ID="ltNormal" /></strong></td></tr>
                        <tr><td>迟到</td><td><strong class="text-warning"><asp:Literal runat="server" ID="ltLate" /></strong></td></tr>
                        <tr><td>早退</td><td><strong class="text-warning"><asp:Literal runat="server" ID="ltEarly" /></strong></td></tr>
                        <tr><td>缺勤</td><td><strong class="text-danger"><asp:Literal runat="server" ID="ltAbsent" /></strong></td></tr>
                        <tr><td>请假</td><td><strong class="text-info"><asp:Literal runat="server" ID="ltLeave" /></strong></td></tr>
                    </table>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
