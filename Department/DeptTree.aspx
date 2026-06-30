<%@ Page Title="部门管理" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="DeptTree.aspx.cs" Inherits="Department_DeptTree" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>部门管理</h2>
    <hr />
    <div class="row">
        <div class="col-md-5">
            <div class="panel panel-default">
                <div class="panel-heading">
                    部门树状结构
                    <a href="DeptEdit.aspx?mode=add" class="btn btn-xs btn-success pull-right">新增部门</a>
                </div>
                <div class="panel-body">
                    <asp:TreeView runat="server" ID="tvDept" ShowLines="True"
                        OnSelectedNodeChanged="tvDept_SelectedNodeChanged"
                        ExpandDepth="2" CssClass="treeview-hrms">
                    </asp:TreeView>
                </div>
            </div>
        </div>
        <div class="col-md-7">
            <div class="panel panel-default">
                <div class="panel-heading">
                    部门详情
                    <asp:Literal runat="server" ID="litDeptTitle" />
                </div>
                <div class="panel-body">
                    <asp:Label runat="server" ID="lblMsg" />
                    <asp:Panel runat="server" ID="pnlDetail" Visible="false">
                        <table class="table table-bordered">
                            <tr><th class="col-md-3">部门名称</th><td><asp:Literal runat="server" ID="litDeptName" /></td></tr>
                            <tr><th>上级部门</th><td><asp:Literal runat="server" ID="litParentName" /></td></tr>
                            <tr><th>部门经理</th><td><asp:Literal runat="server" ID="litManagerName" /></td></tr>
                            <tr><th>描述</th><td><asp:Literal runat="server" ID="litDescn" /></td></tr>
                            <tr><th>创建时间</th><td><asp:Literal runat="server" ID="litCreateTime" /></td></tr>
                        </table>
                        <asp:Button runat="server" ID="btnViewEmployees" Text="查看该部门员工" OnClick="btnViewEmployees_Click" CssClass="btn btn-info" />
                        <a id="linkEdit" runat="server" class="btn btn-primary">编辑</a>
                        <asp:Button runat="server" ID="btnDelete" Text="删除" OnClick="btnDelete_Click" CssClass="btn btn-danger"
                            OnClientClick="return confirm('删除该部门将把下属员工的所属部门置空，确定删除？')" />
                    </asp:Panel>
                </div>
            </div>
            <asp:Panel runat="server" ID="pnlEmployees" Visible="false">
                <h4>该部门员工列表</h4>
                <asp:GridView runat="server" ID="gvDeptEmployees" CssClass="table table-striped table-bordered table-condensed"
                    AutoGenerateColumns="False" AllowPaging="True" PageSize="5" OnPageIndexChanging="gvDeptEmployees_PageIndexChanging">
                    <Columns>
                        <asp:BoundField DataField="EmpId" HeaderText="ID" />
                        <asp:BoundField DataField="EmpNo" HeaderText="工号" />
                        <asp:BoundField DataField="EmpName" HeaderText="姓名" />
                        <asp:BoundField DataField="Gender" HeaderText="性别" />
                        <asp:BoundField DataField="Phone" HeaderText="电话" />
                        <asp:BoundField DataField="Status" HeaderText="状态" />
                    </Columns>
                    <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
                </asp:GridView>
            </asp:Panel>
        </div>
    </div>

    <asp:HiddenField runat="server" ID="hidDeptId" />
</asp:Content>
