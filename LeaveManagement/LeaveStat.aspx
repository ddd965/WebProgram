<%@ Page Title="休假统计" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="LeaveStat.aspx.cs" Inherits="LeaveManagement_LeaveStat" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <div>
        <asp:Label ID="lblFilter" runat="server" Text="筛选条件:"></asp:Label>
        <asp:DropDownList ID="ddlDepartmentFilter" runat="server"></asp:DropDownList>
        <asp:DropDownList ID="ddlLeaveTypeFilter" runat="server"></asp:DropDownList>
        <asp:DropDownList ID="txtYearFilter" runat="server"></asp:DropDownList>
        <asp:Button ID="btnSearch" runat="server" Text="查询" OnClick="btnSearch_Click" />
        <br /><br />

        <asp:Chart ID="Chart1" runat="server" Width="800px" Height="400px">
            <Series>
                <asp:Series Name="Series1"></asp:Series>
            </Series>
            <ChartAreas>
                <asp:ChartArea Name="ChartArea1"></asp:ChartArea>
            </ChartAreas>
        </asp:Chart>

        <asp:GridView ID="gvLeaveStats" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped" GridLines="None">
            <Columns>
                <asp:BoundField DataField="LeaveId" HeaderText="休假ID" />
                <asp:BoundField DataField="EmpId" HeaderText="员工ID" />
                <asp:BoundField DataField="LeaveTypeId" HeaderText="休假类型ID" />
                <asp:BoundField DataField="StartDate" HeaderText="开始日期" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="EndDate" HeaderText="结束日期" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="LeaveDays" HeaderText="休假天数" />
                <asp:BoundField DataField="Reason" HeaderText="事由" />
                <asp:BoundField DataField="Status" HeaderText="状态" />
                <asp:BoundField DataField="ApplyTime" HeaderText="申请时间" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
