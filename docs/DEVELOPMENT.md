# 开发指南

## 环境搭建

### 前置条件

1. 安装 .NET 10 SDK
2. 安装 Visual Studio 2022 或 JetBrains Rider
3. Windows 10/11

### 克隆项目

```bash
git clone <repository-url>
cd WealthOS
```

### 还原依赖

```bash
dotnet restore
```

### 运行

```bash
dotnet run --project src/WealthOS.Presentation
```

---

## 开发流程

### 1. 创建功能分支

```bash
git checkout -b feature/your-feature-name
```

### 2. 开发

遵循 AGENTS.md 中的编码规范。

### 3. 构建验证

```bash
dotnet build
```

确保 0 错误。

### 4. 提交

```bash
git add .
git commit -m "feat(scope): description"
```

---

## 常见任务

### 新增一个实体

#### 步骤 1: 创建实体类

在 `src/WealthOS.Domain/Entities/` 创建新文件：

```csharp
using WealthOS.Domain.Common;

namespace WealthOS.Domain.Entities;

public class YourEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    // 其他属性...
}
```

#### 步骤 2: 创建 Repository 接口

在 `src/WealthOS.Application/Interfaces/` 创建：

```csharp
namespace WealthOS.Application.Interfaces;

public interface IYourEntityRepository : IRepository<YourEntity>
{
    // 自定义查询方法...
}
```

#### 步骤 3: 创建 Repository 实现

在 `src/WealthOS.Infrastructure/Repositories/` 创建：

```csharp
using Dapper;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;

namespace WealthOS.Infrastructure.Repositories;

public class YourEntityRepository : BaseRepository<YourEntity>, IYourEntityRepository
{
    protected override string TableName => "YourEntities";

    public YourEntityRepository(IDbContext context) : base(context) { }

    // 实现自定义查询方法...
}
```

#### 步骤 4: 添加建表 SQL

在 `src/WealthOS.Infrastructure/Data/DatabaseInitializer.cs` 的 `InitializeAsync` 方法中添加：

```sql
CREATE TABLE IF NOT EXISTS YourEntities (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Value REAL NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);
```

#### 步骤 5: 创建 Service

在 `src/WealthOS.Application/Services/` 创建：

```csharp
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;

namespace WealthOS.Application.Services;

public class YourEntityService
{
    private readonly IYourEntityRepository _repo;

    public YourEntityService(IYourEntityRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<YourEntity>> GetAllAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<Guid> AddAsync(YourEntity entity)
    {
        return await _repo.AddAsync(entity);
    }

    // 其他方法...
}
```

#### 步骤 6: 创建 DTO (可选)

在 `src/WealthOS.Application/DTOs/` 创建：

```csharp
namespace WealthOS.Application.DTOs;

public record YourEntityDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Value { get; init; }
}
```

#### 步骤 7: 注册 DI

在 `src/WealthOS.Presentation/App.xaml.cs` 的 `ConfigureServices` 方法中添加：

```csharp
services.AddSingleton<IYourEntityRepository, YourEntityRepository>();
services.AddSingleton<YourEntityService>();
```

---

### 新增一个页面

#### 步骤 1: 创建 ViewModel

在 `src/WealthOS.Presentation/ViewModels/` 创建：

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WealthOS.Presentation.ViewModels;

public partial class YourPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Your Page";

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        // 加载数据...
    }
}
```

#### 步骤 2: 创建 View

在 `src/WealthOS.Presentation/Views/` 创建 XAML 文件：

```xml
<UserControl x:Class="WealthOS.Presentation.Views.YourPageView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <TextBlock Text="{Binding Title}" Style="{StaticResource PageTitleStyle}"/>
    </Grid>
</UserControl>
```

创建 code-behind：

```csharp
using System.Windows.Controls;

namespace WealthOS.Presentation.Views;

public partial class YourPageView : UserControl
{
    public YourPageView()
    {
        InitializeComponent();
    }
}
```

#### 步骤 3: 注册 ViewModel

在 `src/WealthOS.Presentation/App.xaml.cs` 添加：

```csharp
services.AddTransient<YourPageViewModel>();
```

#### 步骤 4: 添加导航

在 `MainWindowViewModel` 中：

1. 添加字段：
```csharp
private readonly YourPageViewModel _yourPageVm;
```

2. 注入构造函数参数
3. 在 `NavigateTo` 方法添加 case

#### 步骤 5: 添加 DataTemplate

在 `MainWindow.xaml` 的 `ContentControl.Resources` 中添加：

```xml
<DataTemplate DataType="{x:Type vm:YourPageViewModel}">
    <views:YourPageView/>
</DataTemplate>
```

#### 步骤 6: 添加侧边栏按钮

在 `MainWindow.xaml` 的导航区域添加按钮。

---

## 调试技巧

### 查看数据库

数据库位置：`%LOCALAPPDATA%/WealthOS/wealthos.db`

可以使用 DB Browser for SQLite 查看和编辑数据。

### 日志

当前版本没有集成日志框架。如需调试：
1. 使用 `System.Diagnostics.Debug.WriteLine()`
2. 使用 Visual Studio 输出窗口

### 常见问题

#### 构建失败

1. 清理解决方案：`dotnet clean`
2. 还原包：`dotnet restore`
3. 重新构建：`dotnet build`

#### 运行时崩溃

1. 检查数据库文件是否损坏
2. 删除数据库文件重新启动
3. 检查 `%LOCALAPPDATA%/WealthOS/` 目录权限

---

## 测试

### 单元测试 (待实现)

```bash
dotnet test
```

### 手动测试清单

- [ ] Dashboard 加载正常
- [ ] 资产 CRUD 正常
- [ ] 负债 CRUD 正常
- [ ] 收支 CRUD 正常
- [ ] 目标 CRUD 正常
- [ ] 导航切换正常
- [ ] 数据持久化正常

---

## 性能优化

### 数据库

- 定期清理过期数据
- 监控查询性能
- 考虑添加缓存层

### UI

- 大列表使用虚拟化
- 图表数据点限制在 1000 以内
- 异步加载避免 UI 阻塞

---

## 部署

### 发布

```bash
dotnet publish src/WealthOS.Presentation -c Release -r win-x64 --self-contained
```

### 安装

将发布产物复制到目标机器，运行 `WealthOS.Presentation.exe`。

数据库自动创建在 `%LOCALAPPDATA%/WealthOS/wealthos.db`。
