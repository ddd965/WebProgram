<%@ Page Title="休假统计汇总" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="LeaveStat.aspx.cs" Inherits="Leave_LeaveStat" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>休假统计汇总（含 Chart 图）</h2>
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
                <div class="col-md-3">
                    <div class="form-group"><label>维度</label>
                        <asp:DropDownList runat="server" ID="ddlDimension" CssClass="form-control">
                            <asp:ListItem Text="按部门汇总" Value="dept" Selected="True" />
                            <asp:ListItem Text="按休假类型汇总" Value="type" />
                            <asp:ListItem Text="按月份汇总（1-12月）" Value="month" />
                            <asp:ListItem Text="按员工明细" Value="emp" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnStat" Text="统计" OnClick="btnStat_Click" CssClass="btn btn-primary btn-block" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>&nbsp;</label>
                        <asp:Button runat="server" ID="btnPrint" Text="打印报表" OnClientClick="window.print();return false;" CssClass="btn btn-info btn-block" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <asp:Label runat="server" ID="lblMsg" CssClass="text-info" />

    <%-- Chart 图 --%>
    <div class="panel panel-default" style="page-break-inside:avoid;">
        <div class="panel-heading">统计图表</div>
        <div class="panel-body">
            <canvas id="leaveChart" height="100"></canvas>
            <canvas id="leavePieChart" height="100" style="margin-top:20px;"></canvas>
        </div>
    </div>

    <asp:HiddenField runat="server" ID="hidLabels" />
    <asp:HiddenField runat="server" ID="hidTotalDays" />
    <asp:HiddenField runat="server" ID="hidApprovedDays" />

    <div class="table-responsive">
    <asp:GridView runat="server" ID="gvStat" CssClass="table table-striped table-bordered table-condensed"
        AutoGenerateColumns="False" AllowPaging="True" PageSize="20"
        OnPageIndexChanging="gvStat_PageIndexChanging">
        <Columns>
            <asp:TemplateField HeaderText="分组项">
                <ItemTemplate><%# Eval("GroupKey") %></ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="申请次数" HeaderText="申请次数" />
            <asp:BoundField DataField="已批准天数" HeaderText="已批准天数" DataFormatString="{0:0.#}" />
            <asp:BoundField DataField="待审批天数" HeaderText="待审批天数" DataFormatString="{0:0.#}" />
            <asp:BoundField DataField="总天数" HeaderText="总天数" DataFormatString="{0:0.#}" />
        </Columns>
        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
    </asp:GridView>
    </div>

    <%-- 使用轻量 CDN Chart.js --%>
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    <script>
        (function () {
            function parseArr(s) {
                if (!s) return [];
                try { return JSON.parse(s); } catch (e) { return []; }
            }
            var labels = parseArr(document.getElementById('<%= hidLabels.ClientID %>').value);
            var totals = parseArr(document.getElementById('<%= hidTotalDays.ClientID %>').value);
            var approv = parseArr(document.getElementById('<%= hidApprovedDays.ClientID %>').value);
            if (labels.length === 0) return;

            var palette = ['#3366CC', '#DC3912', '#FF9900', '#109618', '#990099',
                '#3B3EAC', '#0099C6', '#DD4477', '#66AA00', '#B82E2E', '#316395',
                '#994499', '#22AA99', '#AAAA11', '#6633CC', '#E67300'];

            new Chart(document.getElementById('leaveChart'), {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [
                        { label: '总天数', data: totals, backgroundColor: palette[0] },
                        { label: '已批准天数', data: approv, backgroundColor: palette[3] }
                    ]
                },
                options: {
                    responsive: true,
                    title: { display: true, text: '休假天数分组柱状图' },
                    scales: { y: { beginAtZero: true } }
                }
            });

            new Chart(document.getElementById('leavePieChart'), {
                type: 'doughnut',
                data: {
                    labels: labels,
                    datasets: [{
                        label: '总天数占比',
                        data: totals,
                        backgroundColor: palette.slice(0, labels.length)
                    }]
                },
                options: { title: { display: true, text: '休假占比环形图' } }
            });
        })();
    </script>
</asp:Content>
