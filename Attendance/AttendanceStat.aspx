<%@ Page Title="考勤统计" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="AttendanceStat.aspx.cs" Inherits="Attendance_AttendanceStat" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <div>
        <asp:Label runat="server" Text="部门:" />
        <asp:DropDownList ID="ddlDepartment" runat="server" Width="200px">
            <asp:ListItem Text="--全部部门--" Value="" />
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

        <asp:GridView ID="gvAttendanceStat" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped" GridLines="None">
            <Columns>
                <asp:BoundField DataField="EmpId" HeaderText="员工ID" />
                <asp:BoundField DataField="EmpNo" HeaderText="工号" />
                <asp:BoundField DataField="EmpName" HeaderText="姓名" />
                <asp:BoundField DataField="DeptName" HeaderText="部门" />
                <asp:BoundField DataField="NormalDays" HeaderText="正常天数" />
                <asp:BoundField DataField="LateCount" HeaderText="迟到次数" />
                <asp:BoundField DataField="EarlyCount" HeaderText="早退次数" />
                <asp:BoundField DataField="AbsentCount" HeaderText="缺勤次数" />
                <asp:BoundField DataField="LeaveCount" HeaderText="请假次数" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>