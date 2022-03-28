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
| **RegInvoke** | **用于在切换账号时读写注册表** | **包内进程只能读写虚拟化的注册表** |
| **Secret** | **不适合保存在仓库的内容用这个在生成时加入源代码** | **源代码生成器真好用** |
| **Xunkong.Web.Api** | **服务端** | **用于阿里云函数计算** |
| Controllers | 控制器 |  |
| Filters | 筛选器，异常捕获和操作记录 |  |
| Migrations | 迁移记录 |  |
| Models | 仅在服务端使用的数据模型 |  |
| Services | 数据库上下文 | 业务代码都在控制器里了 |
