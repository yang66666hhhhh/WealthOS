# 架构文档

## 总体架构

WealthOS 采用 **Clean Architecture**（整洁架构），将关注点分离到四个独立的层中。

```
┌─────────────────────────────────────────────┐
│              Presentation                    │
│         (WPF, ViewModels, Views)            │
├─────────────────────────────────────────────┤
│              Application                     │
│      (Services, Interfaces, DTOs)           │
├──────────────────────┬──────────────────────┤
│      Domain          │   Infrastructure     │
│  (Entities, Enums)   │  (Repositories, DB)  │
└──────────────────────┴──────────────────────┘
```

### 依赖规则

```
Presentation → Application ← Infrastructure
                    ↓
                 Domain
```

- 外层依赖内层
- 内层不知道外层的存在
- Domain 层是最内层，零外部依赖

---

## 各层职责

### Domain 层

**职责**: 定义核心业务模型和业务规则

```
WealthOS.Domain/
├── Entities/          # 业务实体
│   ├── Account.cs     # 账户（现金、银行、投资等）
│   ├── Asset.cs       # 资产（固定资产、投资资产等）
│   ├── Liability.cs   # 负债（信用卡、房贷等）
│   ├── Transaction.cs # 交易记录
│   ├── Goal.cs        # 财富目标
│   ├── Category.cs    # 交易分类
│   ├── InvestmentHolding.cs  # 投资持仓
│   └── NetWorthSnapshot.cs   # 净资产快照
├── Enums/             # 枚举定义
│   ├── AssetType.cs   # 资产类型
│   ├── TransactionType.cs    # 交易类型
│   ├── LiabilityType.cs      # 负债类型
│   ├── GoalStatus.cs         # 目标状态
│   └── Frequency.cs          # 频率
└── Common/
    └── BaseEntity.cs  # 实体基类
```

**约束**:
- 不引用任何外部包
- 不包含数据访问逻辑
- 不包含 UI 逻辑
- 实体是 POCO（Plain Old CLR Object）

### Application 层

**职责**: 定义接口、实现业务逻辑、定义数据传输对象

```
WealthOS.Application/
├── Interfaces/        # 接口定义
│   ├── IRepository.cs           # 通用仓储接口
│   ├── IAccountRepository.cs    # 账户仓储接口
│   ├── IAssetRepository.cs      # 资产仓储接口
│   ├── ILiabilityRepository.cs  # 负债仓储接口
│   ├── ITransactionRepository.cs # 交易仓储接口
│   ├── IGoalRepository.cs       # 目标仓储接口
│   ├── ICategoryRepository.cs   # 分类仓储接口
│   ├── INetWorthRepository.cs   # 净资产仓储接口
│   └── IDbContext.cs            # 数据库上下文接口
├── Services/          # 业务服务
│   ├── DashboardService.cs      # 仪表盘服务
│   ├── AssetService.cs          # 资产服务
│   ├── LiabilityService.cs      # 负债服务
│   ├── TransactionService.cs    # 交易服务
│   └── GoalService.cs           # 目标服务
└── DTOs/              # 数据传输对象
    ├── DashboardDto.cs          # 仪表盘数据
    ├── AssetCardDto.cs          # 资产卡片数据
    ├── LiabilityDto.cs          # 负债数据
    ├── TransactionDto.cs        # 交易数据
    └── GoalDto.cs               # 目标数据
```

**约束**:
- 只依赖 Domain 层
- 接口定义在此层（依赖倒置）
- Service 包含业务逻辑
- DTO 是不可变的 `record` 类型

### Infrastructure 层

**职责**: 实现数据访问、外部服务集成

```
WealthOS.Infrastructure/
├── Data/              # 数据库配置
│   ├── SqliteDbContext.cs       # SQLite 连接管理
│   └── DatabaseInitializer.cs   # 数据库初始化（建表）
└── Repositories/      # 仓储实现
    ├── BaseRepository.cs        # 通用仓储基类
    ├── AccountRepository.cs     # 账户仓储
    ├── AssetRepository.cs       # 资产仓储
    ├── LiabilityRepository.cs   # 负债仓储
    ├── TransactionRepository.cs # 交易仓储
    ├── GoalRepository.cs        # 目标仓储
    ├── CategoryRepository.cs    # 分类仓储
    └── NetWorthRepository.cs    # 净资产仓储
```

**约束**:
- 依赖 Application 层（实现接口）
- 使用 Dapper 进行数据访问
- 使用 SQLite 作为数据库
- Repository 实现 Application 层定义的接口

### Presentation 层

**职责**: 用户界面、用户交互

```
WealthOS.Presentation/
├── ViewModels/        # 视图模型
│   ├── MainWindowViewModel.cs   # 主窗口 ViewModel
│   ├── DashboardViewModel.cs    # 仪表盘 ViewModel
│   ├── AssetsViewModel.cs       # 资产 ViewModel
│   ├── LiabilitiesViewModel.cs  # 负债 ViewModel
│   ├── TransactionsViewModel.cs # 交易 ViewModel
│   └── GoalsViewModel.cs        # 目标 ViewModel
├── Views/             # 视图
│   ├── DashboardView.xaml       # 仪表盘视图
│   ├── AssetsView.xaml          # 资产视图
│   ├── LiabilitiesView.xaml     # 负债视图
│   ├── TransactionsView.xaml    # 交易视图
│   └── GoalsView.xaml           # 目标视图
├── Converters/        # 值转换器
│   ├── CurrencyConverter.cs     # 货币格式化
│   ├── BoolToVisibilityConverter.cs
│   ├── TransactionSignMultiConverter.cs
│   └── ...
├── Services/          # UI 服务
│   └── NavigationService.cs     # 导航服务
├── Resources/         # 资源
│   └── Theme.xaml               # 主题样式
├── App.xaml           # 应用入口
├── App.xaml.cs        # DI 配置
├── MainWindow.xaml    # 主窗口
└── MainWindow.xaml.cs # 主窗口 code-behind
```

**约束**:
- 依赖 Application 和 Infrastructure 层
- 使用 CommunityToolkit.Mvvm
- ViewModel 通过 DI 注入
- View 通过 DataTemplate 自动解析

---

## 核心模型关系

```
Account (账户)
├── Cash (现金)
├── Bank (银行)
├── Investment (投资)
└── ...

Asset (资产)
├── Linked to Account (可选)
├── Has Type (AssetType)
└── Has Value (CurrentValue, InitialValue)

Liability (负债)
├── Has Type (LiabilityType)
├── Has Balance
└── Has InterestRate

Transaction (交易)
├── Belongs to Account
├── Has Category (可选)
├── Has Type (Income/Expense/Transfer)
└── Has Amount

Goal (目标)
├── Has TargetAmount
├── Has CurrentAmount
└── Has TargetDate

NetWorthSnapshot (净资产快照)
├── TotalAssets
├── TotalLiabilities
└── NetWorth = TotalAssets - TotalLiabilities
```

---

## 数据流

### 读取数据

```
View → ViewModel → Service → Repository → SQLite
  ↑                                          ↓
  └──────────── DTO ← Entity ←──────────────┘
```

1. View 通过数据绑定请求数据
2. ViewModel 调用 Service
3. Service 调用 Repository
4. Repository 查询 SQLite，返回 Entity
5. Service 将 Entity 转换为 DTO
6. ViewModel 更新属性，View 自动更新

### 写入数据

```
View → ViewModel → Service → Repository → SQLite
```

1. View 通过命令触发操作
2. ViewModel 收集输入，创建 Entity
3. Service 执行业务逻辑
4. Repository 写入 SQLite

---

## DI 注册顺序

```csharp
// 1. 数据库
services.AddSingleton<IDbContext>(new SqliteDbContext(connectionString));
services.AddSingleton<DatabaseInitializer>();

// 2. Repository
services.AddSingleton<IAccountRepository, AccountRepository>();
services.AddSingleton<IAssetRepository, AssetRepository>();
// ...

// 3. Service
services.AddSingleton<DashboardService>();
services.AddSingleton<AssetService>();
// ...

// 4. UI 服务
services.AddSingleton<NavigationService>();

// 5. ViewModel
services.AddTransient<DashboardViewModel>();
services.AddTransient<AssetsViewModel>();
// ...

// 6. View
services.AddTransient<MainWindow>();
```

**注意**:
- Repository 和 Service 注册为 `Singleton`
- ViewModel 注册为 `Transient`（每次导航创建新实例）
- `DatabaseInitializer` 在 `App.OnStartup` 中调用

---

## 数据库 Schema

### Accounts 表

```sql
CREATE TABLE Accounts (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Type INTEGER NOT NULL,
    Balance REAL NOT NULL DEFAULT 0,
    Currency TEXT NOT NULL DEFAULT 'CNY',
    Institution TEXT NOT NULL DEFAULT '',
    Note TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);
```

### Assets 表

```sql
CREATE TABLE Assets (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Type INTEGER NOT NULL,
    CurrentValue REAL NOT NULL DEFAULT 0,
    InitialValue REAL NOT NULL DEFAULT 0,
    Currency TEXT NOT NULL DEFAULT 'CNY',
    Institution TEXT NOT NULL DEFAULT '',
    Note TEXT,
    DepreciationRate REAL,
    PurchaseDate TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);
```

### Liabilities 表

```sql
CREATE TABLE Liabilities (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Type INTEGER NOT NULL,
    Balance REAL NOT NULL DEFAULT 0,
    InterestRate REAL NOT NULL DEFAULT 0,
    MonthlyPayment REAL NOT NULL DEFAULT 0,
    StartDate TEXT NOT NULL,
    EndDate TEXT,
    Currency TEXT NOT NULL DEFAULT 'CNY',
    Institution TEXT,
    Note TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);
```

### Transactions 表

```sql
CREATE TABLE Transactions (
    Id TEXT PRIMARY KEY,
    Type INTEGER NOT NULL,
    Amount REAL NOT NULL,
    CategoryId TEXT,
    AccountId TEXT NOT NULL,
    ToAccountId TEXT,
    Note TEXT,
    OccurredAt TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    FOREIGN KEY (AccountId) REFERENCES Accounts(Id),
    FOREIGN KEY (ToAccountId) REFERENCES Accounts(Id)
);
```

### Goals 表

```sql
CREATE TABLE Goals (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    TargetAmount REAL NOT NULL,
    CurrentAmount REAL NOT NULL DEFAULT 0,
    TargetDate TEXT NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0,
    Icon TEXT,
    Note TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);
```

### 索引

```sql
CREATE INDEX idx_transactions_occurred ON Transactions(OccurredAt);
CREATE INDEX idx_transactions_account ON Transactions(AccountId);
CREATE INDEX idx_transactions_type ON Transactions(Type);
CREATE INDEX idx_networth_date ON NetWorthSnapshots(SnapshotDate);
```

---

## 扩展点

### 新增资产类型

1. 在 `AssetType` 枚举添加值
2. 在 `DashboardService.GetAssetColor()` 添加颜色
3. 更新 UI 筛选器

### 新增页面

1. 创建 ViewModel (继承 `ObservableObject`)
2. 创建 View (UserControl)
3. 在 `MainWindow.xaml` 添加 DataTemplate
4. 在 `MainWindowViewModel` 添加导航
5. 在 `App.xaml.cs` 注册 DI

### 新增数据库表

1. 在 `DatabaseInitializer.cs` 添加建表 SQL
2. 创建 Entity 类
3. 创建 Repository 接口和实现
4. 创建 Service
5. 注册 DI
