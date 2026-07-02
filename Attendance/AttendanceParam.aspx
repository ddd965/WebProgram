<%@ Page Title="考勤参数设置" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="AttendanceParam.aspx.cs" Inherits="Attendance_AttendanceParam" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <div>
        <asp:DetailsView ID="dvSetting" runat="server" AutoGenerateRows="False"
            DefaultMode="Edit" OnItemUpdating="dvSetting_ItemUpdating">
            <Fields>
                <asp:BoundField DataField="SettingId" HeaderText="设置编号" ReadOnly="True" />
                <asp:TemplateField HeaderText="上班时间">
                    <EditItemTemplate>
                        <asp:TextBox ID="txtWorkStart" runat="server" Text='<%# Bind("WorkStartTime") %>' />
                        <asp:RequiredFieldValidator ID="rfvWorkStart" runat="server" ControlToValidate="txtWorkStart"
                            ErrorMessage="*" Display="Dynamic" ForeColor="Red" />
                        <asp:RegularExpressionValidator ID="revWorkStart" runat="server" ControlToValidate="txtWorkStart"
                            ErrorMessage="格式：HH:mm:ss" Display="Dynamic" ForeColor="Red"
                            ValidationExpression="^([01]\d|2[0-3]):[0-5]\d:[0-5]\d$" />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="下班时间">
                    <EditItemTemplate>
                        <asp:TextBox ID="txtWorkEnd" runat="server" Text='<%# Bind("WorkEndTime") %>' />
                        <asp:RequiredFieldValidator ID="rfvWorkEnd" runat="server" ControlToValidate="txtWorkEnd"
                            ErrorMessage="*" Display="Dynamic" ForeColor="Red" />
                        <asp:RegularExpressionValidator ID="revWorkEnd" runat="server" ControlToValidate="txtWorkEnd"
                            ErrorMessage="格式：HH:mm:ss" Display="Dynamic" ForeColor="Red"
                            ValidationExpression="^([01]\d|2[0-3]):[0-5]\d:[0-5]\d$" />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="LateMinutes" HeaderText="迟到宽限(分钟)" />
                <asp:BoundField DataField="EarlyMinutes" HeaderText="早退宽限(分钟)" />
                <asp:TemplateField HeaderText="午休开始">
                    <EditItemTemplate>
                        <asp:TextBox ID="txtLunchStart" runat="server" Text='<%# Bind("LunchStart") %>' />
                        <asp:RegularExpressionValidator ID="revLunchStart" runat="server" ControlToValidate="txtLunchStart"
                            ErrorMessage="格式：HH:mm:ss" Display="Dynamic" ForeColor="Red"
                            ValidationExpression="^([01]\d|2[0-3]):[0-5]\d:[0-5]\d$" />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="午休结束">
                    <EditItemTemplate>
                        <asp:TextBox ID="txtLunchEnd" runat="server" Text='<%# Bind("LunchEnd") %>' />
                        <asp:RegularExpressionValidator ID="revLunchEnd" runat="server" ControlToValidate="txtLunchEnd"
                            ErrorMessage="格式：HH:mm:ss" Display="Dynamic" ForeColor="Red"
                            ValidationExpression="^([01]\d|2[0-3]):[0-5]\d:[0-5]\d$" />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="UpdateTime" HeaderText="最后更新时间" ReadOnly="True" />
                <asp:CommandField ShowEditButton="True" />
            </Fields>
        </asp:DetailsView>
        <asp:Label ID="lblMsg" runat="server" ForeColor="Green" />
    </div>
</asp:Content>