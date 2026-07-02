<%@ Page Title="加班信息录入" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="OvertimeEdit.aspx.cs" Inherits="Overtime_OvertimeEdit" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <div>
        <asp:GridView ID="gvOvertime" runat="server" AutoGenerateColumns="False"
            DataKeyNames="OtId,EmpId" CssClass="table table-bordered"
            OnRowEditing="gvOvertime_RowEditing" OnRowUpdating="gvOvertime_RowUpdating"
            OnRowCancelingEdit="gvOvertime_RowCancelingEdit" OnRowDeleting="gvOvertime_RowDeleting">
            <Columns>
                <asp:BoundField DataField="OtId" HeaderText="ID" ReadOnly="True" />
                <asp:BoundField DataField="EmpName" HeaderText="员工" ReadOnly="True" />
                <asp:BoundField DataField="OtDate" HeaderText="日期" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:TemplateField HeaderText="开始时间">
                    <EditItemTemplate>
                        <asp:TextBox ID="txtStartTime" runat="server" Text='<%# Bind("StartTime") %>' />
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:Label runat="server" Text='<%# Eval("StartTime", "{0:hh\\:mm}") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="结束时间">
                    <EditItemTemplate>
                        <asp:TextBox ID="txtEndTime" runat="server" Text='<%# Bind("EndTime") %>' />
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:Label runat="server" Text='<%# Eval("EndTime", "{0:hh\\:mm}") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="OtHours" HeaderText="小时数" ReadOnly="True" />
                <asp:TemplateField HeaderText="加班类型">
                    <EditItemTemplate>
                        <asp:DropDownList ID="ddlOtType" runat="server" SelectedValue='<%# Bind("OtType") %>'>
                            <asp:ListItem Text="工作日" Value="工作日" />
                            <asp:ListItem Text="周末" Value="周末" />
                            <asp:ListItem Text="节假日" Value="节假日" />
                        </asp:DropDownList>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:Label runat="server" Text='<%# Eval("OtType") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="审批状态">
                    <EditItemTemplate>
                        <asp:DropDownList ID="ddlStatus" runat="server" SelectedValue='<%# Bind("Status") %>'>
                            <asp:ListItem Text="待审批" Value="待审批" />
                            <asp:ListItem Text="已批准" Value="已批准" />
                            <asp:ListItem Text="已拒绝" Value="已拒绝" />
                        </asp:DropDownList>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:Label runat="server" Text='<%# Eval("Status") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Reason" HeaderText="事由" />
                <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
            </Columns>
        </asp:GridView>

        <hr />
        <h4>新增加班记录</h4>
        <div>
            <table>
                <tr>
                    <td>员工：</td>
                    <td><asp:DropDownList ID="ddlEmployee" runat="server" /></td>
                </tr>
                <tr>
                    <td>加班日期：</td>
                    <td>
                        <asp:TextBox ID="txtOtDate" runat="server" Text='<%# DateTime.Now.ToString("yyyy-MM-dd") %>' />
                        <asp:CompareValidator ID="cvDate" runat="server" ControlToValidate="txtOtDate"
                            ErrorMessage="格式：yyyy-MM-dd" Display="Dynamic" ForeColor="Red"
                            Operator="DataTypeCheck" Type="Date" />
                    </td>
                </tr>
                <tr>
                    <td>开始时间：</td>
                    <td>
                        <asp:TextBox ID="txtStartTime" runat="server" Text="18:00" />
                        <asp:RegularExpressionValidator ID="revStart" runat="server" ControlToValidate="txtStartTime"
                            ErrorMessage="格式：HH:mm" Display="Dynamic" ForeColor="Red"
                            ValidationExpression="^([01]\d|2[0-3]):[0-5]\d$" />
                    </td>
                </tr>
                <tr>
                    <td>结束时间：</td>
                    <td>
                        <asp:TextBox ID="txtEndTime" runat="server" Text="22:00" />
                        <asp:RegularExpressionValidator ID="revEnd" runat="server" ControlToValidate="txtEndTime"
                            ErrorMessage="格式：HH:mm" Display="Dynamic" ForeColor="Red"
                            ValidationExpression="^([01]\d|2[0-3]):[0-5]\d$" />
                    </td>
                </tr>
                <tr>
                    <td>加班类型：</td>
                    <td>
                        <asp:DropDownList ID="ddlOtTypeNew" runat="server">
                            <asp:ListItem Text="工作日" Value="工作日" />
                            <asp:ListItem Text="周末" Value="周末" />
                            <asp:ListItem Text="节假日" Value="节假日" />
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>审批状态：</td>
                    <td>
                        <asp:DropDownList ID="ddlStatusNew" runat="server">
                            <asp:ListItem Text="待审批" Value="待审批" />
                            <asp:ListItem Text="已批准" Value="已批准" />
                            <asp:ListItem Text="已拒绝" Value="已拒绝" />
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>加班事由：</td>
                    <td>
                        <asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" Rows="3" Columns="50" />
                        <asp:RequiredFieldValidator ID="rfvReason" runat="server" ControlToValidate="txtReason"
                            ErrorMessage="必填" Display="Dynamic" ForeColor="Red" />
                    </td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <asp:Button ID="btnInsert" runat="server" Text="新增" OnClick="btnInsert_Click" CssClass="btn btn-primary" />
                    </td>
                </tr>
            </table>
            <asp:Label ID="lblMsg" runat="server" ForeColor="Green" />
        </div>
    </div>
</asp:Content>