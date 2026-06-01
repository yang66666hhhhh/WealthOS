# CHANGELOG

## [1.1.0] - 2026-06-01

### 修复

#### 数据完整性
- `Account.Type` 枚举从 `AssetType` 改为正确的 `AccountType`
- `UpdateTransactionAsync` 编辑转账时正确更新目标账户余额
- `TransactionDto` 新增 `AccountId`/`CategoryId`，编辑对话框改用 ID 匹配替代名称匹配
- `ExportCsvAsync` 日期范围改为 `DateTimeKind.Utc`，与数据库 UTC 存储一致

#### 资源加载
- 修复 `CommonStyles.xaml` 作为 MergedDictionary 导致 `StaticResource` 无法解析父字典资源的 WPF 已知问题
- `CommonStyles.xaml` 从 `Theme.xaml` 移至 `App.xaml` 独立加载
- `ApplyTheme()` 搜索逻辑改用 `EndsWith` 精确匹配

#### 空状态显示
- 修复 10 个视图空状态显示逻辑完全反转的 bug（有数据时显示空状态，无数据时隐藏）

#### 错误处理
- 6 个 ViewModel 的写操作（Add/Edit/Delete）添加 `try/catch`
- `ExportCsvAsync` 添加错误处理
- `BackupAsync` 添加 `File.Exists` 检查
- `RestoreAsync` 添加 SQLite `integrity_check` 验证，防止损坏备份覆盖有效数据库

### 改进

#### 架构
- Application 层移除 Dapper/Sqlite 包引用（移至 Infrastructure）
- 删除未使用的 `Frequency` 枚举、`TransactionTypeToColorConverter`、`TransactionTypeToSignConverter`
- `ViewModelBase` 新增 `RefreshCommand` 抽象属性，消除 `MainWindowViewModel` 的 if/else 刷新链
- `ViewModelBase` 新增 `GetResourceColor`/`GetResourceString` 共享方法
- `NavigationService` 参数类型从 `ObservableObject` 改为 `ViewModelBase`
- `SeedDataService` 自包含类别种子调用
- `SafeInitializeAsync` 异常后调用 `SetError` 显示给用户

#### UI/UX
- 交易列表从 `ItemsControl` 改为 `ListView` + 虚拟化回收模式
- 22 个对话框添加 Enter 确认 / Escape 取消键盘快捷键
- 12 个视图添加 `ErrorBanner` 错误提示组件
- GoalsView/LiabilitiesView 添加 Loading 进度条
- 统一 ScrollViewer 内边距（28,24）和页面标题样式（`PageTitleStyle`）
- ReportsView 年份按钮改为动态绑定 `AvailableYears`
- ReportsView/AnalyticsView 添加空状态
- SettingsView 添加 `IsBusy` Loading 指示器
- Dashboard 所有区块添加 "查看全部" 导航链接
- Dashboard "添加交易" 快捷操作自动打开新增对话框
- 16 个图标按钮添加 ToolTip
- 侧边栏修复重复图标（Budgets 💰→💳，Analytics 📊→📉）
- `StartupErrorWindow` 改为亮色主题颜色

#### 国际化
- 新增 30+ 本地化资源键（验证消息、空状态、工具提示、文件对话框等）
- 验证消息从硬编码中文改为资源键引用
- BudgetsView "年"/"月" 改为资源绑定
- FixedAssetsView "个月" 改为 MultiBinding
- `CurrentDate` 日期格式改为通用 `yyyy-MM-dd dddd`
- `App.xaml.cs` 启动错误消息改为资源键引用

#### 主题
- `DangerButton` 悬停颜色从硬编码 `#DC2626` 改为 `ErrorDarkBrush` 主题资源
- `Theme.xaml`/`Theme.Dark.xaml` 新增 `ErrorDarkColor` + `ErrorDarkBrush`

#### 性能
- `AnalyticsViewModel` 从 6 次 DB 查询优化为 1 次批量查询 + 内存分组
- 数据库新增 3 个索引：`CategoryId`、`ToAccountId`、`InvestmentHoldings(AccountId)`

---

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
