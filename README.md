# Freshman Bootstrap

Developer Freshman Bootstrap Toolkit for Windows  
萌新秒装：新手开发者 Windows 开发环境配置工具箱

Freshman Bootstrap 是面向 Windows 新手开发者和新生的开发环境配置工具。项目将常用软件的安装包封入单文件安装器，通过图形界面完成静默安装、已有安装检测和环境配置修复，使用时无需联网。

## 计划支持 <sub>Planned</sub>

- Windows 10/11 x64；首版要求登录账户本身具有管理员权限，程序启动时请求提权。
- 内置 Git、PowerShell 7+、VS Code、Python 3 和 Node.js 22+ 的安装包。
- 检测现有安装情况，并按需逐项进行安装或修复环境配置。
- 通过 Inno Setup 打包为单个安装文件；正式版计划由 GitHub Actions 定期构建发布。

## 开发 <sub>Development</sub>

项目开发需要 Windows 和 .NET 10 SDK。可在 Visual Studio 中打开 `FreshmanBootstrap.slnx`，或在项目根目录运行：

```powershell
dotnet build .\FreshmanBootstrap.slnx
dotnet test .\tests\FreshmanBootstrap.Tests\FreshmanBootstrap.Tests.csproj
dotnet run --project .\src\FreshmanBootstrap.App\FreshmanBootstrap.App.csproj
```

为了准备内置的第三方安装包缓存，需要下载额外文件到仓库 `payload/` 目录下：

```powershell
dotnet run --project .\src\FreshmanBootstrap.App\FreshmanBootstrap.App.csproj -- --prepare
```

> 这个操作会在命中缓存时校验本地文件哈希（优先使用官方提供的哈希，其次使用上次记录在 `payload/manifest.json` 的哈希）。若追加 `--fresh` 参数可强制重新下载全部第三方安装包。

软件的本地打包需要 Inno Setup 6。可运行以下命令进行构建，并打包为 Inno Setup 单文件安装器：

```powershell
dotnet publish .\src\FreshmanBootstrap.App\FreshmanBootstrap.App.csproj -c Release -p:PublishProfile=win-x64 -p:Version=202609.0
& "你的InnoSetup安装路径\ISCC.exe" /DMyAppVersion=202609.0 .\installer\FreshmanBootstrap.iss
```

项目结构：

- `src/FreshmanBootstrap.Core`：与 UI 无关的检测、PATH 修复、下载与软件定义
- `src/FreshmanBootstrap.App`：WPF 界面与 Windows 平台实现
- `tests/FreshmanBootstrap.Tests`：单元测试（不会在本机执行安装或环境修改）
- `installer`：Inno Setup 打包脚本

## 许可证 <sub>Licensing</sub>

本项目基于 **BSD-3 开源协议**。任何人都可以自由地使用和修改项目内的源代码，前提是要在源代码或版权声明中保留作者说明和原有协议，且不可以使用本项目名称或作者名称进行宣传推广。
