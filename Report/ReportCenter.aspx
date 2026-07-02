<%@ Page Title="报表打印中心" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="ReportCenter.aspx.cs" Inherits="Report_ReportCenter" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>报表打印中心</h2>
    <p class="text-muted">请在下方选择要打印的报表，单击左侧即可查看右侧的详细说明，双击可直接跳转到对应报表页面。各报表页面内置「在线打印」按钮，CSS @media print 已自动隐藏导航栏 / 按钮 / 分页控件，输出即标准打印样式。</p>
    <hr />

    <div class="row no-print">
        <div class="col-md-6">
            <div class="panel panel-default">
                <div class="panel-heading"><strong>① 选择报表（单选，双击即可跳转）</strong></div>
                <div class="panel-body">
                    <asp:ListBox runat="server" ID="lbReports" Rows="10" CssClass="form-control"
                        AutoPostBack="false" ondblclick="goJump();" onchange="onRptChange(this.value);">
                    </asp:ListBox>
                    <div style="margin-top:12px;">
                        <asp:Button runat="server" ID="btnGo" Text="▶ 前往生成 / 打印" CssClass="btn btn-primary btn-lg"
                            OnClick="btnGo_Click" />
                        <asp:Button runat="server" ID="btnPrint" Text="打印本说明（可选）"
                            OnClientClick="window.print();return false;" CssClass="btn btn-default" />
                    </div>
                </div>
            </div>
        </div>

        <div class="col-md-6">
            <div class="panel panel-info">
                <div class="panel-heading"><strong>② 报表说明 / 包含内容</strong></div>
                <div class="panel-body">
                    <div id="descDiv" style="min-height:260px;">
                        <asp:Literal runat="server" ID="litDescription" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        // 客户端描述映射（跟随选中项即时更新，无需 PostBack）
        var __rptDescMap = <asp:Literal runat="server" ID="litDescJSON" />;
        function escapeHtml(s) {
            var div = document.createElement('div');
            div.appendChild(document.createTextNode(s));
            return div.innerHTML;
        }
        function onRptChange(val) {
            var el = document.getElementById('descDiv');
            if (!__rptDescMap || !__rptDescMap[val]) {
                el.innerHTML = '<p class="text-muted">（选择左侧报表查看详细说明）</p>';
                return;
            }
            var r = __rptDescMap[val];
            var html = ''
                + '<h4>' + escapeHtml(r.Name) + '</h4>'
                + '<p><strong>包含字段/内容：</strong><br/>&nbsp;&nbsp;' + escapeHtml(r.Content) + '</p>'
                + '<p><strong>筛选条件：</strong><br/>&nbsp;&nbsp;' + escapeHtml(r.Filter) + '</p>'
                + '<p><strong>@media print 输出特点：</strong><br/>&nbsp;&nbsp;' + escapeHtml(r.PrintHint) + '</p>'
                + '<p class="text-info"><strong>页面路径：</strong> <code>' + escapeHtml(r.File) + '</code></p>';
            el.innerHTML = html;
        }
        function goJump() {
            var lb = document.getElementById('<%= lbReports.ClientID %>');
            if (lb.selectedIndex < 0) return;
            var url = lb.options[lb.selectedIndex].value;
            if (!url) return;
            window.location.href = url;
        }
    </script>
</asp:Content>
