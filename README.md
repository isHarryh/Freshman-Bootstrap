# Freshman Bootstrap

Developer Freshman Bootstrap Toolkit for Windows  
萌新秒装：新手开发者 Windows 开发环境配置工具箱

Freshman Bootstrap 是面向 Windows 新手开发者和新生的开发环境配置工具。项目将常用软件的安装包封入单文件安装器，通过图形界面完成静默安装、已有安装检测和环境配置修复，使用时无需联网。

## 计划支持 <sub>Planned</sub>

- Windows 10/11 x64；首版要求登录账户本身具有管理员权限，程序启动时请求提权。
- 内置 Git、PowerShell 7+、VS Code、Python 3 和 Node.js 22+ 的安装包。
- 检测现有安装，按需安装或修复环境配置；支持选择多个软件依次处理。
- 通过 Inno Setup 打包为单个安装文件；正式版计划由 GitHub Actions 定期构建发布。

## 开发 <sub>Development</sub>

需要 Windows 和 .NET 10 SDK。可在 Visual Studio 中打开 `Freshman-Bootstrap.sln`，或在项目根目录运行：

```powershell
dotnet build .\Freshman-Bootstrap.sln
dotnet run --project .\Freshman-Bootstrap.csproj
```

## 许可证 <sub>Licensing</sub>

本项目基于 **BSD-3 开源协议**。任何人都可以自由地使用和修改项目内的源代码，前提是要在源代码或版权声明中保留作者说明和原有协议，且不可以使用本项目名称或作者名称进行宣传推广。
