# 寻空

> 项目将拆分成多个仓库，本仓库仅专注于桌面端的内容。

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

## 相关链接

[项目结构](./DevelopGuide.md)