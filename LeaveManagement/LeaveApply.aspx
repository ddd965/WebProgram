<%@ Page Title="休假申请" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="LeaveApply.aspx.cs" Inherits="LeaveManagement_LeaveApply" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <div>
        <asp:FormView ID="fvLeaveApply" runat="server" DefaultMode="Insert" OnItemInserting="fvLeaveApply_ItemInserting">
            <InsertItemTemplate>
                <table class="auto-style1">
                    <tr>
                        <td class="auto-style2">员工:</td>
                        <td>
                            <asp:DropDownList ID="ddlEmployee" runat="server"></asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td class="auto-style2">休假类型:</td>
                        <td>
                            <asp:DropDownList ID="ddlLeaveType" runat="server"></asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td class="auto-style2">开始日期:</td>
                        <td>
                            <asp:Calendar ID="calStartDate" runat="server"></asp:Calendar>
                        </td>
                    </tr>
                    <tr>
                        <td class="auto-style2">结束日期:</td>
                        <td>
                            <asp:Calendar ID="calEndDate" runat="server"></asp:Calendar>
                        </td>
                    </tr>
                    <tr>
                        <td class="auto-style2">请假原因:</td>
                        <td>
                            <asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" Rows="5"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="auto-style2"></td>
                        <td>
                            <asp:Button ID="btnSubmit" runat="server" Text="提交申请" CommandName="Insert" />
                        </td>
                    </tr>
                </table>
            </InsertItemTemplate>
        </asp:FormView>
        <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
    </div>
</asp:Content>
