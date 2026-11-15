# AcmeTool Introduction

`AcmeTool` is a console program for update cert by acme protocol.

# Installation 

### Nuget Package

Address：[https://www.nuget.org/packages/AcmeTool]

Install Command：
``` shell
dotnet tool install --global AcmeTool
```

### Binary file

Address：[https://github.com/lchfj/AcmeTool/releases](https://github.com/lchfj/AcmeTool/releases)

There are three versions of the binary files included.

1. `AcmeTool.AnyCpu.dotnet10.zip` Cross-platform universal version, which can run in any environment that supports `.net 10.0`, and requires the installation of the `.net 10.0` runtime [https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0/runtime].

2. `AcmeTool.AnyCpu.dotnet11.zip` Cross-platform universal version, which can run in any environment that supports `.net 11.0`, and requires the installation of the `.net 11.0` runtime [https://dotnet.microsoft.com/zh-cn/download/dotnet/11.0/runtime].

3. `AcmeTool.win-x64_native.zip` Only for Windows x64 operating system, Native AOT compiled version, without the need for dotnet runtime.


# Usage 

### Working directory


To store the Acme account key and historical application information, please specify a fixed working directory before launching this application. The final generated working directory structure is as follows
| File | Explanation |
|--|--|
| setting.json | The default configuration file storage location can be changed in the startup parameters. |
| orders | Order information storage directory |
| - account.key | Acme account key (encrypted storage, reused when in the same working directory, does not distinguish between CAs) |
| - [xxxxxxxx] | The directory in Base64Url format, different application configurations use different directories to distinguish each other. |
| - - [xxxxxxxx] | Timestamp format directory, information of multiple orders applied for under the same configuration, logs and backups |
| - - - order.json | Simple information of the order |
| - - - cert.log| Certificate Application Log |
| - - - cert.crt | Certificate backup |
| - - - cert.key | Certificate key (encrypted storage) |

### Startup parameters

#### Nuget package OR win-x64_native

```shell
AcmeTool [-ld] [-fu] [-ch] [-tm] [-de] [filepath]
```

#### AnyCpu 

```shell
dotnet AcmeTool.dll [-ld] [-fu] [-ch] [-tm] [-de] [filepath]
```

#### 参数说明

| Switch | Explanation |
|--|--|
| -ld | Only generate the content of the Txt record used by dns-persist-01. |
| -fu | Force certificate update: Regardless of whether the existing certificate is valid or not, force the update process to be initiated. |
| -ch | Before updating the certificate, check whether the http-01 configuration is valid. By default, no check is performed. |
| -tm | Test message sending: Do not update the certificate, only test whether the message sending method specified in the configuration file is effective. |
| -de | Decrypt the key file: The decrypted PEM format key will be output in the console. At this point, the `filepath` parameter must not be empty. |
| filepath | File path:<br />When updating certificates or sending test messages, it is the configuration file path. The default is the `setting.json` file in the working directory. <br /> When decrypting the key file, it is the key file path and cannot be empty. |


### Configuration file

If the file is not found at the default or specified configuration file path during startup, the sample template content will be output to the location.

The configurable content can view the output template or view：[https://github.com/lchfj/AcmeTool/tree/main/code/AcmeTool/Setting/SettingTemplateEn.json]

### Scheduled task

The application itself does not run for an extended period. It will automatically terminate its operation once it completes one round of tasks. Therefore, it is necessary to set up scheduled tasks using tools such as task plans (in Windows), cron (in Linux), or launchd (in macOS).。

It is recommended to start at a random time (please avoid whole hours or half hours). The application should run every 8 hours. The application will automatically determine whether the certificate needs to be updated and can also prevent failure due to network issues during a single application attempt.

# The supported Acme protocol

### Challenge

1. HTTP-01（[rfc8555](https://datatracker.ietf.org/doc/html/rfc8555)）；

2. Dns-Persist-01（[draft-ietf-acme-dns-persist-02](https://datatracker.ietf.org/doc/html/draft-ietf-acme-dns-persist-02)）（Not all CAs support this type of challenge. Please confirm the support status before using.）；

### Identifier 

1. DNS（[rfc8555](https://datatracker.ietf.org/doc/html/rfc8555)）；

2. IP（[rfc8738](https://datatracker.ietf.org/doc/html/rfc8738)）；

### Extension

1. Profiles Extension（[draft-ietf-acme-profiles-03](https://datatracker.ietf.org/doc/html/draft-ietf-acme-profiles-03)）；

2. ARI Extension（[rfc9773](https://datatracker.ietf.org/doc/html/rfc9773)）（No switch parameters. This extension will be automatically applied when it is available.）；

# License

This program is released under the `AGPLv3` license.

# Third-party dependency libraries

MailKitLite & MimeKitLite v4.18.1

License: MIT 

Copyright (C) 2013-2026 .NET Foundation and Contributors

SourceCode : [https://github.com/jstedfast/MailKit](https://github.com/jstedfast/MailKit)

---

This document has been translated by youdao.com