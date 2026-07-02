<%@ Page Title="休假类型管理" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="LeaveTypeMgr.aspx.cs" Inherits="LeaveManagement_LeaveTypeMgr" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <div>
        <asp:GridView ID="gvLeaveTypes" runat="server" AutoGenerateColumns="False" DataKeyNames="LeaveTypeId"
            OnRowEditing="gvLeaveTypes_RowEditing" OnRowUpdating="gvLeaveTypes_RowUpdating"
            OnRowCancelingEdit="gvLeaveTypes_RowCancelingEdit" OnRowDeleting="gvLeaveTypes_RowDeleting">
            <Columns>
                <asp:BoundField DataField="LeaveTypeId" HeaderText="ID" ReadOnly="True" SortExpression="LeaveTypeId" />
                <asp:BoundField DataField="TypeName" HeaderText="休假类型名称" SortExpression="TypeName" />
                <asp:BoundField DataField="DefaultDays" HeaderText="默认天数" SortExpression="DefaultDays" />
                <asp:CheckBoxField DataField="NeedApproval" HeaderText="需要审批" SortExpression="NeedApproval" />
                <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
            </Columns>
        </asp:GridView>
        <asp:DetailsView ID="dvLeaveType" runat="server" AutoGenerateRows="False" DataKeyNames="LeaveTypeId" DefaultMode="Insert" OnItemInserting="dvLeaveType_ItemInserting" OnItemUpdating="dvLeaveType_ItemUpdating" OnItemDeleting="dvLeaveType_ItemDeleting">
            <Fields>
                <asp:BoundField DataField="TypeName" HeaderText="休假类型名称" SortExpression="TypeName" />
                <asp:BoundField DataField="DefaultDays" HeaderText="默认天数" SortExpression="DefaultDays" />
                <asp:CheckBoxField DataField="NeedApproval" HeaderText="需要审批" SortExpression="NeedApproval" />
                <asp:CommandField ShowInsertButton="True" ShowEditButton="True" />
            </Fields>
        </asp:DetailsView>
    </div>
</asp:Content>
