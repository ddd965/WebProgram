<%@ Page Title="加班汇总" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="OvertimeStat.aspx.cs" Inherits="Overtime_OvertimeStat" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>加班汇总统计（按月/季度）</h2>
    <hr />

    <div class="panel panel-default no-print">
        <div class="panel-heading">统计条件</div>
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
                    <div class="form-group"><label>年份</label>
                        <asp:DropDownList runat="server" ID="ddlYear" CssClass="form-control" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label>周期类型</label>
                        <asp:DropDownList runat="server" ID="ddlRange" CssClass="form-control" AutoPostBack="True" OnSelectedIndexChanged="ddlRange_SelectedIndexChanged">
                            <asp:ListItem Text="月度" Value="month" Selected="True" />
                            <asp:ListItem Text="季度" Value="quarter" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group"><label id="lblPeriodLabel" runat="server">月份</label>
                        <asp:DropDownList runat="server" ID="ddlPeriod" CssClass="form-control">
                            <asp:ListItem Text="1月" Value="1" />
                            <asp:ListItem Text="2月" Value="2" />
                            <asp:ListItem Text="3月" Value="3" />
                            <asp:ListItem Text="4月" Value="4" />
                            <asp:ListItem Text="5月" Value="5" />
                            <asp:ListItem Text="6月" Value="6" />
                            <asp:ListItem Text="7月" Value="7" />
                            <asp:ListItem Text="8月" Value="8" />
                            <asp:ListItem Text="9月" Value="9" />
                            <asp:ListItem Text="10月" Value="10" />
                            <asp:ListItem Text="11月" Value="11" />
                            <asp:ListItem Text="12月" Value="12" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnStat" Text="统计" OnClick="btnStat_Click" CssClass="btn btn-primary" />
                        <asp:Button runat="server" ID="btnPrint" Text="打印" OnClientClick="window.print();return false;" CssClass="btn btn-info" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <%-- Chart 图 --%>
    <div class="panel panel-default" style="page-break-inside:avoid;">
        <div class="panel-heading">部门加班时长汇总图</div>
        <div class="panel-body">
            <canvas id="otChartBar" height="90" style="image-rendering:-webkit-optimize-contrast;image-rendering:crisp-edges;-webkit-print-color-adjust:exact;print-color-adjust:exact;"></canvas>
            <canvas id="otChartPie" height="90" style="margin-top:15px;image-rendering:-webkit-optimize-contrast;image-rendering:crisp-edges;-webkit-print-color-adjust:exact;print-color-adjust:exact;"></canvas>
        </div>
    </div>
    <asp:HiddenField runat="server" ID="hidLabels" />
    <asp:HiddenField runat="server" ID="hidHours" />

    <div class="table-responsive">
    <asp:GridView runat="server" ID="gvStat" CssClass="table table-striped table-bordered table-condensed"
        AutoGenerateColumns="False" AllowPaging="False">
        <Columns>
            <asp:TemplateField HeaderText="分组（员工/部门）">
                <ItemTemplate><%# Eval("GroupKey") %></ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Workday" HeaderText="工作日(h)" DataFormatString="{0:0.0}" />
            <asp:BoundField DataField="Weekend" HeaderText="周末(h)" DataFormatString="{0:0.0}" />
            <asp:BoundField DataField="Holiday" HeaderText="节假日(h)" DataFormatString="{0:0.0}" />
            <asp:BoundField DataField="TotalHours" HeaderText="合计(h)" DataFormatString="{0:0.0}" />
            <asp:BoundField DataField="RecordCount" HeaderText="次数" />
        </Columns>
        <PagerStyle CssClass="pagination-ys no-print" HorizontalAlign="Center" />
    </asp:GridView>
    </div>

    <%-- 部门级汇总 --%>
    <h4>按部门汇总</h4>
    <div class="table-responsive">
    <asp:GridView runat="server" ID="gvDept" CssClass="table table-bordered table-condensed"
        AutoGenerateColumns="False" AllowPaging="False" style="page-break-inside:avoid;">
        <Columns>
            <asp:BoundField DataField="Period" HeaderText="周期" />
            <asp:BoundField DataField="DeptName" HeaderText="部门" />
            <asp:BoundField DataField="DeptEmpCount" HeaderText="部门人数" />
            <asp:BoundField DataField="OtEmpCount" HeaderText="加班人数" ItemStyle-CssClass="text-warning" />
            <asp:BoundField DataField="TotalHours" HeaderText="总时长(h)" DataFormatString="{0:0.0}" />
        </Columns>
    </asp:GridView>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    <script>
        (function () {
            function p(s) { try { return JSON.parse(s || '[]'); } catch (e) { return []; } }
            var labels = p(document.getElementById('<%= hidLabels.ClientID %>').value);
            var hours = p(document.getElementById('<%= hidHours.ClientID %>').value);
            if (!labels.length) return;
            var palette = ['#3366CC', '#DC3912', '#FF9900', '#109618', '#990099',
                '#3B3EAC', '#0099C6', '#DD4477', '#66AA00', '#B82E2E', '#316395', '#994499'];
            new Chart(document.getElementById('otChartBar'), {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [
                        { label: '总时长(h)', data: hours, backgroundColor: palette[0] }
                    ]
                },
                options: {
                    responsive: true,
                    title: { display: true, text: '各部门加班时长' },
                    scales: { y: { beginAtZero: true } }
                }
            });
            new Chart(document.getElementById('otChartPie'), {
                type: 'doughnut',
                data: {
                    labels: labels,
                    datasets: [{ data: hours, backgroundColor: palette.slice(0, labels.length), label: '时长占比' }]
                },
                options: { title: { display: true, text: '加班时长占比' } }
            });
        })();
    </script>
</asp:Content>
