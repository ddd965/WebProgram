<%@ Page Title="工资录入" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="SalaryEdit.aspx.cs" Inherits="Salary_SalaryEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>发薪录入</h2>
    <hr />

    <div class="panel panel-default no-print">
        <div class="panel-heading">发薪记录录入 <small style="color:#999;">（NetSalary = 基本工资+绩效+奖金+加班费 - 五险-公积金-个税-其他扣款）</small></div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group">
                        <label>员工 *</label>
                        <asp:DropDownList runat="server" ID="ddlEmployee" CssClass="form-control" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlEmployee_SelectedIndexChanged" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>薪资月份 *</label>
                        <asp:TextBox runat="server" ID="txtSalaryMonth" CssClass="form-control" placeholder="YYYY-MM" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtSalaryMonth"
                            CssClass="text-danger" ErrorMessage="必填" Display="Dynamic" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtSalaryMonth"
                            ValidationExpression="[1-2][0-9]{3}-[0-1][0-9]" ErrorMessage="YYYY-MM格式"
                            CssClass="text-danger" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>基本工资 *</label>
                        <asp:TextBox runat="server" ID="txtBase" CssClass="form-control salary-input" Text="0" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>绩效 *</label>
                        <asp:TextBox runat="server" ID="txtPerf" CssClass="form-control salary-input" Text="0" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>奖金 *</label>
                        <asp:TextBox runat="server" ID="txtBonus" CssClass="form-control salary-input" Text="0" />
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-2">
                    <div class="form-group">
                        <label>加班费 *</label>
                        <asp:TextBox runat="server" ID="txtOtPay" CssClass="form-control salary-input" Text="0" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>五险 *</label>
                        <asp:TextBox runat="server" ID="txtIns" CssClass="form-control salary-input" Text="0" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>公积金 *</label>
                        <asp:TextBox runat="server" ID="txtFund" CssClass="form-control salary-input" Text="0" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>个税 *</label>
                        <asp:TextBox runat="server" ID="txtTax" CssClass="form-control salary-input" Text="0" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>其他扣款 *</label>
                        <asp:TextBox runat="server" ID="txtDed" CssClass="form-control salary-input" Text="0" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>实发工资</label>
                        <asp:TextBox runat="server" ID="txtNet" CssClass="form-control" ReadOnly="true" style="font-weight:bold;color:#2b8cbe;" />
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group">
                        <label>发薪日期</label>
                        <asp:TextBox runat="server" ID="txtPayDate" CssClass="form-control" TextMode="Date" />
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-group">
                        <label>备注</label>
                        <asp:TextBox runat="server" ID="txtRemark" CssClass="form-control" MaxLength="200" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnCalc" Text="自动计算" OnClick="btnCalc_Click" CssClass="btn btn-default" CausesValidation="false" />
                        <asp:Button runat="server" ID="btnSave" Text="保存/覆盖" OnClick="btnSave_Click" CssClass="btn btn-primary" />
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <p class="text-muted">
                        导航：<a href="SalaryList.aspx">工资列表</a> |
                        <a href="SalaryStat.aspx">发薪历史汇总（部门/全公司/月份合计 + 导出CSV）</a> |
                        <a href="../Report/SalarySlip.aspx">工资条打印（逐页）</a>
                    </p>
                </div>
            </div>
        </div>
    </div>

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />
</asp:Content>
