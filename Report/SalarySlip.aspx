<%@ Page Title="员工工资条报表" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="SalarySlip.aspx.cs" Inherits="Report_SalarySlip" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>员工工资条报表</h2>
    <hr />

    <style>
        .slip-box {
            border: 1px solid #ddd;
            margin-bottom: 30px;
            padding: 20px;
            page-break-inside: avoid;
            page-break-after: always;
        }
        .slip-title { text-align: center; font-weight: bold; font-size: 18px; margin-bottom: 15px; }
        .slip-info { margin-bottom: 10px; }
        .slip-info span { margin-right: 25px; }
        .slip-table { width: 100%; border-collapse: collapse; }
        .slip-table th, .slip-table td { border:1px solid #999; padding:6px 10px; text-align:center; font-size:13px; }
        .slip-table th { background:#f0f0f0; }
        .slip-table td.num { text-align: right; font-family: Consolas, monospace; }
        .net-cell { font-weight: bold; color:#2b8cbe; background:#f6fbff; }
    </style>

    <div class="panel panel-default no-print">
        <div class="panel-heading">打印条件</div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group"><label>部门</label>
                        <asp:DropDownList runat="server" ID="ddlDept" CssClass="form-control">
                            <asp:ListItem Value="">全部部门</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label>员工</label>
                        <asp:DropDownList runat="server" ID="ddlEmp" CssClass="form-control">
                            <asp:ListItem Value="">全部员工</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label>薪资月份 *</label>
                        <asp:TextBox runat="server" ID="txtMonth" CssClass="form-control" placeholder="YYYY-MM" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnGen" Text="生成工资条" OnClick="btnGen_Click" CssClass="btn btn-primary btn-block" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnPrint" Text="批量打印"
                            OnClientClick="window.print();return false;" CssClass="btn btn-info btn-block" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <asp:Repeater runat="server" ID="rptSlips" OnItemDataBound="rptSlips_ItemDataBound">
        <ItemTemplate>
            <div class="slip-box">
                <div class="slip-title">薪 资 条</div>
                <div class="slip-info">
                    <span>薪资月份：<strong><%# Eval("SalaryMonth") %></strong></span>
                    <span>工号：<strong><%# Eval("EmpNo") %></strong></span>
                    <span>姓名：<strong><%# Eval("EmpName") %></strong></span>
                    <span>部门：<strong><%# Eval("DeptName") %></strong></span>
                    <span>打印日期：<%# DateTime.Now.ToString("yyyy-MM-dd") %></span>
                </div>
                <table class="slip-table">
                    <tr>
                        <th>项目</th><th>金额 (¥)</th>
                        <th>项目</th><th>金额 (¥)</th>
                        <th>项目</th><th>金额 (¥)</th>
                    </tr>
                    <tr>
                        <td>基本工资</td><td class="num"><%# decimal.Parse(Eval("BaseSalary").ToString()).ToString("N2") %></td>
                        <td>绩效</td><td class="num"><%# decimal.Parse(Eval("Performance").ToString()).ToString("N2") %></td>
                        <td>奖金</td><td class="num"><%# decimal.Parse(Eval("Bonus").ToString()).ToString("N2") %></td>
                    </tr>
                    <tr>
                        <td>加班费</td><td class="num"><%# decimal.Parse(Eval("OvertimePay").ToString()).ToString("N2") %></td>
                        <td>五险</td><td class="num" style="color:#c00;">-<%# decimal.Parse(Eval("Insurance").ToString()).ToString("N2") %></td>
                        <td>公积金</td><td class="num" style="color:#c00;">-<%# decimal.Parse(Eval("Fund").ToString()).ToString("N2") %></td>
                    </tr>
                    <tr>
                        <td>个税</td><td class="num" style="color:#c00;">-<%# decimal.Parse(Eval("Tax").ToString()).ToString("N2") %></td>
                        <td>其他扣款</td><td class="num" style="color:#c00;">-<%# decimal.Parse(Eval("Deduction").ToString()).ToString("N2") %></td>
                        <td>发薪日</td><td class="num"><%# Eval("PayDateStr") %></td>
                    </tr>
                    <tr>
                        <th colspan="5" style="text-align:right;">实发工资：</th>
                        <td class="num net-cell">¥ <%# decimal.Parse(Eval("NetSalary").ToString()).ToString("N2") %></td>
                    </tr>
                </table>
                <div style="margin-top:15px;font-size:12px;color:#999;">
                    备注：<asp:Literal runat="server" ID="litRemark" />
                    <span style="float:right;">员工签字：__________________</span>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</asp:Content>
