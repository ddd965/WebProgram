<%@ Page Title="部门编辑" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="DeptEdit.aspx.cs" Inherits="Department_DeptEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><asp:Literal runat="server" ID="litTitle" /></h2>
    <hr />

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <div class="form-horizontal">
        <div class="form-group">
            <label class="col-md-2 control-label">部门名称 *</label>
            <div class="col-md-8">
                <asp:TextBox runat="server" ID="txtDeptName" CssClass="form-control" MaxLength="80" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDeptName"
                    CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
            </div>
        </div>
        <div class="form-group">
            <label class="col-md-2 control-label">上级部门</label>
            <div class="col-md-8">
                <asp:DropDownList runat="server" ID="ddlParent" CssClass="form-control">
                    <asp:ListItem Value="">（无 — 顶级部门）</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div class="form-group">
            <label class="col-md-2 control-label">部门经理</label>
            <div class="col-md-8">
                <asp:DropDownList runat="server" ID="ddlManager" CssClass="form-control">
                    <asp:ListItem Value="">（未指定）</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div class="form-group">
            <label class="col-md-2 control-label">部门描述</label>
            <div class="col-md-8">
                <asp:TextBox runat="server" ID="txtDescn" CssClass="form-control" MaxLength="255"
                    TextMode="MultiLine" Rows="3" />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-8">
                <asp:Button runat="server" ID="btnSave" Text="保存" OnClick="btnSave_Click" CssClass="btn btn-primary" />
                <a href="DeptTree.aspx" class="btn btn-default">返回</a>
            </div>
        </div>
    </div>

    <asp:HiddenField runat="server" ID="hidDeptId" />
</asp:Content>
