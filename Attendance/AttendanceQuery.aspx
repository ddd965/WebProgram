<%@ Page Title="考勤记录查询" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="AttendanceQuery.aspx.cs" Inherits="Attendance_AttendanceQuery" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <div>
        <asp:Label runat="server" Text="员工:" />
        <asp:DropDownList ID="ddlEmployee" runat="server" AutoPostBack="False" Width="200px">
            <asp:ListItem Text="--请选择员工--" Value="" />
        </asp:DropDownList>
        &nbsp;&nbsp;
        <asp:Label runat="server" Text="年度:" />
        <asp:DropDownList ID="ddlYear" runat="server" Width="80px" />
        &nbsp;&nbsp;
        <asp:Label runat="server" Text="月份:" />
        <asp:DropDownList ID="ddlMonth" runat="server" Width="60px" />
        &nbsp;&nbsp;
        <asp:Button ID="btnSearch" runat="server" Text="查询" OnClick="btnSearch_Click" CssClass="btn btn-default" />
        <br /><br />

        <asp:GridView ID="gvAttendance" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered" GridLines="None">
            <Columns>
                <asp:BoundField DataField="AttDate" HeaderText="日期" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="CheckInTime" HeaderText="签到时间" DataFormatString="{0:HH:mm:ss}" />
                <asp:BoundField DataField="CheckOutTime" HeaderText="签退时间" DataFormatString="{0:HH:mm:ss}" />
                <asp:BoundField DataField="Status" HeaderText="考勤状态" />
                <asp:BoundField DataField="Remark" HeaderText="备注" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>