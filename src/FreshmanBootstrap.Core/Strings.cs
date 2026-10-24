namespace FreshmanBootstrap.Core;

public static class Strings
{
    public static class Common
    {
        public const string AppTitle = "萌新秒装 Freshman Bootstrap";
        public const string Loading = "正在检测…";
        public const string Ok = "确定";
        public const string Cancel = "取消";
    }

    public static class MainWindow
    {
        public const string EnvironmentPageName = "环境安装";
        public const string AboutPageName = "关于软件";
    }

    public static class EnvironmentPage
    {
        public const string Name = "名称";
        public const string Usage = "用途";
        public const string PackagedVersion = "秒装封包版本";
        public const string InstalledVersion = "已安装的版本";
        public const string Status = "状态";
        public const string Actions = "操作";
        public const string Install = "一键安装";
        public const string Upgrade = "可升级";
        public const string Repair = "立即修复";
        public const string Retry = "重试";
        public const string Refresh = "重新检测";
        public const string Installed = "已安装";
        public const string NotInstalled = "未安装";
        public const string NeedsRepair = "需要额外操作";
        public const string Ready = "已就绪";
        public const string Working = "处理中…";
        public const string Failed = "失败";
        public const string Unknown = "未知";
        public const string VersionPlaceholder = "-";
        public const string InstalledVersionsHeader = "当前已安装的版本";
        public const string InstalledVersionLineFormat = "{0} ({1})";
        public const string PostProcessHeader = "额外步骤检查单";
        public const string PayloadMissing = "封包中缺少 {0} 的安装包，请重新获取本软件的最新版本。";
        public const string PackageFileMissing = "安装包文件缺失：{0}";
        public const string InstallFailed = "{0} 安装失败：{1}";
        public const string RepairFailed = "{0} 修复失败：{1}";
        public const string DetectionFailed = "{0} 检测失败：{1}";
        public const string ProcessFailed = "安装程序返回代码 {0}。{1}";
        public const string ProcessTimedOut = "安装程序超时未完成。";
        public const string StageInstalling = "正在安装 {0}…";
    }

    public static class UpgradeDialog
    {
        public const string Title = "确认升级";
        public const string MessageFormat = "是否要将 {0} 从 {1} 升级到 {2} 版本？\n您需要确保已关闭了正在运行的 {0} 程序。";
        public const string Confirm = "确认升级";
    }

    public static class PathRepairDialog
    {
        public const string Title = "确认修复";
        public const string Summary = "为了修复此问题，需要对 PATH 环境变量配置进行修改。";
        public const string Header = "以下相关路径条目将被调整，其他条目保持不变：";
        public const string MachineLevel = "系统级 PATH";
        public const string UserLevel = "用户级 PATH";
        public const string Confirm = "确认修复";
        public const string KindAdd = "新增";
        public const string KindMove = "调整顺序";
        public const string KindRemove = "移除";
    }

    public static class AboutPage
    {
        public const string VersionLabel = "当前版本：{0}";
        public const string GitHubLabel = "GitHub：{0}";
        public const string Uninstall = "卸载本软件";
        public const string UninstallHint = "如您已配置好所有环境，您可以无痕卸载本软件，卸载后不会删除已安装的软件和配置。感谢您的使用！";
        public const string UninstallConfirmTitle = "卸载确认";
        public const string UninstallConfirmMessage = "确定要卸载萌新秒装吗？已安装的软件和环境配置不会被删除。";
        public const string UninstallFailed = "无法启动卸载程序：{0}";
    }

    public static class Software
    {
        public const string GitName = "Git";
        public const string GitDescription = "版本控制工具";
        public const string PowerShellName = "PowerShell 7+";
        public const string PowerShellDescription = "跨平台命令行与脚本环境";
        public const string VSCodeName = "VS Code";
        public const string VSCodeDescription = "集成开发环境";
        public const string PythonName = "Python 3";
        public const string PythonDescription = "编程语言";
        public const string NodeJsName = "Node.js 22+";
        public const string NodeJsDescription = "JavaScript 运行时";
    }

    public static class PostProcess
    {
        public const string GitFirst = "确保最高版本的 git.exe 是 PATH 所有候选项中的第一个";
        public const string PowerShellFirst = "确保最高版本的 pwsh.exe 是 PATH 所有候选项中的第一个";
        public const string VSCodeFirst = "确保 PATH 中的 code 命令解析到已安装的最高版本";
        public const string PythonAddPath = "添加 Python 3 相关目录到 PATH 环境变量";
        public const string PythonWindowsApps = "确保 WindowsApps 在 PATH 中的优先级不高于任何 Python 3 相关目录";
        public const string PythonFirst = "确保最高版本的 python.exe 是 PATH 中所有候选项中的第一个";
        public const string NodeJsFirst = "确保最高版本的 node.exe 是 PATH 所有候选项中的第一个";
    }

    public static class InstallOptions
    {
        public const string InstallDirectory = "安装目录";
        public const string DesktopIcon = "创建桌面图标";
        public const string PythonLauncher = "安装 py 启动器（推荐）";
        public const string PathRequired = "请填写安装目录。";
        public const string PathMustBeAbsolute = "安装目录必须是绝对路径。";
        public const string PathInvalid = "安装目录包含无效字符。";
        public const string PathIsFile = "安装目录与现有文件冲突。";
        public const string PathIsRoot = "安装目录不能是驱动器根目录。";
    }

    public static class Prepare
    {
        public const string Resolving = "正在解析 {0} 的最新版本…";
        public const string Downloading = "正在下载 {0} …";
        public const string Downloaded = "已下载 {0} ({1})";
        public const string Skipped = "缓存校验通过，跳过下载：{0}";
        public const string ResolveFailed = "无法解析 {0} 的下载地址。";
        public const string DownloadFailed = "下载 {0} 失败：{1}";
        public const string HashMismatch = "{0} 的校验值不匹配。";
        public const string CacheInvalid = "缓存文件校验不通过，重新下载：{0}";
        public const string ForceRefresh = "已指定 --fresh，强制重新下载：{0}";
    }
}
