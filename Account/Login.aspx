<%@ Page Title="登录 - 人事管理系统" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Account_Login" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2>人事管理系统登录</h2>
    <hr />
    <div class="row">
        <div class="col-md-6">
            <asp:PlaceHolder runat="server" ID="ErrorMessage" Visible="false">
                <div class="alert alert-danger">
                    <asp:Literal runat="server" ID="FailureText" />
                </div>
            </asp:PlaceHolder>
            <div class="form-horizontal">
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="UserName" CssClass="col-md-3 control-label">用户名</asp:Label>
                    <div class="col-md-9">
                        <asp:TextBox runat="server" ID="UserName" CssClass="form-control" placeholder="请输入用户名" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="UserName"
                            CssClass="text-danger" ErrorMessage="请输入用户名" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="Password" CssClass="col-md-3 control-label">密码</asp:Label>
                    <div class="col-md-9">
                        <asp:TextBox runat="server" ID="Password" TextMode="Password" CssClass="form-control" placeholder="请输入密码" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="Password"
                            CssClass="text-danger" ErrorMessage="请输入密码" Display="Dynamic" />
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-md-offset-3 col-md-9">
                        <asp:Button runat="server" OnClick="LogIn" Text="登 录" CssClass="btn btn-primary btn-block" />
                    </div>
                </div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="well">
                <h4>测试账号</h4>
                <table class="table table-bordered table-condensed">
                    <tr><th>角色</th><th>用户名</th><th>密码</th></tr>
                    <tr><td>管理员</td><td>admin</td><td>123456</td></tr>
                    <tr><td>部门经理</td><td>lina</td><td>123456</td></tr>
                    <tr><td>普通用户</td><td>zhaom</td><td>123456</td></tr>
                </table>
            </div>
        </div>
    </div>
    <asp:HiddenField runat="server" ID="RegisterHyperLink" />
</asp:Content>
