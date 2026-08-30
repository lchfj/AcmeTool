# AcmeTool 简介

`AcmeTool` 是一个用于通过 ACME 协议更新证书的控制台程序。

`AcmeTool` is a console program for update cert by acme protocol.

[English](https://github.com/lchfj/AcmeTool/tree/main/doc/README.en.md)

# 安装方法

### Nuget 包

地址：[https://www.nuget.org/packages/AcmeTool]
安装命令：
``` shell
dotnet tool install --global AcmeTool
```

### 二进制文件

地址：[https://github.com/lchfj/AcmeTool/releases](https://github.com/lchfj/AcmeTool/releases)

当前包含两个版本的二进制文件

1. `AcmeTool.AnyCpu.zip` 跨平台通用版本，可在任何支持 `dotnet 10.0` 的环境中运行，需安装 `dotnet 10.0` 运行时。

2. `AcmeTool.win-x64_native.zip` 仅限 Windows x64 操作系统，Native AOT编译版本，无需 dotnet 运行时。


# 使用方式

### 工作目录

为存储Acme账户密钥及历史申请信息，请在通过启动本应用前指定固定的工作目录，最终生成的工作目录结构如下。
| 文件 | 说明 |
|--|--|
| setting.json | 默认的配置文件存放地址，可在启动参数中更改 |
| orders | 订单信息存储目录 |
| - account.key | Acme账户密钥（加密存储，同工作目录时复用，不区分CA） |
| - [xxxxxxxx] | Base64Url格式的目录，不同申请配置使用不同目录以区分 |
| - - [xxxxxxxx] | 时间戳格式的目录，相同配置下多次申请的订单信息、日志及备份 |
| - - - order.json | 订单简易信息 |
| - - - cert.log| 证书申请日志 |
| - - - cert.crt | 证书备份 |
| - - - cert.key | 证书密钥（加密存储） |

### 启动参数

#### Nuget包 及 win-x64_native

```shell
AcmeTool [-la] [-fu] [-ch] [-tm] [-de] [filepath]
```

#### AnyCpu 

```shell
dotnet AcmeTool.dll [-la] [-fu] [-ch] [-tm] [-de] [filepath]
```

#### 参数说明

| 开关 | 说明 |
|--|--|
| -la | 仅创建账户：创建用户账户后终止，控制台输出账户地址，用于 dns-persist-01 配置 |
| -fu | 强制更新证书：无论现有证书是否有效，强制启动更新流程 |
| -ch | 更新证书前检查 http-01 配置是否有效，默认不检查 |
| -tm | 测试消息发送：不更新证书，仅测试配置文件传入的消息发送方法是否有效 |
| -de | 解密密钥文件：将在控制台输出解密后的pem格式密钥，此时 `filepath` 参数不能为空 |
| filepath | 文件路径：<br />更新证书或测试消息发送时为配置文件路径，默认为工作目录下的 `setting.json` 文件 <br /> 解密密钥文件时为密钥文件路径，不可为空 |


### 配置文件

如果启动时在默认或指定的配置文件路径上没有发现文件，则将示例模板内容输出到位置上。

可配置内容可以查看输出的模板，或者查看：[https://github.com/lchfj/AcmeTool/tree/main/code/AcmeTool/Setting/SettingTemplate.json]

### 定时运行

应用本身不会长期运行，执行完一轮任务就会自动结束运行。所以需要以任务计划（windows）、cron（linux）或者  launchd（macOS）等设置定时任务。

建议以一个随机的时间（请避免整点或半点）为开始，每隔8小时运行一次。应用会自动判断证书是否需要更新，也可避免单次申请时网络故障造成失败。

# 支持的Acme协议范围

### 支持的挑战类型

1. HTTP-01（[rfc8555](https://datatracker.ietf.org/doc/html/rfc8555)）；

2. Dns-Persist-01（[Dns-Persist-01](https://datatracker.ietf.org/doc/html/draft-ietf-acme-dns-persist-01)）（并非所有CA均支持该挑战类型，使用前先确认支持情况）；

### 支持的标识符类型

1. DNS（[rfc8555](https://datatracker.ietf.org/doc/html/rfc8555)）；

2. IP（[rfc8738](https://datatracker.ietf.org/doc/html/rfc8738)）；

### 支持的扩展

1. Profiles Extension（[Profiles Extension](https://datatracker.ietf.org/doc/html/draft-ietf-acme-profiles-01)）；

2. ARI Extension（[rfc9773](https://datatracker.ietf.org/doc/html/rfc9773)）（无开关参数，在可使用时自动应用该扩展）；

# 许可证

本应用基于 `AGPLv3` 协议发布。

# 第三方依赖库

MailKitLite v4.7.*

MimeKitLite v4.7.*

License: MIT 

Copyright (C) 2013-2026 .NET Foundation and Contributors

SourceCode : [https://github.com/jstedfast/MailKit](https://github.com/jstedfast/MailKit)