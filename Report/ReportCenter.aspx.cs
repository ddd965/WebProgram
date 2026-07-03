using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

public partial class Report_ReportCenter : HRMS.Common.BasePage
{
    public static List<ReportCatalog> Catalog = new List<ReportCatalog>
    {
        // 顺序严格对应功能清单 5 张核心报表 + 增值报表：
        // ①员工花名册、②部门人员清单、③月度考勤汇总、④员工工资条(逐页)、
        // ⑤休假/加班统计报表、⑥发薪历史汇总、⑦休假可视化、
        // ⑧加班月/季汇总、⑨按月考勤明细 共 9 张
        new ReportCatalog
        {
            Name = "① 员工花名册报表", File = "EmployeeRoster.aspx",
            Content = "全员花名册：工号、姓名、性别、生日、部门、职位、电话、邮箱、入职日期、在职状态、头像缩略图。",
            Filter = "部门 / 职位 / 在职状态（默认「在职」）",
            PrintHint = "隐藏筛选按钮与分页；每页 50 条；保留标题（统计日期、总人数）、头像缩略。"
        },
        new ReportCatalog
        {
            Name = "② 部门人员清单报表", File = "DeptRoster.aspx",
            Content = "按部门分组列人员：每个部门独立卡片，列工号/姓名/性别/职位/电话/邮箱/入职日期/状态；末页总合计。",
            Filter = "单部门 / 全部部门（分组显示） + 员工状态（默认在职）；支持 CSV 导出（UTF-8 BOM）。",
            PrintHint = "每个部门 page-break-after 分页；保留部门经理与人数；空部门占位显示「该部门暂无员工」。"
        },
        new ReportCatalog
        {
            Name = "③ 月度考勤汇总报表", File = "../Attendance/AttendanceStat.aspx",
            Content = "按员工汇总当月考勤：正常出勤天数、迟到次数、早退次数、缺勤天数、请假天数，合计=当月自然天数。",
            Filter = "部门 + 年 + 月；2026-05 之前为空，之后按考勤+休假记录自动生成。",
            PrintHint = "隐藏筛选与分页；保留标题年月、生成时间、员工总数；thead 每页重复。"
        },
        new ReportCatalog
        {
            Name = "④ 员工工资条报表（逐页）", File = "SalarySlip.aspx",
            Content = "每人一张独立薪资条：基本/绩效/奖金/加班费 + 五险/公积金/个税/扣款 + 实发工资；打印时逐页分页。",
            Filter = "部门 + 员工（可选）+ 薪资月份 YYYY-MM；每人独立卡片+签字栏。",
            PrintHint = "每页一人 page-break-after:always；隐藏生成条件面板；保留工资卡片与员工签字线。"
        },
        new ReportCatalog
        {
            Name = "⑤ 休假/加班统计报表", File = "LeaveOvertimeRpt.aspx",
            Content = "统一统计页：默认休假；切换「类型=加班」即加班统计；按员工×分组维度：工作日/周末/节假日或各类休假 小时/天数+次数。",
            Filter = "类型（休假/加班）+ 部门 + 年 + 月份（留空=全年）；支持 CSV 导出（UTF-8 BOM）。",
            PrintHint = "页脚合计行；隐藏估算金额；CSV 自动写 BOM 避免 Excel 乱码。"
        },
        new ReportCatalog
        {
            Name = "⑥ 发薪历史汇总", File = "../Salary/SalaryStat.aspx",
            Content = "按部门或月份聚合：员工数/基本合计/绩效/奖金/加班费/扣款/实发合计/人均实发；附折线趋势+CSV导出。",
            Filter = "部门（可选）+ 起止月份 + 汇总方式（按部门 / 按月份）",
            PrintHint = "打印保留 Canvas 趋势图；一键导出 CSV（UTF-8 BOM + 合计行）。"
        },
        new ReportCatalog
        {
            Name = "⑦ 休假统计可视化", File = "../Leave/LeaveStat.aspx",
            Content = "Chart.js 可视化：休假柱状图(按部门/类型/月份分组) + 环形图占比；维度切换：部门/类型/月份/员工明细。",
            Filter = "年 + 部门（可选，默认全公司）+ 维度",
            PrintHint = "保留 Canvas 渲染结果（Chart.js 打印不丢失）；显示数据标签。"
        },
        new ReportCatalog
        {
            Name = "⑧ 加班月/季度汇总", File = "../Overtime/OvertimeStat.aspx",
            Content = "按月或季度 × 部门 × 员工：工作日/周末/节假日加班小时数、总小时、总次数；全体员工0数据占位不丢。",
            Filter = "部门 + 年 + 月/季度 + 范围（月/季度）；已移除回写工资加班费按钮。",
            PrintHint = "显示汇总合计；打印隐藏筛选与分页，保留图表与汇总数据表格。"
        },
        new ReportCatalog
        {
            Name = "⑨ 按月考勤明细", File = "../Attendance/AttendanceDetail.aspx",
            Content = "按员工×日期：签到/签退时间、工时、状态（正常/迟到/早退/缺勤/请假/周末）；列表+日历双视图。",
            Filter = "员工 + 年月 + 视图切换（列表/日历）；2026-05 之前为空。",
            PrintHint = "列表/日历两种视图打印；日历每页一个月；周末灰色底色；保留状态颜色图例。"
        }
    };

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // 绑定 ListBox：Text = 报表名称；Value = 跳转路径
            lbReports.Items.Clear();
            foreach (var r in Catalog)
                lbReports.Items.Add(new ListItem(r.Name, r.File));
            lbReports.SelectedIndex = 0;

            // 服务端默认描述（首屏）
            ShowDescription(Catalog[0]);

            // 生成客户端 JSON 映射（key=File；用于 onchange 即时更新）
            var ser = new JavaScriptSerializer();
            var dict = Catalog.ToDictionary(r => r.File, r => new
            {
                Name = r.Name,
                Content = r.Content,
                Filter = r.Filter,
                PrintHint = r.PrintHint,
                File = r.File
            });
            litDescJSON.Text = ser.Serialize(dict) + ";";
        }
    }

    protected void btnGo_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(lbReports.SelectedValue)) return;
        Response.Redirect(lbReports.SelectedValue, true);
    }

    private void ShowDescription(ReportCatalog r)
    {
        if (r == null) { litDescription.Text = "<p class=\"text-muted\">（选择左侧报表查看详细说明）</p>"; return; }
        var sb = new StringBuilder();
        sb.AppendFormat("<h4>{0}</h4>", Server.HtmlEncode(r.Name));
        sb.AppendFormat("<p><strong>包含字段/内容：</strong><br/>&nbsp;&nbsp;{0}</p>", Server.HtmlEncode(r.Content));
        sb.AppendFormat("<p><strong>筛选条件：</strong><br/>&nbsp;&nbsp;{0}</p>", Server.HtmlEncode(r.Filter));
        sb.AppendFormat("<p><strong>@media print 输出特点：</strong><br/>&nbsp;&nbsp;{0}</p>", Server.HtmlEncode(r.PrintHint));
        sb.AppendFormat("<p class=\"text-info\"><strong>页面路径：</strong> <code>{0}</code></p>", Server.HtmlEncode(r.File));
        litDescription.Text = sb.ToString();
    }

    public class ReportCatalog
    {
        public string Name { get; set; }
        public string File { get; set; }
        public string Content { get; set; }
        public string Filter { get; set; }
        public string PrintHint { get; set; }
    }
}
