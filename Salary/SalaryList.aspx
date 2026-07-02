<%@ Page Title="工资列表" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="SalaryList.aspx.cs" Inherits="Salary_SalaryList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>工资列表</h2>
    <hr />

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <div class="panel panel-default no-print">
        <div class="panel-heading">员工薪酬查询（按工号+月份范围）</div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group">
                        <label>员工</label>
                        <asp:DropDownList runat="server" ID="ddlQEmp" CssClass="form-control">
                            <asp:ListItem Value="">全部</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>月份起</label>
                        <asp:TextBox runat="server" ID="txtQFrom" CssClass="form-control" placeholder="YYYY-MM" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>月份止</label>
                        <asp:TextBox runat="server" ID="txtQTo" CssClass="form-control" placeholder="YYYY-MM" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnSearch" Text="查询" OnClick="btnSearch_Click" CssClass="btn btn-primary btn-block" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <a href="SalaryStat.aspx" class="btn btn-success">发薪历史汇总（合计+导出）</a>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="table-responsive">
    <asp:GridView runat="server" ID="gvSalary" CssClass="table table-striped table-bordered table-condensed"
        AutoGenerateColumns="False" DataKeyNames="SalaryId" OnRowCommand="gvSalary_RowCommand"
        AllowPaging="True" PageSize="15" OnPageIndexChanging="gvSalary_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="SalaryId" HeaderText="ID" ItemStyle-Width="50" />
            <asp:TemplateField HeaderText="员工">
                <ItemTemplate><%# GetEmpName(Eval("EmpId")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="SalaryMonth" HeaderText="月份" />
            <asp:BoundField DataField="BaseSalary" HeaderText="基本" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="Performance" HeaderText="绩效" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="Bonus" HeaderText="奖金" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="OvertimePay" HeaderText="加班费" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="Insurance" HeaderText="五险" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="Fund" HeaderText="公积金" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="Tax" HeaderText="个税" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="Deduction" HeaderText="扣款" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="NetSalary" HeaderText="实发工资" DataFormatString="{0:N2}"
                ItemStyle-ForeColor="#2b8cbe" ItemStyle-Font-Bold="true" />
            <asp:BoundField DataField="PayDate" HeaderText="发薪日" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:TemplateField HeaderText="操作" ItemStyle-Width="60">
                <ItemTemplate>
                    <asp:LinkButton runat="server" CommandName="DeleteRow" CommandArgument='<%# Eval("SalaryId") %>'
                        CausesValidation="false"
                        CssClass="btn btn-xs btn-danger" Text="删除"
                        OnClientClick="return confirm('确认删除该工资记录？')" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
    </asp:GridView>
    </div>
</asp:Content>
