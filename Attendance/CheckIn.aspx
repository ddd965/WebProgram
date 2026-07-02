<%@ Page Title="考勤打卡" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="CheckIn.aspx.cs" Inherits="Attendance_CheckIn" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <div style="margin-top: 20px;">
        <asp:Label ID="lblCurrentTime" runat="server" Font-Size="Large" /><br /><br />
        <asp:Label ID="lblEmpInfo" runat="server" Font-Size="Medium" /><br /><br />

        <asp:Button ID="btnCheckIn" runat="server" Text="签 到" CssClass="btn btn-primary btn-lg"
            OnClick="btnCheckIn_Click" Width="200px" />
        &nbsp;&nbsp;
        <asp:Button ID="btnCheckOut" runat="server" Text="签 退" CssClass="btn btn-warning btn-lg"
            OnClick="btnCheckOut_Click" Width="200px" />
        <br /><br />

        <asp:Label ID="lblStatus" runat="server" Font-Size="Large" ForeColor="Green" />
        <asp:Label ID="lblError" runat="server" ForeColor="Red" />

        <hr />
        <h4>今日打卡记录</h4>
        <asp:GridView ID="gvTodayRecord" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered" GridLines="None">
            <Columns>
                <asp:BoundField DataField="EmpName" HeaderText="员工姓名" />
                <asp:BoundField DataField="AttDate" HeaderText="日期" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="CheckInTime" HeaderText="签到时间" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                <asp:BoundField DataField="CheckOutTime" HeaderText="签退时间" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                <asp:BoundField DataField="Status" HeaderText="考勤状态" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>