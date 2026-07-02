<%@ Page Title="部门工资汇总 + 发薪历史" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="SalaryStat.aspx.cs" Inherits="Salary_SalaryStat" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>部门工资汇总与发薪历史（支持导出 CSV）</h2>
    <hr />

    <div class="panel panel-default no-print">
        <div class="panel-heading">汇总条件</div>
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
                    <div class="form-group"><label>起始月份</label>
                        <asp:TextBox runat="server" ID="txtMonthFrom" CssClass="form-control" placeholder="YYYY-MM" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtMonthFrom"
                            ValidationExpression="([1-2][0-9]{3}-[0-1][0-9])?" ErrorMessage="YYYY-MM"
                            CssClass="text-danger" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label>结束月份</label>
                        <asp:TextBox runat="server" ID="txtMonthTo" CssClass="form-control" placeholder="YYYY-MM" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtMonthTo"
                            ValidationExpression="([1-2][0-9]{3}-[0-1][0-9])?" ErrorMessage="YYYY-MM"
                            CssClass="text-danger" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label>汇总方式</label>
                        <asp:DropDownList runat="server" ID="ddlGroup" CssClass="form-control">
                            <asp:ListItem Text="按部门汇总（当前月/区间合计）" Value="dept" Selected="True" />
                            <asp:ListItem Text="发薪历史汇总（按月份）" Value="history" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnStat" Text="汇总" OnClick="btnStat_Click" CssClass="btn btn-primary" />
                        <asp:Button runat="server" ID="btnPrint" Text="打印" OnClientClick="window.print();return false;" CssClass="btn btn-info" />
                        <asp:Button runat="server" ID="btnExport" Text="导出 CSV" OnClick="btnExport_Click" CssClass="btn btn-success" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <%-- Chart --%>
    <div class="panel panel-default" style="page-break-inside:avoid;">
        <div class="panel-heading">薪资趋势图</div>
        <div class="panel-body">
            <canvas id="salaryChart" height="90"></canvas>
        </div>
    </div>
    <asp:HiddenField runat="server" ID="hidLabels" />
    <asp:HiddenField runat="server" ID="hidNets" />
    <asp:HiddenField runat="server" ID="hidBases" />

    <div class="table-responsive">
    <asp:GridView runat="server" ID="gvStat" CssClass="table table-striped table-bordered table-condensed"
        AutoGenerateColumns="False" AllowPaging="True" PageSize="20"
        OnPageIndexChanging="gvStat_PageIndexChanging"
        ShowFooter="True">
        <Columns>
            <asp:TemplateField HeaderText="分组">
                <ItemTemplate><%# Eval("Group") %></ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="EmpCount" HeaderText="员工数" />
            <asp:BoundField DataField="TotalBase" HeaderText="基本工资合计" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="TotalPerf" HeaderText="绩效合计" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="TotalBonus" HeaderText="奖金合计" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="TotalOtPay" HeaderText="加班费合计" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="TotalDeduct" HeaderText="扣款合计" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="TotalNet" HeaderText="实发工资合计" DataFormatString="{0:N2}"
                ItemStyle-ForeColor="#2b8cbe" ItemStyle-Font-Bold="true" />
            <asp:BoundField DataField="AvgNet" HeaderText="人均实发" DataFormatString="{0:N2}" />
        </Columns>
        <FooterStyle BackColor="#f5f5f5" Font-Bold="true" />
        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
    </asp:GridView>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    <script>
        (function () {
            function p(s) { try { return JSON.parse(s || '[]'); } catch (e) { return []; } }
            var labels = p(document.getElementById('<%= hidLabels.ClientID %>').value);
            var nets = p(document.getElementById('<%= hidNets.ClientID %>').value);
            var bases = p(document.getElementById('<%= hidBases.ClientID %>').value);
            if (!labels.length) return;
            new Chart(document.getElementById('salaryChart'), {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [
                        { label: '实发合计', data: nets, borderColor: '#2b8cbe', backgroundColor: 'rgba(43,140,190,0.2)', fill: true, tension: 0.2 },
                        { label: '基本合计', data: bases, borderColor: '#fc8d59', backgroundColor: 'rgba(252,141,89,0.15)', fill: true, tension: 0.2 }
                    ]
                },
                options: {
                    responsive: true,
                    title: { display: true, text: '工资趋势' },
                    scales: { y: { beginAtZero: true } }
                }
            });
        })();
    </script>
</asp:Content>
