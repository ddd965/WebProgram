<%@ Page Title="首页" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron">
        <h1>人事管理系统</h1>
        <p class="lead">基于 ASP.NET 三层架构的企业人事管理系统，涵盖员工管理、部门管理、休假管理、考勤打卡、加班管理、工资管理和报表打印等核心业务。</p>
        <p>
            <a href="Account/Login.aspx" class="btn btn-primary btn-lg" id="btnLogin" runat="server">立即登录</a>
        </p>
    </div>
</asp:Content>
