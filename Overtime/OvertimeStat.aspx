<%@ Page Title="加班统计" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="OvertimeStat.aspx.cs" Inherits="Overtime_OvertimeStat" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <div>
        <asp:Label runat="server" Text="部门:" />
        <asp:DropDownList ID="ddlDepartment" runat="server" Width="200px">
            <asp:ListItem Text="--全部部门--" Value="" />
        </asp:DropDownList>
        &nbsp;&nbsp;
        <asp:Label runat="server" Text="员工:" />
        <asp:DropDownList ID="ddlEmployee" runat="server" Width="200px">
            <asp:ListItem Text="--全部员工--" Value="" />
        </asp:DropDownList>
        &nbsp;&nbsp;
        <asp:Label runat="server" Text="年度:" />
        <asp:DropDownList ID="ddlYear" runat="server" Width="80px" />
        &nbsp;&nbsp;
        <asp:Label runat="server" Text="季度:" />
        <asp:DropDownList ID="ddlQuarter" runat="server" Width="80px">
            <asp:ListItem Text="一季度" Value="1" />
            <asp:ListItem Text="二季度" Value="2" />
            <asp:ListItem Text="三季度" Value="3" />
            <asp:ListItem Text="四季度" Value="4" />
        </asp:DropDownList>
        &nbsp;&nbsp;
        <asp:Button ID="btnSearch" runat="server" Text="查询" OnClick="btnSearch_Click" CssClass="btn btn-default" />
        <br /><br />

        <asp:GridView ID="gvOvertimeStat" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped" GridLines="None">
            <Columns>
                <asp:BoundField DataField="EmpId" HeaderText="员工ID" />
                <asp:BoundField DataField="EmpNo" HeaderText="工号" />
                <asp:BoundField DataField="EmpName" HeaderText="姓名" />
                <asp:BoundField DataField="DeptName" HeaderText="部门" />
                <asp:BoundField DataField="OtCount" HeaderText="加班次数" />
                <asp:BoundField DataField="TotalOtHours" HeaderText="加班总小时" />
                <asp:BoundField DataField="WorkdayHours" HeaderText="工作日加班" />
                <asp:BoundField DataField="WeekendHours" HeaderText="周末加班" />
                <asp:BoundField DataField="HolidayHours" HeaderText="节假日加班" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>