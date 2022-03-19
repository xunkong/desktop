# 寻空

## 构建

### 前提条件

- Windows 10 1809 及以上版本
- Visual Studio 2022 及以上版本

### 生成客户端

- 安装 Visual Studio 工作负载
  - .NET 桌面开发
  - 使用 C++ 的桌面开发
  - 通用 Windows 平台开发
- 安装 Windows App SDK
  - 若使用 Visual Studio 17.1 及以上版本，在上一步选择工作负载的窗口右侧勾选 「.NET 桌面开发 / Windows 应用 SDK C# 模板」
  - 若使用其他版本，前往 [此页面](https://docs.microsoft.com/zh-cn/windows/apps/windows-app-sdk/downloads) 下载 1.0 稳定版的 Windows App SDK
- 打开解决方案 `Xunkong.sln`，并将项目 `Xunkong.Desktop.Package` 设为启动项目，配置设为 `x64`
- 如要打包发布，请更换自签名证书（参考 #38）

到此您已经完成了生成客户端的全部准备。

### 生成服务端

- 安装 Visual Studio 工作负载 「ASP.NET 和 Web 开发」
- 安装 MySQL 8.0 的最新版本
- 在项目 `Xunkong.Web.Api` 的 `appsettings.json` 文件中修改连接字符串
- 迁移数据库架构
  - 安装 EF Core CLI 工具，命令行运行 `dotnet tool install --global dotnet-ef`
  - 更新数据库表结构，在 `Xunkong.Web.Api` 项目的文件夹内使用命令行运行 `dotnet ef database update`
  - 这里有个坑，实体结构 `Xunkong.Core.Wish.WishlogAuthkeyItem` 使用了 `Url (string)` 作为主键，对应数据库类型为 `varchar(4096)` ，但是 MySql 的索引大小限制为 `3072 bytes` ，首次迁移需要生成 SQL 后手动修改索引大小。

到此您已经完成了生成服务端的全部准备，单独运行则需要修改 `appsettings.json` 文件中的端口。

## 项目结构

我深知 **想要学习一个项目但是下载代码后看到一堆文件夹和文件却无从下手** 的痛苦，所以画了个表让大家能够快速的了解本项目的结构，更详细的内容就要去看源码了，初次写文档，请多多包含，欢迎提出改进建议。

项目主要分为三个部分 `公用库 Xunkong.Core` ， `桌面端 Xunkong.Desktop` ， `服务端 Xunkong.Web.Api` ，下面简要地介绍一下各命名空间所实现的功能。

| 命名空间 | 概述 | 备注 |
| ------- | ---- | --- |
| **Xunkong.Core** | **共用库** |  |
| Artifact | 圣遗物 | 暂时无用 |
| Hoyolab | 米游社相关功能，包含已拥有角色信息、实时便笺、旅行札记、探索进度等 |  |
| Metadata | 角色、武器等 | 通过 `XunkongApi.XunkongApiClient` 获取数据 |
| SpiralAbyss | 深境螺旋 | 通过 `Hoyolab.HoyolabClient` 获取数据 |
| TravelRecord | 旅行札记 | 通过 `Hoyolab.HoyolabClient` 获取数据 |
| Wish | 祈愿 |      |
| XunkongApi | 服务端 API，包含更新、通知、祈愿记录备份等 |  |
| **Xunkong.Desktop** | **桌面端，这里的项目有点多** | **以下加粗部分为项目(文件夹)名称** |
| **Desktop** | **桌面端主体部分，WinUI3 框架** |  |
| Controls | 用户控件、自定义控件，没有 `ViewModel` | |
| Converters | 后台到页面的数据转换 | |
| Helpers | 全局静态方法，包含提示横幅、内部设置、导航等 | 操作UI的功能必须从主线程调用 |
| Messages | 使用 `CommunityToolkit.Mvvm.Messaging` 定义的消息模板 | 跨页面信息传递 |
| Models | 根据页面显示需要定义的数据结构 | 一个文件中可能有很多类，这些类都用于一个界面 |
| Pages | 导航页面，页面内引用 `ViewModel` |  |
| Services | 封装了 `Xunkong.Core` 和操作数据库的相关方法 |  |
| Themes | 样式 | Copy from [microsoft-ui-xaml](https://github.com/microsoft/microsoft-ui-xaml) |
| Toolbox | 工具箱，一个工具仅有一个类 |  |
| ViewModels | 视图模型（应该这么翻译吧） |  |
| Views | 用户控件，包含 `ViewModel` |  |
| **BackgroudTask** | **UWP 应用模型的后台任务，遵循 UWP 权限控制，不能访问外部数据库，此项目会调用 Extension** | **必须得有这个才能使用后台任务，参考 [CsWinRT/BgTaskComponent](https://github.com/microsoft/CsWinRT/tree/master/src/Samples/BgTaskComponent)** |
| **Database** | **使用 EFCore 设计的数据库模型** | **WinUI3 暂时不能使用 `dotnet ef` 迁移数据库，只能单独作为一个项目** |
| Migrations | 迁移记录 |  |
| Models | 仅在桌面数据库中使用的相关模型 |  |
| Services | 数据库上下文（这几个字不太好理解） |  |
| **Extension** | **后台任务调用的方法，包含更新磁贴和米游社签到** |  |
| **FpsUnlocker** | **启动游戏以及解锁帧数上限** | **Copy from [genshin-fps-unlock](https://gitee.com/Euphony_Facetious/genshin-fps-unlock)** |
| **Package** | **桌面项目打包** |  |
| **Secret** | **不适合保存在仓库的内容用这个在生成时加入源代码** | **源代码生成器真好用** |
| **Xunkong.Web.Api** | **服务端** | **用于阿里云函数计算** |
| Controllers | 控制器 |  |
| Filters | 筛选器，异常捕获和操作记录 |  |
| Migrations | 迁移记录 |  |
| Models | 仅在服务端使用的数据模型 |  |
| Services | 数据库上下文 | 业务代码都在控制器里了 |
