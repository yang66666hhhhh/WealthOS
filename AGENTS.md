# AGENTS.md - AI Agent 开发约束

本文档约束 AI Agent（Codex、Claude Code、Cursor、Copilot 等）在 WealthOS 项目中的行为。

---

## 项目认知

WealthOS 是**个人财富操作系统**，不是传统记账软件。

核心区别：
- 管理的是**资产、负债、净资产、人生目标**
- 不是收入-支出-分类-报表这种流水账
- 产品定位类似 Notion + MoneyWiz + 资产管理 + 人生目标

---

## 架构约束

### 必须遵循 Clean Architecture

```
Domain → Application → Infrastructure → Presentation
```

- **Domain**: 纯实体和枚举，零依赖
- **Application**: 接口定义、Service、DTO，只依赖 Domain
- **Infrastructure**: Repository 实现，依赖 Application
- **Presentation**: WPF + ViewModel，依赖 Application 和 Infrastructure

### 依赖方向

```
Presentation → Application ← Infrastructure
                  ↓
               Domain
```

- Domain 层**禁止**引用任何其他层
- Application 层**禁止**引用 Infrastructure 和 Presentation
- Infrastructure 层**禁止**引用 Presentation

### 数据库

- 只用 SQLite
- 只用 Dapper（禁止 Entity Framework）
- 连接字符串通过 `IDbContext` 注入
- 禁止硬编码连接字符串

### MVVM

- 只用 CommunityToolkit.Mvvm
- ViewModel 继承 `ObservableObject`
- 命令用 `[RelayCommand]` Source Generator
- 属性用 `[ObservableProperty]` Source Generator
- 禁止在 ViewModel 中直接引用 View

---

## 编码规范

### C# 风格

- 文件范围命名空间：`namespace X;`
- 使用 `record` 类型做 DTO
- 使用 `init` 属性做不可变 DTO
- 使用 `nullable reference types`（项目已启用 `<Nullable>enable</Nullable>`）
- 使用 `ImplicitUsings`（项目已启用）
- 禁止使用 `var` 推断复杂类型（LINQ 链、泛型等必须显式类型）
- 简单类型可以用 `var`：`var name = "test";`

### 命名

| 类型 | 规则 | 示例 |
|------|------|------|
| 类/记录 | PascalCase | `DashboardService` |
| 接口 | I + PascalCase | `IAssetRepository` |
| 公共属性 | PascalCase | `CurrentValue` |
| 私有字段 | _camelCase | `_repo` |
| 方法 | PascalCase | `GetAllAssetsAsync` |
| 参数 | camelCase | `assetType` |
| 局部变量 | camelCase | `totalAssets` |
| 枚举值 | PascalCase | `AssetType.Cash` |

### 异步

- 所有 I/O 操作必须异步
- 方法名以 `Async` 结尾
- 返回 `Task` 或 `Task<T>`
- 禁止 `.Result` 或 `.Wait()`

### 命名空间

```
WealthOS.Domain.Entities
WealthOS.Domain.Enums
WealthOS.Domain.Common
WealthOS.Application.Interfaces
WealthOS.Application.Services
WealthOS.Application.DTOs
WealthOS.Infrastructure.Data
WealthOS.Infrastructure.Repositories
WealthOS.Presentation.ViewModels
WealthOS.Presentation.Views
WealthOS.Presentation.Converters
WealthOS.Presentation.Services
WealthOS.Presentation.Resources
```

---

## XAML 约束

### 资源

- 主题定义在 `Resources/Theme.xaml`
- 颜色用 `SolidColorBrush` 资源
- 样式用 `Style` 资源
- 转换器定义在 `Converters/` 目录

### 布局

- 卡片用 `Border` + `Style="{StaticResource CardStyle}"`
- 圆角统一用 `CornerRadius` 资源值
- 间距统一用 `Margin` 和 `Padding`
- 禁止硬编码颜色值（必须引用资源）

### 数据绑定

- 使用 `{Binding PropertyName}`
- 转换器用 `{StaticResource ConverterKey}`
- 禁止在 XAML 中写 C# 代码

### .NET 10 WPF 兼容性注意事项

1. **XAML 命名空间声明**：禁止使用 `assembly=mscorlib`，应使用 `clr-namespace:System`（不指定程序集）
   ```xml
   <!-- 错误 -->
   xmlns:sys="clr-namespace:System;assembly=mscorlib"
   <!-- 正确 -->
   xmlns:sys="clr-namespace:System"
   ```

2. **资源字典 URI**：必须使用完整的 pack URI 格式
   ```xml
   <!-- 错误 -->
   <ResourceDictionary Source="Resources/Theme.xaml"/>
   <!-- 正确 -->
   <ResourceDictionary Source="pack://application:,,,/WealthOS.Presentation;component/Resources/Theme.xaml"/>
   ```

3. **Binding.Source 禁止使用 DynamicResource**：`Binding.Source` 不是依赖属性，只能用 `StaticResource`
   ```xml
   <!-- 错误 -->
   <Binding Source="{DynamicResource Nav.Dashboard}"/>
   <!-- 正确 -->
   <Binding Source="{StaticResource Nav.Dashboard}"/>
   ```

4. **Run.Text 绑定必须指定 Mode=OneWay**：`Run.Text` 默认双向绑定，但 `Binding.Source` 无 Path
   ```xml
   <!-- 错误 -->
   <Run><Run.Text><Binding Source="{StaticResource Key}"/></Run.Text></Run>
   <!-- 正确 -->
   <Run><Run.Text><Binding Source="{StaticResource Key}" Mode="OneWay"/></Run.Text></Run>
   ```

5. **RelayCommand 参数类型**：XAML `CommandParameter` 传递字符串，命令方法参数必须用 `object?` 并内部转换
   ```csharp
   // 错误
   [RelayCommand]
   private async Task FilterByTypeAsync(TransactionType? type) { ... }
   
   // 正确
   [RelayCommand]
   private async Task FilterByTypeAsync(object? parameter)
   {
       TransactionType? type = parameter switch
       {
           TransactionType t => t,
           string s when Enum.TryParse<TransactionType>(s, out var parsed) => parsed,
           _ => null
       };
       // ...
   }
   ```

6. **BoolToVisibilityConverter 必须处理 int 类型**：用于 `Collection.Count` 绑定时需要支持 int
   ```csharp
   // 必须支持 bool 和 int
   return value switch
   {
       true => Visibility.Visible,
       false => Visibility.Collapsed,
       int i => i != 0 ? Visibility.Visible : Visibility.Collapsed,
       _ => Visibility.Collapsed
   };
   ```

---

## 禁止事项

### 绝对禁止

1. 引入 Entity Framework
2. 引入 AutoMapper
3. 引入 MediatR
4. 引入任何第三方 MVVM 框架（Prism、ReactiveUI 等）
5. 使用 `async void`（除事件处理器外）
6. 在 ViewModel 中直接操作数据库
7. 在 View 中写业务逻辑
8. 使用 `MessageBox.Show()` 做错误提示（应该用 MVVM 方式）
9. 硬编码连接字符串、API Key、密码等敏感信息
10. 提交包含 `TODO`、`HACK`、`FIXME` 注释的代码

### 不推荐

1. 过度抽象（不需要的地方不要抽接口）
2. 过度使用依赖注入（简单工具类可以直接 new）
3. 创建不必要的基类
4. 使用反射替代强类型

---

## 文件组织

### 新增实体

1. 在 `Domain/Entities/` 添加实体类，继承 `BaseEntity`
2. 在 `Domain/Enums/` 添加相关枚举
3. 在 `Application/Interfaces/` 添加 Repository 接口
4. 在 `Application/Services/` 添加 Service
5. 在 `Application/DTOs/` 添加 DTO
6. 在 `Infrastructure/Repositories/` 添加 Repository 实现
7. 在 `Infrastructure/Data/DatabaseInitializer.cs` 添加建表 SQL
8. 在 `Presentation/ViewModels/` 添加 ViewModel
9. 在 `Presentation/Views/` 添加 View
10. 在 `App.xaml.cs` 注册 DI
11. 在 `MainWindowViewModel` 添加导航

### 新增页面

1. 在 `ViewModels/` 创建 ViewModel
2. 在 `Views/` 创建 View (XAML + code-behind)
3. 在 `MainWindow.xaml` 添加 DataTemplate 映射
4. 在 `MainWindowViewModel` 添加导航命令
5. 在 `MainWindow.xaml` 添加侧边栏导航按钮

---

## Git 约束

### 提交信息格式

```
<type>(<scope>): <description>

[optional body]
```

**提交信息必须使用中文描述**，type 和 scope 保持英文。

类型：
- `feat`: 新功能
- `fix`: 修复
- `refactor`: 重构
- `style`: 样式调整
- `docs`: 文档
- `chore`: 构建/工具

示例：
```
feat(assets): 添加资产卡片 CRUD
fix(dashboard): 修正净资产计算逻辑
refactor(infrastructure): 提取通用仓储基类
feat(i18n): 添加中英文语言切换
```

### 禁止提交

- `bin/`、`obj/` 目录
- `.vs/` 目录
- `*.user` 文件
- 数据库文件 `*.db`
- 任何包含敏感信息的文件

---

## 测试约束

- 测试项目放在 `tests/` 目录
- 测试框架：xUnit
- Mock 框架：Moq
- 测试命名：`Method_Scenario_ExpectedResult`
- 每个 Service 方法至少一个测试

---

## 依赖管理

### 已批准的包

| 包 | 用途 |
|----|------|
| CommunityToolkit.Mvvm | MVVM |
| LiveChartsCore.SkiaSharpView.WPF | 图表 |
| Dapper | ORM |
| Microsoft.Data.Sqlite | SQLite |
| Microsoft.Extensions.DependencyInjection | DI |

### 新增包的规则

1. 必须有明确的使用场景
2. 必须是活跃维护的项目
3. 必须与 .NET 10 兼容
4. 优先使用已有包的功能
5. 禁止引入有安全漏洞的包

---

## 性能约束

- 列表数据超过 100 条必须分页或虚拟化
- 数据库查询必须有索引
- 图表数据点不超过 1000 个
- 异步操作必须有取消支持（长时间操作）
- 内存中缓存频繁访问的数据

---

## UI/UX 约束

- 所有金额显示使用 `￥` 符号
- 大金额使用万/亿单位
- 百分比保留 1 位小数
- 日期使用 `yyyy-MM-dd` 格式
- 加载状态必须有 Loading 指示器
- 空状态必须有友好提示
- 操作确认使用对话框，不用 MessageBox
