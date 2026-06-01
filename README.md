# WealthOS

**Personal Wealth Operating System** - 管理你拥有的一切资产、负债、现金流和人生目标。

## 定位

不是传统记账软件。WealthOS 是一个**个人财富操作系统**，目标是：

- 管理全部资产（现金、银行、投资、固定资产）
- 管理负债（信用卡、房贷、车贷、个人借款）
- 追踪收支与现金流
- 设定并追踪财富目标（买房、买车、退休）
- 提供资产趋势、净资产变化等分析
- 生成年度财富报告

## 技术栈

| 层 | 技术 |
|---|------|
| UI | WPF (.NET 10) |
| MVVM | CommunityToolkit.Mvvm 8.4 |
| 图表 | LiveCharts2 (SkiaSharp) |
| 数据库 | SQLite |
| ORM | Dapper |
| DI | Microsoft.Extensions.DependencyInjection |
| 架构 | Clean Architecture |

## 快速开始

### 环境要求

- .NET 10 SDK
- Windows 10/11

### 运行

```bash
dotnet run --project src/WealthOS.Presentation
```

数据库自动创建在 `%LOCALAPPDATA%/WealthOS/wealthos.db`。

### 构建

```bash
dotnet build
```

## 项目结构

```
WealthOS/
├── src/
│   ├── WealthOS.Domain/           # 核心模型与枚举
│   ├── WealthOS.Application/      # 业务逻辑、接口、DTO
│   ├── WealthOS.Infrastructure/   # SQLite + Dapper 实现
│   └── WealthOS.Presentation/     # WPF 界面
└── docs/                          # 文档
```

## 功能模块

### MVP (Phase 1)

- [x] Dashboard - 净资产、资产配置、净值曲线、最近交易、快捷操作
- [x] 资产中心 - 卡片式布局、按类型筛选、增删改查
- [x] 负债中心 - 卡片展示、总额统计、增删改查
- [x] 收支中心 - 列表展示、按类型筛选、增删改查、虚拟化滚动
- [x] 目标中心 - 进度条、预计达成时间、增删改查、进度更新
- [x] 账户中心 - 账户管理、余额统计
- [x] 预算中心 - 月度预算、执行率追踪

### Phase 2

- [x] 投资模块 - 股票/基金/ETF/黄金持仓管理、收益率计算
- [x] 固定资产 - MacBook、相机、车辆折旧追踪
- [x] 图表分析 - 收支趋势柱状图、资产分布饼图
- [x] 时间轴 - GitHub Activity 风格的财富变动记录
- [x] 年度报告 - 收支构成、储蓄率、净资产增长、CSV 导出

### Phase 3

- [ ] 多币种支持
- [ ] 数据导入/导出
- [ ] 自动生成 PDF 报告

### Phase 4

- [ ] 云同步
- [ ] 多设备
- [ ] AI 财务分析

## 设计风格

- 奶油白背景 + 深色侧边栏
- 超大圆角 (16~20px)
- 卡片化布局
- 数据可视化优先
- 参考：Notion、Linear、YNAB、Apple Wallet

## 许可证

Private - Personal Use Only
