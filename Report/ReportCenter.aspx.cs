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
        // 顺序必须与需求一致：
        // 1 部门人员清单报表、2 月度考勤汇总报表、3 员工工资条报表、
        // 4 休假统计报表、5 加班统计报表、6 发薪历史汇总、
        // 7 休假统计可视化、8 加班月/季度汇总、9 按月考勤明细
        new ReportCatalog
        {
            Name = "部门人员清单报表", File = "EmployeeRoster.aspx",
            Content = "工号、姓名、性别、生日、部门、职位、电话、邮箱、入职日期、在职状态、头像等完整员工信息清单。",
            Filter = "部门 / 职位 / 在职状态（默认「在职」）",
            PrintHint = "隐藏筛选按钮与分页控件；每页 50 条；保留报告标题（统计日期、总人数）"
        },
        new ReportCatalog
        {
            Name = "月度考勤汇总报表", File = "AttendanceMonthly.aspx",
            Content = "按员工汇总当月考勤：正常出勤天数、迟到次数、早退次数、缺勤天数、请假天数，合计等于当月自然天数。",
            Filter = "部门 + 年 + 月；2026-05 月之前为空，之后数据按考勤+休假记录自动生成。",
            PrintHint = "隐藏筛选条件与分页；保留标题年月、生成时间、员工总数；thead 每页重复。"
        },
        new ReportCatalog
        {
            Name = "员工工资条报表", File = "SalarySlip.aspx",
            Content = "每位员工一张独立工资条：基本工资、绩效、奖金、加班费、五险、公积金、个税、其他扣款、实发工资、员工签字栏。",
            Filter = "部门 / 员工 / 薪资月份（YYYY-MM）",
            PrintHint = "CSS page-break-after 保证每页一张工资条；打印时自动隐藏顶部批量按钮。"
        },
        new ReportCatalog
        {
            Name = "休假统计报表", File = "LeaveOvertimeRpt.aspx?type=leave",
            Content = "按员工×休假类型分组：年假、事假、病假、调休、婚假、产假、丧假、其他的天数与次数合计；支持导出 CSV。",
            Filter = "类型=休假 + 部门 + 年份 + 月份（月份留空=全年统计）",
            PrintHint = "页脚合计行；CSV 导出自动写入 UTF-8 BOM，Excel 打开不乱码；图表默认不打印。"
        },
        new ReportCatalog
        {
            Name = "加班统计报表", File = "LeaveOvertimeRpt.aspx?type=ot",
            Content = "按员工分组：工作日、周末、节假日三类加班小时数；按 1.5/2/3 倍估算加班费；总小时、总次数、总金额。",
            Filter = "类型=加班 + 部门 + 年份 + 月份（月份留空=全年统计）",
            PrintHint = "页脚合计行；CSV 导出自动写 BOM；与工资模块对接可回写加班费。"
        },
        new ReportCatalog
        {
            Name = "发薪历史汇总", File = "../Salary/SalaryStat.aspx",
            Content = "按部门或月份维度聚合：员工数、基本工资合计、绩效、奖金、加班费、扣款、实发合计、人均实发；附 CSV 导出与折线趋势。",
            Filter = "部门（可选）+ 起止月份 + 汇总方式（按部门 / 按月份）",
            PrintHint = "打印时保留趋势图（Canvas 截图）；一键导出 CSV（UTF-8 BOM + 合计行）。"
        },
        new ReportCatalog
        {
            Name = "休假统计可视化", File = "../Leave/LeaveStat.aspx",
            Content = "Chart.js 可视化：各部门休假天数（柱状图）、休假类型占比（饼图）、每月休假次数趋势（折线图）。",
            Filter = "年 + 部门（可选，默认全公司）",
            PrintHint = "浏览器打印时保留 Canvas 渲染结果；显示数据标签。"
        },
        new ReportCatalog
        {
            Name = "加班月/季度汇总", File = "../Overtime/OvertimeStat.aspx",
            Content = "按月或按季度 × 部门 × 员工汇总工作日/周末/节假日加班小时；按倍率计算预估加班费；一键回写工资模块 OvertimePay。",
            Filter = "部门 + 年 + 月/季度 + 范围（月 / 季度）",
            PrintHint = "显示汇总合计；回写动作仅在浏览器端执行，不影响打印输出。"
        },
        new ReportCatalog
        {
            Name = "按月考勤明细", File = "../Attendance/AttendanceDetail.aspx",
            Content = "按员工×日期维度展示考勤明细：签到时间、签退时间、工时、状态（正常/迟到/早退/缺勤/请假/周末）；支持列表视图与日历视图。",
            Filter = "员工 + 年月 + 视图切换（列表 / 日历）；2026-05 之前为空，之后仅显示至系统今日。",
            PrintHint = "日历模式每页一个月；周末灰色底；保留状态颜色图例。"
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
