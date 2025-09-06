# selectCourse

这是一个基于 .NET 的天蛙云选课自动抢课的解决方案，包含控制台/后台程序和 WPF GUI 两个项目。该仓库由 `selectCourse`（核心库/控制台）和 `SelectCourseGUI`（WPF 前端）组成。

GUI项目仍在开发过程中。

### 目录结构
- `selectCourse/`：核心应用程序项目。
- `SelectCourseGUI/`：WPF 桌面 GUI 项目，仍在开发中。
- `LICENSE`：项目授权信息。
- `selectCourse.sln`：Visual Studio 解决方案文件。

### 关键文件
- `selectCourse/Program.cs`：控制台或程序入口。
- `selectCourse/App.config`：应用程序配置（如果使用传统配置系统）。
- `selectCourse/selectCourse.csproj`：核心项目的项目文件。


### 环境与依赖
- 需要安装 .NET 9 SDK
- 推荐使用 Microsoft Visual Studio 2022 或更新版本，安装 .NET 桌面开发工作负载以获得最佳开发体验。
- Windows 上若要运行 WPF GUI，请使用支持 Windows 桌面（WPF）的 SDK，例如 `dotnet` 在 Windows 上的桌面支持。
- 项目使用 `Newtonsoft.Json`，如果从源码构建，NuGet 会自动恢复依赖。

### 快速开始开发
1. 安装 .NET SDK：从 https://dotnet.microsoft.com/download 下载并安装适合的版本（推荐 .NET 9）。
2. 在仓库根目录打开 PowerShell（`pwsh.exe`）：

```powershell
# 还原依赖并构建解决方案
dotnet restore selectCourse.sln
dotnet build selectCourse.sln -c Debug

# 运行控制台/核心应用（示例）
dotnet run --project .\selectCourse\selectCourse.csproj -c Debug

# 运行 WPF GUI（需在 Windows 环境中）
dotnet run --project .\SelectCourseGUI\SelectCourseGUI.csproj -c Debug
```

发布（生成可分发部署包）
```powershell
# 例如对 net8.0 为 x64 发布为单文件可执行（示例）
dotnet publish .\selectCourse\selectCourse.csproj -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true -o .\publish\selectCourse\

# 发布 WPF GUI（示例）
dotnet publish .\SelectCourseGUI\SelectCourseGUI.csproj -c Release -r win-x64 -o .\publish\SelectCourseGUI\
```

### 运行已经编译的二进制
- 仓库中 [Releases 界面](https://github.com/yangnuozhen/selectCourse/releases/latest) 已包含可执行文件，可直接双击或从命令行运行。

### 常见问题及排查
- 恢复依赖失败：确保网络连接可用并且 `nuget.org` 可访问，执行 `dotnet restore`。
- WPF 项目无法运行：请在 Windows 上安装桌面开发相关的 .NET SDK，确认目标框架与本地 SDK 版本兼容。


### 许可证
- 参见仓库根目录的 `LICENSE` 文件。

