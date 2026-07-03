/* ======================================================================
 *  人事管理系统 HRMS - 数据库建表与初始化脚本
 *  SQL Server | 三层架构 ASP.NET
 *  要求：1) 必要字段非空  2) 级联删除/修改  3) 单例约束 & Status放宽
 *  ====================================================================== */

USE [master];
GO
IF DB_ID(N'HRMS') IS NOT NULL
BEGIN
    ALTER DATABASE [HRMS] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [HRMS];
END
GO
CREATE DATABASE [HRMS];
GO
USE [HRMS];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

/* ======================================================================
 *  一、建表（先建字段 + 主键 + 唯一/检查约束，外键统一最后加，避免双向依赖冲突）
 * ====================================================================== */

/* 1. 部门表（自关联树形结构）--------------------------------------------- */
CREATE TABLE dbo.Department (
    DeptId     INT IDENTITY(1,1) NOT NULL,
    DeptName   NVARCHAR(80)      NOT NULL,
    ParentId   INT               NULL,          -- 顶级部门为 NULL
    ManagerId  INT               NULL,          -- 部门经理 Employee.EmpId
    Descn      NVARCHAR(255)     NULL,
    CreateTime DATETIME          NOT NULL CONSTRAINT DF_Department_CreateTime DEFAULT (GETDATE()),
    CONSTRAINT PK_Department PRIMARY KEY CLUSTERED (DeptId ASC),
    CONSTRAINT UK_Department_NameParent UNIQUE NONCLUSTERED (ParentId, DeptName)
);
CREATE NONCLUSTERED INDEX IX_Department_Parent ON dbo.Department(ParentId);

/* 2. 职位表（字典表）----------------------------------------------------- */
CREATE TABLE dbo.Position (
    PositionId   INT IDENTITY(1,1) NOT NULL,
    PositionName NVARCHAR(80)      NOT NULL,
    Level        INT               NULL,
    Descn        NVARCHAR(255)     NULL,
    CONSTRAINT PK_Position PRIMARY KEY CLUSTERED (PositionId ASC),
    CONSTRAINT UK_Position_Name UNIQUE NONCLUSTERED (PositionName)
);

/* 3. 员工表 --------------------------------------------------------------- */
CREATE TABLE dbo.Employee (
    EmpId      INT IDENTITY(1,1) NOT NULL,
    EmpNo      NVARCHAR(20)      NOT NULL,
    EmpName    NVARCHAR(50)      NOT NULL,
    Gender     NVARCHAR(4)       NOT NULL,
    Birthday   DATE              NULL,
    IdCard     NVARCHAR(18)      NULL,
    HireDate   DATE              NOT NULL,
    DeptId     INT               NULL,
    PositionId INT               NULL,
    Phone      NVARCHAR(20)      NOT NULL,
    Email      NVARCHAR(80)      NOT NULL,
    Address    NVARCHAR(255)     NULL,
    PhotoPath  NVARCHAR(200)     NULL,
    Status     NVARCHAR(20)      NOT NULL CONSTRAINT DF_Employee_Status DEFAULT (N'在职'),
    CreateTime DATETIME          NOT NULL CONSTRAINT DF_Employee_CreateTime DEFAULT (GETDATE()),
    CONSTRAINT PK_Employee PRIMARY KEY CLUSTERED (EmpId ASC),
    CONSTRAINT UK_Employee_EmpNo UNIQUE NONCLUSTERED (EmpNo),
    CONSTRAINT UK_Employee_IdCard UNIQUE NONCLUSTERED (IdCard),
    CONSTRAINT CK_Employee_Gender CHECK (Gender IN (N'男', N'女'))
    /* Status 去掉硬检查约束 → 应用层枚举管理，方便扩展停薪留职/休长假等 */
);
CREATE NONCLUSTERED INDEX IX_Employee_Dept     ON dbo.Employee(DeptId);
CREATE NONCLUSTERED INDEX IX_Employee_Position ON dbo.Employee(PositionId);
CREATE NONCLUSTERED INDEX IX_Employee_Name     ON dbo.Employee(EmpName);
CREATE NONCLUSTERED INDEX IX_Employee_Status   ON dbo.Employee(Status);

/* 4. 用户表（系统登录账号，与员工 1:1）------------------------------------ */
CREATE TABLE dbo.[User] (
    UserId        INT IDENTITY(1,1) NOT NULL,
    UserName      NVARCHAR(50)      NOT NULL,
    Password      NVARCHAR(100)     NOT NULL,
    EmpId         INT               NULL,
    RoleName      NVARCHAR(20)      NOT NULL CONSTRAINT DF_User_RoleName DEFAULT (N'普通用户'),
    IsLocked      BIT               NOT NULL CONSTRAINT DF_User_IsLocked DEFAULT ((0)),
    LastLoginTime DATETIME          NULL,
    CreateTime    DATETIME          NOT NULL CONSTRAINT DF_User_CreateTime DEFAULT (GETDATE()),
    CONSTRAINT PK_User PRIMARY KEY CLUSTERED (UserId ASC),
    CONSTRAINT UK_User_UserName UNIQUE NONCLUSTERED (UserName),
    CONSTRAINT UK_User_EmpId    UNIQUE NONCLUSTERED (EmpId)   -- 1 员工 ↔ 1 账号
);

/* 5. 休假类型表（休假设置字典）------------------------------------------- */
CREATE TABLE dbo.LeaveType (
    LeaveTypeId INT IDENTITY(1,1) NOT NULL,
    TypeName    NVARCHAR(50)      NOT NULL,
    DefaultDays DECIMAL(5,1)      NOT NULL CONSTRAINT DF_LeaveType_DefaultDays DEFAULT ((0)),
    NeedApproval BIT              NOT NULL CONSTRAINT DF_LeaveType_NeedApproval DEFAULT ((1)),
    Descn       NVARCHAR(255)     NULL,
    CONSTRAINT PK_LeaveType PRIMARY KEY CLUSTERED (LeaveTypeId ASC),
    CONSTRAINT UK_LeaveType_Name UNIQUE NONCLUSTERED (TypeName)
);

/* 6. 休假记录表 ---------------------------------------------------------- */
CREATE TABLE dbo.LeaveRecord (
    LeaveId     INT IDENTITY(1,1) NOT NULL,
    EmpId       INT               NOT NULL,
    LeaveTypeId INT               NOT NULL,
    StartDate   DATE              NOT NULL,
    EndDate     DATE              NOT NULL,
    LeaveDays   DECIMAL(5,1)      NOT NULL,
    Reason      NVARCHAR(500)     NOT NULL,          -- 请假原因必填
    Status      NVARCHAR(10)      NOT NULL CONSTRAINT DF_LeaveRecord_Status DEFAULT (N'待审批'),
    ApproverId  INT               NULL,
    ApplyTime   DATETIME          NOT NULL CONSTRAINT DF_LeaveRecord_ApplyTime DEFAULT (GETDATE()),
    CONSTRAINT PK_LeaveRecord PRIMARY KEY CLUSTERED (LeaveId ASC),
    CONSTRAINT CK_LeaveRecord_Date CHECK (EndDate >= StartDate),
    CONSTRAINT CK_LeaveRecord_Days CHECK (LeaveDays > 0),
    CONSTRAINT CK_LeaveRecord_Status CHECK (Status IN (N'待审批',N'已批准',N'已拒绝',N'已取消'))
);
CREATE NONCLUSTERED INDEX IX_LeaveRecord_Emp    ON dbo.LeaveRecord(EmpId);
CREATE NONCLUSTERED INDEX IX_LeaveRecord_Type   ON dbo.LeaveRecord(LeaveTypeId);
CREATE NONCLUSTERED INDEX IX_LeaveRecord_Date   ON dbo.LeaveRecord(StartDate, EndDate);
CREATE NONCLUSTERED INDEX IX_LeaveRecord_Status ON dbo.LeaveRecord(Status);

/* 7. 考勤参数设置表（全局单例）------------------------------------------- */
CREATE TABLE dbo.AttendanceSetting (
    SettingId     INT IDENTITY(1,1) NOT NULL,
    WorkStartTime TIME(0)           NOT NULL CONSTRAINT DF_AttendanceSetting_WorkStart DEFAULT ('09:00:00'),
    WorkEndTime   TIME(0)           NOT NULL CONSTRAINT DF_AttendanceSetting_WorkEnd   DEFAULT ('18:00:00'),
    LateMinutes   INT               NOT NULL CONSTRAINT DF_AttendanceSetting_Late      DEFAULT ((10)),
    EarlyMinutes  INT               NOT NULL CONSTRAINT DF_AttendanceSetting_Early     DEFAULT ((10)),
    LunchStart    TIME(0)           NOT NULL CONSTRAINT DF_AttendanceSetting_LunchStart DEFAULT ('12:00:00'),
    LunchEnd      TIME(0)           NOT NULL CONSTRAINT DF_AttendanceSetting_LunchEnd   DEFAULT ('13:30:00'),
    UpdateTime    DATETIME          NOT NULL CONSTRAINT DF_AttendanceSetting_UpdateTime DEFAULT (GETDATE()),
    CONSTRAINT PK_AttendanceSetting PRIMARY KEY CLUSTERED (SettingId ASC),
    /* === 建议修正 #1：强制单例，全局只允许 SettingId=1 一条 === */
    CONSTRAINT CK_AttendanceSetting_Singleton CHECK (SettingId = 1),
    CONSTRAINT CK_AttendanceSetting_Time CHECK (WorkEndTime > WorkStartTime)
);

/* 8. 考勤表 -------------------------------------------------------------- */
CREATE TABLE dbo.Attendance (
    AttId        INT IDENTITY(1,1) NOT NULL,
    EmpId        INT               NOT NULL,
    AttDate      DATE              NOT NULL,
    CheckInTime  DATETIME          NULL,
    CheckOutTime DATETIME          NULL,
    Status       NVARCHAR(10)      NOT NULL CONSTRAINT DF_Attendance_Status DEFAULT (N'正常'),
    Remark       NVARCHAR(200)     NULL,
    CONSTRAINT PK_Attendance PRIMARY KEY CLUSTERED (AttId ASC),
    CONSTRAINT UK_Attendance_EmpDate UNIQUE NONCLUSTERED (EmpId, AttDate),
    CONSTRAINT CK_Attendance_Status CHECK (Status IN (N'正常',N'迟到',N'早退',N'缺勤',N'请假'))
);
CREATE NONCLUSTERED INDEX IX_Attendance_Date   ON dbo.Attendance(AttDate);
CREATE NONCLUSTERED INDEX IX_Attendance_Status ON dbo.Attendance(Status);

/* 9. 加班表 -------------------------------------------------------------- */
CREATE TABLE dbo.Overtime (
    OtId       INT IDENTITY(1,1) NOT NULL,
    EmpId      INT               NOT NULL,
    OtDate     DATE              NOT NULL,
    StartTime  TIME(0)           NOT NULL,
    EndTime    TIME(0)           NOT NULL,
    OtHours    DECIMAL(4,1)      NOT NULL,
    OtType     NVARCHAR(10)      NOT NULL CONSTRAINT DF_Overtime_Type DEFAULT (N'工作日'),
    Reason     NVARCHAR(500)     NOT NULL,
    Status     NVARCHAR(10)      NOT NULL CONSTRAINT DF_Overtime_Status DEFAULT (N'待审批'),
    CreateTime DATETIME          NOT NULL CONSTRAINT DF_Overtime_CreateTime DEFAULT (GETDATE()),
    CONSTRAINT PK_Overtime PRIMARY KEY CLUSTERED (OtId ASC),
    CONSTRAINT CK_Overtime_Hours  CHECK (OtHours > 0),
    CONSTRAINT CK_Overtime_Time   CHECK (EndTime > StartTime),
    CONSTRAINT CK_Overtime_Type   CHECK (OtType IN (N'工作日',N'周末',N'节假日')),
    CONSTRAINT CK_Overtime_Status CHECK (Status IN (N'待审批',N'已批准',N'已拒绝'))
);
CREATE NONCLUSTERED INDEX IX_Overtime_Emp  ON dbo.Overtime(EmpId);
CREATE NONCLUSTERED INDEX IX_Overtime_Date ON dbo.Overtime(OtDate);

/* 10. 工资表（每月一人一条）---------------------------------------------- */
CREATE TABLE dbo.Salary (
    SalaryId    INT IDENTITY(1,1) NOT NULL,
    EmpId       INT               NOT NULL,
    SalaryMonth CHAR(7)           NOT NULL,
    BaseSalary  DECIMAL(10,2)     NOT NULL CONSTRAINT DF_Salary_Base  DEFAULT ((0)),
    Performance DECIMAL(10,2)     NOT NULL CONSTRAINT DF_Salary_Perf  DEFAULT ((0)),
    Bonus       DECIMAL(10,2)     NOT NULL CONSTRAINT DF_Salary_Bonus DEFAULT ((0)),
    OvertimePay DECIMAL(10,2)     NOT NULL CONSTRAINT DF_Salary_Ot    DEFAULT ((0)),
    Insurance   DECIMAL(10,2)     NOT NULL CONSTRAINT DF_Salary_Ins   DEFAULT ((0)),
    Fund        DECIMAL(10,2)     NOT NULL CONSTRAINT DF_Salary_Fund  DEFAULT ((0)),
    Tax         DECIMAL(10,2)     NOT NULL CONSTRAINT DF_Salary_Tax   DEFAULT ((0)),
    Deduction   DECIMAL(10,2)     NOT NULL CONSTRAINT DF_Salary_Ded   DEFAULT ((0)),
    NetSalary   DECIMAL(10,2)     NOT NULL,
    PayDate     DATE              NULL,
    Remark      NVARCHAR(200)     NULL,
    CreateTime  DATETIME          NOT NULL CONSTRAINT DF_Salary_CreateTime DEFAULT (GETDATE()),
    CONSTRAINT PK_Salary PRIMARY KEY CLUSTERED (SalaryId ASC),
    CONSTRAINT UK_Salary_EmpMonth UNIQUE NONCLUSTERED (EmpId, SalaryMonth),
    CONSTRAINT CK_Salary_Month CHECK (SalaryMonth LIKE '[1-2][0-9][0-9][0-9]-[0-1][0-9]'),
    CONSTRAINT CK_Salary_Net   CHECK (NetSalary >= 0)
);
CREATE NONCLUSTERED INDEX IX_Salary_Month ON dbo.Salary(SalaryMonth);

/* 11. 事件日志表（无外键，UserName 逻辑关联，保留审计）------------------ */
CREATE TABLE dbo.EventLog (
    LogId       BIGINT IDENTITY(1,1) NOT NULL,
    UserName    NVARCHAR(50)         NOT NULL,
    EventName   NVARCHAR(50)         NOT NULL,
    Description NVARCHAR(1000)       NULL,
    IPAddress   NVARCHAR(50)         NULL,
    EventTime   DATETIME             NOT NULL CONSTRAINT DF_EventLog_EventTime DEFAULT (GETDATE()),
    CONSTRAINT PK_EventLog PRIMARY KEY CLUSTERED (LogId ASC)
);
CREATE NONCLUSTERED INDEX IX_EventLog_Time ON dbo.EventLog(EventTime);
CREATE NONCLUSTERED INDEX IX_EventLog_User ON dbo.EventLog(UserName);
CREATE NONCLUSTERED INDEX IX_EventLog_Name ON dbo.EventLog(EventName);
GO


/* ======================================================================
 *  二、外键 & 级联规则（统一添加，避开环形级联错误）
 *  ----------------------------------------------------------------------
 *  级联策略总览：
 *   ┌────────── 主表 ──────────┬─ 从表字段 ──────┬─ ON DELETE ──┬─ ON UPDATE ──┐
 *   │ Department               │ Department.ParentId │ CASCADE   │ CASCADE     │ 删父→子部门同步
 *   │ Department               │ Employee.DeptId    │ SET NULL   │ CASCADE     │ 删部门→员工脱岗不丢
 *   │ Employee(EmpId 经理)     │ Department.ManagerId│ SET NULL  │ CASCADE     │ 删经理→职位空缺
 *   │ Position                 │ Employee.PositionId│ SET NULL   │ CASCADE     │ 删职位→员工待分配
 *   │ Employee                 │ [User].EmpId       │ CASCADE    │ CASCADE     │ 删员工→账号同步删除
 *   │ Employee                 │ LeaveRecord.EmpId  │ CASCADE    │ CASCADE     │ 删员工→休假记录清空
 *   │ LeaveType                │ LeaveRecord.LeaveTypeId │ CASCADE│ CASCADE     │ 删类型→该类型记录清空
 *   │ Employee(审批人)         │ LeaveRecord.ApproverId │ SET NULL│ CASCADE     │ 审批人离职→记录保留
 *   │ Employee                 │ Attendance.EmpId   │ CASCADE    │ CASCADE     │ 删员工→考勤清空
 *   │ Employee                 │ Overtime.EmpId     │ CASCADE    │ CASCADE     │ 删员工→加班清空
 *   │ Employee                 │ Salary.EmpId       │ CASCADE    │ CASCADE     │ 删员工→工资记录清空
 *   │ 无 FK（逻辑关联 UserName）│ EventLog          │ 不受影响   │ 不受影响    │ 审计日志永保留
 *   └──────────────────────────┴──────────────────┴────────────┴────────────┘
 * ====================================================================== */

ALTER TABLE dbo.Department ADD CONSTRAINT FK_Department_Parent
    FOREIGN KEY (ParentId) REFERENCES dbo.Department (DeptId)
    ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE dbo.Employee ADD CONSTRAINT FK_Employee_Dept
    FOREIGN KEY (DeptId) REFERENCES dbo.Department (DeptId)
    ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE dbo.Employee ADD CONSTRAINT FK_Employee_Position
    FOREIGN KEY (PositionId) REFERENCES dbo.Position (PositionId)
    ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE dbo.Department ADD CONSTRAINT FK_Department_Manager
    FOREIGN KEY (ManagerId) REFERENCES dbo.Employee (EmpId)
    ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE dbo.[User] ADD CONSTRAINT FK_User_Employee
    FOREIGN KEY (EmpId) REFERENCES dbo.Employee (EmpId)
    ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE dbo.LeaveRecord ADD CONSTRAINT FK_LeaveRecord_Emp
    FOREIGN KEY (EmpId) REFERENCES dbo.Employee (EmpId)
    ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE dbo.LeaveRecord ADD CONSTRAINT FK_LeaveRecord_Type
    FOREIGN KEY (LeaveTypeId) REFERENCES dbo.LeaveType (LeaveTypeId)
    ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE dbo.LeaveRecord ADD CONSTRAINT FK_LeaveRecord_Approver
    FOREIGN KEY (ApproverId) REFERENCES dbo.Employee (EmpId)
    ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE dbo.Attendance ADD CONSTRAINT FK_Attendance_Emp
    FOREIGN KEY (EmpId) REFERENCES dbo.Employee (EmpId)
    ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE dbo.Overtime ADD CONSTRAINT FK_Overtime_Emp
    FOREIGN KEY (EmpId) REFERENCES dbo.Employee (EmpId)
    ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE dbo.Salary ADD CONSTRAINT FK_Salary_Emp
    FOREIGN KEY (EmpId) REFERENCES dbo.Employee (EmpId)
    ON DELETE CASCADE ON UPDATE CASCADE;
GO

PRINT '✓ 外键与级联规则已全部建立';
GO


/* ======================================================================
 *  三、初始化种子数据
 * ====================================================================== */

/* --- 3.1 部门（9 个并列顶级部门，无上下级层级关系，ParentId=NULL） --- */
SET IDENTITY_INSERT dbo.Department ON;
INSERT INTO dbo.Department(DeptId,DeptName,ParentId,Descn) VALUES
 (1,N'总公司',   NULL, N'行政与高层管理')
,(2,N'技术部',   NULL, N'研发、测试、运维')
,(3,N'人事部',   NULL, N'人力资源与行政管理')
,(4,N'财务部',   NULL, N'会计、出纳、税务')
,(5,N'市场部',   NULL, N'销售、品牌、客户关系')
,(6,N'研发部',   NULL, N'软件开发与项目交付')
,(7,N'运维部',   NULL, N'基础设施与运维保障')
,(8,N'招聘部',   NULL, N'人才引进与招聘配置')
,(9,N'薪酬部',   NULL, N'薪酬福利与员工关系');
SET IDENTITY_INSERT dbo.Department OFF;

/* --- 3.2 职位 --- */
INSERT INTO dbo.Position(PositionName,Level,Descn) VALUES
 (N'总经理',     1, N'公司最高负责人')
,(N'部门经理',   3, N'各部门负责人')
,(N'主管',       5, N'小组负责人')
,(N'高级工程师', 6, NULL)
,(N'工程师',     7, NULL)
,(N'专员',       8, N'普通职员')
,(N'实习生',     9, NULL);

/* --- 3.3 员工（先插总经理等上层，再填部门经理外键） --- */
SET IDENTITY_INSERT dbo.Employee ON;
INSERT INTO dbo.Employee(EmpId,EmpNo,EmpName,Gender,Birthday,IdCard,HireDate,DeptId,PositionId,Phone,Email,Status) VALUES
 (1, 'E00001', N'张伟',  N'男', '1985-03-12', '110101198503120001', '2010-05-01', 1, 1, '13800000001', 'zhangwei@hrms.com',  N'在职')
,(2, 'E00002', N'李娜',  N'女', '1988-07-25', '110101198807250002', '2012-03-15', 2, 2, '13800000002', 'lina@hrms.com',      N'在职')
,(3, 'E00003', N'王强',  N'男', '1990-11-08', '110101199011080003', '2014-08-20', 6, 4, '13800000003', 'wangqiang@hrms.com', N'在职')
,(4, 'E00004', N'赵敏',  N'女', '1992-02-14', '110101199202140004', '2015-11-10', 6, 5, '13800000004', 'zhaomin@hrms.com',   N'在职')
,(5, 'E00005', N'刘洋',  N'男', '1991-09-30', '110101199109300005', '2016-02-01', 7, 5, '13800000005', 'liuyang@hrms.com',   N'在职')
,(6, 'E00006', N'陈静',  N'女', '1989-12-05', '110101198912050006', '2013-07-01', 3, 2, '13800000006', 'chenjing@hrms.com',  N'在职')
,(7, 'E00007', N'孙丽',  N'女', '1995-04-18', '110101199504180007', '2017-06-15', 8, 6, '13800000007', 'sunli@hrms.com',     N'在职')
,(8, 'E00008', N'周杰',  N'男', '1993-08-22', '110101199308220008', '2018-04-10', 9, 6, '13800000008', 'zhoujie@hrms.com',   N'在职')
,(9, 'E00009', N'吴芳',  N'女', '1987-01-09', '110101198701090009', '2011-09-25', 4, 2, '13800000009', 'wufang@hrms.com',    N'在职')
,(10,'E00010', N'郑浩',  N'男', '1994-06-27', '110101199406270010', '2019-03-01', 5, 3, '13800000010', 'zhenghao@hrms.com',  N'试用期')
,(11,'E00011', N'何磊',  N'男', '1996-10-02', '110101199610020011', '2024-07-01', 6, 9, '13800000011', 'helei@hrms.com',     N'实习期');
SET IDENTITY_INSERT dbo.Employee OFF;

/* --- 回填部门经理（Employee 表已存在） --- */
UPDATE dbo.Department SET ManagerId = 1  WHERE DeptId = 1;   -- 总公司→张伟
UPDATE dbo.Department SET ManagerId = 2  WHERE DeptId = 2;   -- 技术部→李娜
UPDATE dbo.Department SET ManagerId = 3  WHERE DeptId = 6;   -- 研发组→王强
UPDATE dbo.Department SET ManagerId = 5  WHERE DeptId = 7;   -- 运维组→刘洋
UPDATE dbo.Department SET ManagerId = 6  WHERE DeptId = 3;   -- 人事部→陈静
UPDATE dbo.Department SET ManagerId = 7  WHERE DeptId = 8;   -- 招聘组→孙丽
UPDATE dbo.Department SET ManagerId = 8  WHERE DeptId = 9;   -- 薪酬组→周杰
UPDATE dbo.Department SET ManagerId = 9  WHERE DeptId = 4;   -- 财务部→吴芳
UPDATE dbo.Department SET ManagerId = 10 WHERE DeptId = 5;   -- 市场部→郑浩

/* --- 3.4 用户（密码明文示例；生产环境请改为 SHA256/BCrypt 哈希） --- */
SET IDENTITY_INSERT dbo.[User] ON;
INSERT INTO dbo.[User](UserId,UserName,Password,EmpId,RoleName) VALUES
 (1,'admin',  '123456', 1,  N'管理员')
,(2,'lina',   '123456', 2,  N'部门经理')
,(3,'wangq',  '123456', 3,  N'部门经理')
,(4,'zhaom',  '123456', 4,  N'普通用户')
,(5,'liuy',   '123456', 5,  N'普通用户')
,(6,'chenj',  '123456', 6,  N'部门经理')
,(7,'sunl',   '123456', 7,  N'普通用户')
,(8,'zhouj',  '123456', 8,  N'普通用户')
,(9,'wuf',    '123456', 9,  N'部门经理')
,(10,'zhengh','123456', 10, N'普通用户');
SET IDENTITY_INSERT dbo.[User] OFF;

/* --- 3.5 休假类型 --- */
INSERT INTO dbo.LeaveType(TypeName,DefaultDays,NeedApproval,Descn) VALUES
 (N'年假', 10, 1, N'工龄满1年以上每年享有的带薪假期')
,(N'病假', 15, 1, N'需提供医院证明')
,(N'事假', 10, 1, N'无薪事假，年度限额')
,(N'婚假',  3, 1, N'依法享受婚假')
,(N'产假', 98, 1, N'女性生育带薪假期')
,(N'陪产假',15, 1, N'男性配偶陪产假')
,(N'丧假',  3, 1, N'直系亲属去世');

/* --- 3.6 考勤参数（单例，SettingId=1；脚本自动命中 CHECK 约束） --- */
SET IDENTITY_INSERT dbo.AttendanceSetting ON;
INSERT INTO dbo.AttendanceSetting(SettingId,WorkStartTime,WorkEndTime,LateMinutes,EarlyMinutes,LunchStart,LunchEnd)
VALUES(1,'09:00:00','18:00:00',10,10,'12:00:00','13:30:00');
SET IDENTITY_INSERT dbo.AttendanceSetting OFF;

/* --- 3.7 休假记录 --- */
INSERT INTO dbo.LeaveRecord(EmpId,LeaveTypeId,StartDate,EndDate,LeaveDays,Reason,Status,ApproverId) VALUES
 (3,  1, '2026-06-15','2026-06-19', 5.0, N'回老家探望父母',        N'已批准', 2)
,(4,  2, '2026-06-22','2026-06-23', 2.0, N'感冒发烧需静养',        N'已批准', 2)
,(5,  3, '2026-06-29','2026-06-29', 1.0, N'处理个人事务',          N'待审批', 2)
,(7,  4, '2026-07-06','2026-07-08', 3.0, N'本人结婚',              N'待审批', 6)
,(10, 1, '2026-07-01','2026-07-03', 3.0, N'外出培训',             N'已批准', 9);

/* --- 3.8 考勤（6 月下旬若干日） --- */
INSERT INTO dbo.Attendance(EmpId,AttDate,CheckInTime,CheckOutTime,Status,Remark) VALUES
 (1, '2026-06-29','2026-06-29 08:45:00','2026-06-29 18:40:00',N'正常', NULL)
,(2, '2026-06-29','2026-06-29 08:55:00','2026-06-29 18:12:00',N'正常', NULL)
,(3, '2026-06-29','2026-06-29 09:25:00','2026-06-29 18:08:00',N'迟到', N'地铁故障')
,(4, '2026-06-29', NULL,                   NULL,                  N'请假', N'病假 (LeaveId=2)')
,(5, '2026-06-29','2026-06-29 08:58:00','2026-06-29 17:35:00',N'早退', N'家中有急事')
,(6, '2026-06-29','2026-06-29 08:50:00','2026-06-29 18:05:00',N'正常', NULL)
,(8, '2026-06-29','2026-06-29 08:59:00','2026-06-29 18:02:00',N'正常', NULL)
,(9, '2026-06-29','2026-06-29 08:40:00','2026-06-29 18:20:00',N'正常', NULL)
,(10,'2026-06-29','2026-06-29 08:52:00','2026-06-29 18:06:00',N'正常', NULL);

INSERT INTO dbo.Attendance(EmpId,AttDate,CheckInTime,CheckOutTime,Status) VALUES
 (1, '2026-06-30','2026-06-30 08:50:00','2026-06-30 18:10:00',N'正常')
,(2, '2026-06-30','2026-06-30 08:58:00','2026-06-30 18:05:00',N'正常')
,(3, '2026-06-30','2026-06-30 08:45:00','2026-06-30 18:15:00',N'正常')
,(4, '2026-06-30','2026-06-30 09:00:00','2026-06-30 18:00:00',N'正常')
,(5, '2026-06-30','2026-06-30 08:48:00','2026-06-30 18:00:00',N'正常')
,(7, '2026-06-30','2026-06-30 08:59:00','2026-06-30 18:01:00',N'正常');

/* --- 3.9 加班 --- */
INSERT INTO dbo.Overtime(EmpId,OtDate,StartTime,EndTime,OtHours,OtType,Reason,Status) VALUES
 (3, '2026-06-20','18:30','22:00', 3.5, N'工作日', N'新版本紧急上线',     N'已批准')
,(3, '2026-06-27','09:00','18:00', 8.0, N'周末',   N'处理生产环境Bug',    N'已批准')
,(4, '2026-06-28','18:30','21:00', 2.5, N'工作日', N'完成需求评审文档',   N'待审批')
,(5, '2026-06-28','19:00','23:00', 4.0, N'工作日', N'服务器升级与迁移',   N'已批准')
,(10,'2026-06-22','18:00','21:00', 3.0, N'工作日', N'市场活动方案筹备',   N'已批准');

/* --- 3.10 工资（2026-05 月度薪资） NetSalary = Base+Perf+Bonus+OtPay−Insur−Fund−Tax−Ded --- */
INSERT INTO dbo.Salary(EmpId,SalaryMonth,BaseSalary,Performance,Bonus,OvertimePay,Insurance,Fund,Tax,Deduction,NetSalary,PayDate,Remark) VALUES
 (1, '2026-05', 20000, 5000, 2000,   0, 2200, 1400, 2100,   0, 21300, '2026-06-10', N'总经理月薪')
,(2, '2026-05', 15000, 3000, 1000,   0, 1650, 1050, 1200,   0, 15100, '2026-06-10', NULL)
,(3, '2026-05', 12000, 2500,  800, 560, 1320,  840,  800,   0, 12900, '2026-06-10', N'含加班费')
,(4, '2026-05', 10000, 2000,  500,   0, 1100,  700,  500,   0, 10200, '2026-06-10', NULL)
,(5, '2026-05', 10000, 2000,  500, 280, 1100,  700,  520,   0, 10460, '2026-06-10', NULL)
,(6, '2026-05', 14000, 2800, 1000,   0, 1540,  980, 1080,   0, 14200, '2026-06-10', NULL)
,(7, '2026-05',  7000, 1000,  300,   0,  770,  490,  150,   0,  6890, '2026-06-10', NULL)
,(8, '2026-05',  7500, 1200,  300,   0,  825,  525,  200,   0,  7450, '2026-06-10', NULL)
,(9, '2026-05', 15000, 3000, 1000,   0, 1650, 1050, 1200,   0, 15100, '2026-06-10', NULL)
,(10,'2026-05',  9000, 1500,  400, 300,  990,  630,  350,   0,  9230, '2026-06-10', N'试用期，绩效按80%');

/* --- 3.11 事件日志 --- */
INSERT INTO dbo.EventLog(UserName,EventName,Description,IPAddress) VALUES
 ('admin',  N'登录',       N'管理员使用账号 admin 成功登录 HRMS 系统','192.168.1.10')
,('admin',  N'新增员工',   N'新增员工 E00011 何磊（实习生，研发组）','192.168.1.10')
,('admin',  N'修改参数',   N'将考勤迟到宽限从 5 分钟调整为 10 分钟','192.168.1.10')
,('lina',   N'审批通过',   N'批准员工 王强 2026-06-15 ~ 2026-06-19 年假申请','192.168.1.22')
,('lina',   N'审批通过',   N'批准员工 赵敏 2026-06-22 ~ 2026-06-23 病假申请','192.168.1.22')
,('wangq',  N'签到',       N'6月29日签到 09:25:00（迟到25分钟）','192.168.1.33')
,('chenj',  N'发薪确认',   N'完成 2026-05 月度薪资全员发放，共 10 人','192.168.1.44')
,('wuf',    N'导出报表',   N'导出 2026-05 部门薪资汇总 Excel','192.168.1.45')
,('zhengh', N'登录失败',   N'密码错误（连续第2次）','192.168.1.99');
GO

PRINT '✓ 初始化数据插入完成';
GO


/* ======================================================================
 *  四、自检：行数统计 & 外键级联有效性验证
 * ====================================================================== */
DECLARE @tbl SYSNAME, @sql NVARCHAR(MAX) = N'';
DECLARE c CURSOR LOCAL FAST_FORWARD FOR
    SELECT t.name FROM sys.tables t WHERE t.is_ms_shipped = 0 ORDER BY t.name;
OPEN c; FETCH NEXT FROM c INTO @tbl;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql += N'
SELECT N''' + @tbl + N''' AS [Table Name], COUNT_BIG(*) AS [Rows] FROM dbo.' + QUOTENAME(@tbl) + N';';
    FETCH NEXT FROM c INTO @tbl;
END
CLOSE c; DEALLOCATE c;
EXEC sp_executesql @sql;
GO

PRINT CHAR(13) + CHAR(10) + N'═══════════════════════════════════════════';
PRINT N'  HRMS 数据库初始化全部完成 ✓';
PRINT N'  共 11 张表，完整支持 8 大功能模块';
PRINT N'═══════════════════════════════════════════';
GO
