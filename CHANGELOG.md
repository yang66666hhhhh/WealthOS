# CHANGELOG

## [1.0.0] - 2026-05-31

### 新增

#### 核心架构
- Clean Architecture 四层架构 (Domain, Application, Infrastructure, Presentation)
- SQLite 数据库自动初始化
- Dapper ORM 数据访问
- Microsoft.Extensions.DependencyInjection 依赖注入

#### Dashboard
- 净资产显示（总额、本月变化）
- 现金/投资/负债分类统计
- 储蓄率计算
- 资产配置饼图 (LiveCharts2)
- 净资产趋势曲线
- 最近 10 笔交易列表

#### 资产中心
- 卡片式资产展示
- 按类型筛选（现金、银行、投资、房产等）
- 新增资产对话框
- 删除资产
- 资产变化百分比显示

#### 负债中心
- 卡片式负债展示
- 负债总额统计
- 利率、月供信息展示
- 新增负债对话框
- 删除负债

#### 收支中心
- 交易列表展示
- 按类型筛选（收入、支出、转账）
- 颜色编码（绿色收入、红色支出、蓝色转账）
- 新增交易对话框
- 删除交易
- 自动更新账户余额

#### 目标中心
- 目标卡片展示
- 进度条可视化
- 预计达成时间计算
- 新增目标对话框
- 删除目标

#### UI/UX
- 奶油白背景 + 深色侧边栏主题
- 超大圆角卡片设计
- 响应式布局
- 加载状态指示器
- 货币格式化（万/亿单位）

#### 数据模型
- Account (账户)
- Asset (资产)
- Liability (负债)
- Transaction (交易)
- Goal (目标)
- Category (分类)
- InvestmentHolding (投资持仓)
- NetWorthSnapshot (净资产快照)

#### 开发基础设施
- 完整的项目文档
- 架构文档
- 开发指南
- AI Agent 约束文档 (AGENTS.md)
- 产品需求文档 (PRD.md)

---

## [未发布]

### 计划

#### Phase 2
- 投资管理模块
- 固定资产管理
- 图表分析增强
- 时间轴功能

#### Phase 3
- 年度报告生成
- 多币种支持
- 数据导入/导出

#### Phase 4
- 云同步
- 多设备支持
- AI 财务分析
