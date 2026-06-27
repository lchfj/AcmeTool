/**
 * AcmeTool is a console program for update cert by acme protocol.
 * 
 * Copyright (C) 2026 lchfj.cn
 * 
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Immutable;
using System.Net;
using System.Security.Authentication;
using AcmeLib.Account;
using AcmeLib.Authorization;
using AcmeLib.Cert;
using AcmeLib.Challenge;
using AcmeLib.HttpClient;
using AcmeLib.Key;
using AcmeLib.Order;
using AcmeLib.Renewal;
using AcmeTool.Exceptions;
using AcmeTool.Record;
using AcmeTool.Setting;

namespace AcmeTool.Workflow;

public partial class ApplyCertWorkStep
{
    private readonly string AccountKeyPath = Path.GetFullPath(Path.Combine("orders", "account.key"));

    private readonly HashSet<string> _challengeFiles;


    public AcmeLib.AcmeClient AcmeClient { get; }
    public ApplySetting Setting { get; }
    public LogRecorder Logger { get; }


    private ApplyRecord ApplyExtendSetting = null!;

    public ApplyCertWorkStep(ApplySetting config, LogRecorder errorSender)
    {
        Setting = config;
        Logger = errorSender;

        _challengeFiles = [];
        AcmeClient = new AcmeLib.AcmeClient(config.AcmeDirectory);
    }

    public async Task<bool> CheckApplySettingAsync()
    {
        using var scop = Logger.CreateScoped("申请配置检查");

        await Logger.LogInfomation("开始校验配置");



        var errorMessageList = new List<string>();



        if (Setting.HostList is not { Count: > 0 })
        {
            errorMessageList.Add("未配置域名列表");
        }
        else
        {
            var existsHttp01 = false;
            foreach (var item in Setting.HostList)
            {
                if (ChallengeType.Http01.Equals(item.ChallengeType))
                {
                    existsHttp01 = true;
                    if (item.Value.Contains('*'))
                    {
                        errorMessageList.Add($"泛域名【{item.Value}】不可使用【{ChallengeType.Http01}】认证");
                    }
                }
                else if (ChallengeType.DnsPersist01.Equals(item.ChallengeType))
                {
                    if (item.Type == AcmeLib.Identifier.IdentifierType.Ip)
                    {
                        errorMessageList.Add($"IP地址【{item.Value}】不可使用【{ChallengeType.DnsPersist01}】认证");
                    }
                }
                else
                {
                    errorMessageList.Add($"【{item.Value}】配置的认证方式【{item.ChallengeType}】无效");
                }
            }
            if (existsHttp01)
            {
                if (string.IsNullOrWhiteSpace(Setting.ChallengeFileDirectory))
                {
                    errorMessageList.Add("未配置HTTP01挑战文件存放目录");
                }
                else if (!Directory.Exists(Setting.ChallengeFileDirectory))
                {
                    errorMessageList.Add("HTTP01挑战文件存放目录不存在");
                }
            }
        }


        if (string.IsNullOrWhiteSpace(Setting.CertPath))
        {
            errorMessageList.Add("未配置证书存放路径！");
        }
        else if (!Directory.Exists(Path.GetDirectoryName(Setting.CertPath)))
        {
            errorMessageList.Add("证书存放目录不存在！");
        }


        if (string.IsNullOrWhiteSpace(Setting.CertKeyPath))
        {
            errorMessageList.Add("未配置证书密钥存放路径！");
        }
        else if (!Directory.Exists(Path.GetDirectoryName(Setting.CertKeyPath)))
        {
            errorMessageList.Add("证书密钥存放目录不存在！");
        }

        if (errorMessageList.Count == 0)
        {
            await Logger.LogInfomation("配置校验通过");
            return true;
        }
        else
        {
            await Logger.LogError(errorMessageList);
            return false;
        }
    }

    public async Task<bool> CheckHttp01HostAsync()
    {
        if (!Setting.HostList.Any(h => ChallengeType.Http01.Equals(h.ChallengeType)))
            return true;

        using var scop12 = Logger.CreateScoped("HTTP01校验");

        var filename = Guid.NewGuid().ToString();
        var filepath = Path.Combine(Setting.ChallengeFileDirectory!, filename);
        _challengeFiles.Add(filepath);

        await File.WriteAllTextAsync(filepath, filename);

        using var httpClient = new HttpClient(new HttpClientHandler()
        {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.All,
            ServerCertificateCustomValidationCallback = (a, b, c, d) => true,
            SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
        })
        {
            DefaultRequestVersion = HttpVersion.Version11,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
            Timeout = TimeSpan.FromSeconds(10)
        };

        var result = true;

        var errors = new List<string>();

        foreach (var host in Setting.HostList)
        {
            if (!ChallengeType.Http01.Equals(host.ChallengeType))
                continue;

            var url = $"http://{host.Value}/.well-known/acme-challenge/{filename}";

            try
            {
                var text = await httpClient.GetStringAsync(url);
                if (filename != text)
                {
                    if (text.Length > 128)
                        text = text.Substring(0, 128);

                    errors.Add($"【{host.Value}】读取到的值与期望值不匹配：读取到的值为：\n{text}");
                    result = false;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"【{host.Value}】读取验证测试文件失败：\n{ex.Message}");
                result = false;
            }
        }


        if (errors.Count > 0)
        {
            await Logger.LogError(errors);
        }

        return result;
    }

    public async Task LoadAcmeAccountAsync()
    {
        using var s = Logger.CreateScoped("ACME账户");

        var accountKey = await EncryptHelper.LoadKeyFromFileAsync(AccountKeyPath);

        if (accountKey is not null)
        {
            await Logger.LogInfomation("以已存在的密钥创建或加载账户信息");
        }
        else
        {
            await Logger.LogInfomation("创建新的账户密钥");
            accountKey = KeyManager.CreateNew(Setting.KeyType ?? KeyTypeName.ES384);

            await EncryptHelper.StoreKeyToFileAsync(accountKey, AccountKeyPath);
        }

        var account = await AcmeClient.NewAccount(accountKey, true);
        await Logger.LogInfomation("已成功获取账户", account.Location!.AbsoluteUri);
    }

    public async Task<CertCheckResult> CheckCurrentCertStateAsync()
    {
        using var scoped = Logger.CreateScoped("更新检查");

        {
            await Logger.LogInfomation("检查现有证书状态");

            var currentCert = await CertLite.LoadFromFile(Setting.CertPath);
            var updateStat = await GetCertUpdateState(currentCert);
            if (updateStat == UpdateSateEnum.NotUpdate)
            {
                await Logger.LogInfomation("现有证书有效，无需更新");
                return new CertCheckResult() { Result = UpdateSateEnum.NotUpdate };
            }
        }


        await Logger.LogInfomation("需要更新证书");

        await Logger.LogInfomation("开始检查上一次申请的证书");

        var lastRecord = ApplyRecord.LoadLast(Setting.AcmeDirectory, AcmeClient.AccountKey!, Setting.HostList);
        if (lastRecord is null)
        {
            await Logger.LogInfomation("未找到有效申请记录，开始正常申请流程");
            return new CertCheckResult() { Result = UpdateSateEnum.NormalUpdate };
        }

        // 有上次获取记录
        await Logger.LogInfomation("发现上次申请记录，加载中");

        var lastOrder = await lastRecord.LoadOrderInfo();
        if (lastOrder is not null)
        {
            if (lastOrder.Status == OrderStatus.Valid)
            {
                // 上次申请状态为有效
                var lastCert = await lastRecord.LoadCert();
                if (lastCert is not null)
                {
                    // 上次证书存在
                    var updateStat = await GetCertUpdateState(lastCert);
                    if (updateStat == UpdateSateEnum.NotUpdate)
                    {
                        // 上次的证书有效

                        await Logger.LogInfomation("上次申请的证书还在有效期中，替换现有证书");
                        return new CertCheckResult() { Result = UpdateSateEnum.Replace, LastRecord = lastRecord };
                    }
                    else
                    {
                        await Logger.LogInfomation("上次申请的证书不可使用，启动更新");
                        return new CertCheckResult() { Result = updateStat, LastCert = lastCert };
                    }
                }
            }
            else if (lastOrder.Status is OrderStatus.Pending or OrderStatus.Processing or OrderStatus.Ready)
            {
                if (!lastOrder.Expires.HasValue || lastOrder.Expires.Value > DateTimeOffset.UtcNow)
                {
                    await Logger.LogInfomation("发现上次未完成的申请记录，检查订单状态");

                    var order = await AcmeClient.TryGetOrder(lastOrder.OrderUrl);


                    if (order is null)
                    {
                        await lastRecord.UpdateOrderInfo(new OrderLite()
                        {
                            OrderUrl = lastOrder.OrderUrl,
                            Expires = lastOrder.Expires,
                            Status = OrderStatus.Invalid,
                        });

                        await Logger.LogInfomation("无法读取上次证书申请信息，开始新的申请");
                        return new CertCheckResult() { Result = UpdateSateEnum.NormalUpdate };
                    }

                    await lastRecord.UpdateOrderInfo(order);
                    if (order.Content.Status is OrderStatus.Invalid)
                    {
                        await Logger.LogInfomation("上次证书申请不可继续，开始新的申请");
                        return new CertCheckResult() { Result = UpdateSateEnum.NormalUpdate };
                    }
                    else
                    {
                        await Logger.LogInfomation("上次证书申请可以继续，继续申请");
                        return new CertCheckResult() { Result = UpdateSateEnum.Continue, LastRecord = lastRecord };
                    }
                }
            }
        }

        await Logger.LogInfomation("上次证书申请记录无效，开始新的申请");
        return new CertCheckResult() { Result = UpdateSateEnum.NormalUpdate };
    }

    private async Task<UpdateSateEnum> GetCertUpdateState(CertLite? cert)
    {
        if (cert is null)
            return UpdateSateEnum.NormalUpdate;

        if (!cert.AlternativeName.SetEquals(Setting.HostList.Select(i => i.ToIdentifierInfo())))
        {
            // 当前证书的使用者与设置不符
            return UpdateSateEnum.NormalUpdate;
        }

        var now = DateTimeOffset.UtcNow;

        var remainDay = (cert.NotAfter - now).TotalDays;
        var limitDays = GetLimitDay(cert);

        SuggestedWindowState renewal;
        try
        {
            renewal = await AcmeClient.GetSuggestedTimeState(cert);
        }
        catch
        {
            renewal = SuggestedWindowState.NoneSuggested;
        }

        if (Setting.RemainDayForUpdate.HasValue)
        {
            if (remainDay >= limitDays)
                return UpdateSateEnum.NotUpdate;

            if (renewal == SuggestedWindowState.InTime)
                return UpdateSateEnum.AriUpdate;

            return UpdateSateEnum.NormalUpdate;
        }
        else
        {
            return renewal switch
            {
                SuggestedWindowState.NoneSuggested => remainDay < limitDays ? UpdateSateEnum.NormalUpdate : UpdateSateEnum.NotUpdate,
                SuggestedWindowState.NotStart => UpdateSateEnum.NotUpdate,
                SuggestedWindowState.InTime => UpdateSateEnum.AriUpdate,
                _ => UpdateSateEnum.NormalUpdate,
            };
        }


        int GetLimitDay(CertLite cert)
        {
            if (Setting.RemainDayForUpdate.HasValue)
            {
                return int.Max(2, Setting.RemainDayForUpdate.Value);
            }
            else
            {
                var totalDays = (cert.NotAfter - cert.NotBefore).TotalDays;
                if (totalDays >= 10)
                    return 7;
                else
                    return 2;
            }
        }
    }

    public void SetApplyRecord(ApplyRecord record)
    {
        ApplyExtendSetting = record;
        Logger.SetExtendSetting(record);
    }

    public async Task<AcmeResponse<OrderInfo>> CreatOrderAsync(CertLite? ariCert)
    {
        using var scoped = Logger.CreateScoped("订单");
        await Logger.LogInfomation("生成新订单");

        var hostList = Setting.HostList.Select(i => i.ToIdentifierInfo()).ToImmutableList();
        var order = await AcmeClient.NewOrder(hostList, Setting.Profile, ariCert);

        await Logger.LogInfomation("订单地址为：", order.Location!.AbsoluteUri);
        return order;
    }

    public async Task ApplyCertAsync()
    {
        var orderLite = await ApplyExtendSetting.LoadOrderInfo();
        var orderUrl = orderLite?.OrderUrl ?? throw new NullReferenceException();


        using var socop = Logger.CreateScoped("ACME");

        var cycleIndex = 0;
        while (true)
        {
            if (cycleIndex > 100)
            {
                await Logger.LogError("证书申请未在指定的周期内完成，强行终止");
                return;
            }
            cycleIndex++;


            await Logger.LogInfomation("刷新订单状态");
            var orderResponse = await AcmeClient.GetOrder(orderUrl);
            await ApplyExtendSetting.UpdateOrderInfo(orderResponse);

            var order = orderResponse.Content;
            switch (order.Status)
            {
                case OrderStatus.Pending:
                    await AuthorizateOrderAsync(order);
                    break;
                case OrderStatus.Ready:
                    await SubmitOrderCsrAsync(order);
                    break;
                case OrderStatus.Processing:
                    await Logger.LogInfomation("证书生成中");
                    break;
                case OrderStatus.Valid:
                    await DownloadCertAsync(order);
                    return;
                case OrderStatus.Invalid:
                    await SendOrderErrorInfoAsync(order);
                    break;
                default:
                    await Logger.LogError("证书状态错误：" + order.Status);
                    return;
            }

            await Logger.LogInfomation("本轮业务处理完成，等待10s进行下一轮处理");
            await Task.Delay(10_000);
        }
    }

    private async Task<IKey> CreateCertKeyAsync()
    {
        using var scoped = Logger.CreateScoped("证书密钥");

        await Logger.LogInfomation("尝试读取已存在的密钥");

        var certKey = await EncryptHelper.LoadKeyFromFileAsync(ApplyExtendSetting.CertKeyBackupPath);
        if (certKey is null)
        {
            await Logger.LogInfomation("密钥不存在，生成新的证书密钥");
            certKey = KeyManager.CreateNew(Setting.KeyType ?? KeyTypeName.ES384);

            await EncryptHelper.StoreKeyToFileAsync(certKey, ApplyExtendSetting.CertKeyBackupPath);
        }

        return certKey;
    }

    private async Task AuthorizateOrderAsync(OrderInfo order)
    {
        using var socop = Logger.CreateScoped("域名认证");

        await Logger.LogInfomation("开始对所有域名进行认证");
        foreach (var authUrl in order.Authorizations)
        {
            await AuthorizateOneAsync(authUrl);
        }
    }

    private async Task AuthorizateOneAsync(Uri authUrl)
    {
        var authContext = await AcmeClient.GetAuthorization(authUrl);
        if (authContext is null || authContext.Content is null)
        {
            await Logger.LogInfomation($"获取【{authUrl}】认证信息失败！");
            return;
        }

        var auth = authContext.Content;

        var domain = (auth.Wildcard ? "*." : "") + auth.Identifier.Value;

        using var socopDomain = Logger.CreateScoped(domain);

        switch (auth.Status)
        {
            case AuthorizationStatus.Pending:
                break;
            case AuthorizationStatus.Valid:
                return;
            default:
                await Logger.LogInfomation("认证失败：" + authUrl);
                return;
        }

        await Logger.LogInfomation("开始认证");

        ChallengeInfo? selectedChallenge;
        var setting = Setting.HostList.Find(i => i.Value == domain);
        if (ChallengeType.Http01 == setting?.ChallengeType)
        {
            using var http01 = Logger.CreateScoped(ChallengeType.Http01);

            var httpChallenge = authContext.GetHttp01(AcmeClient);
            if (httpChallenge is null)
            {
                await Logger.LogError($"【{authUrl}】不包含【http-01】认证，申请终止");
                throw new BreakWorkException();
            }

            selectedChallenge = httpChallenge.Challenge;

            await Logger.LogInfomation("生成验证文件");
            var path = Path.Combine(Setting.ChallengeFileDirectory!, httpChallenge.Filename);
            if (!File.Exists(path))
            {
                await File.WriteAllTextAsync(path, httpChallenge.Content);
            }
            _challengeFiles.Add(path);
        }
        else if (ChallengeType.DnsPersist01 == setting?.ChallengeType)
        {
            using var dp01 = Logger.CreateScoped(ChallengeType.DnsPersist01);

            selectedChallenge = authContext.GetChallenge(ChallengeType.DnsPersist01);
            if (selectedChallenge is null)
            {
                await Logger.LogError($"【{authUrl}】不包含【dns-persist-01】认证，申请终止");
                throw new BreakWorkException();
            }
        }
        else
        {
            await Logger.LogError("设置的申请方式不在支持的范围中");
            throw new BreakWorkException();
        }

        await Logger.LogInfomation("提交挑战");
        await AcmeClient.AcknowledgeChallenge(selectedChallenge.Url);
    }

    private async Task SendOrderErrorInfoAsync(OrderInfo order)
    {
        if (order.Error is not null)
        {
            await Logger.LogError("订单无效：", order.Error);
        }
        else
        {
            using var socop = Logger.CreateScoped("域名认证错误");

            foreach (var authUrl in order.Authorizations)
            {
                var authContext = await AcmeClient.GetAuthorization(authUrl);
                var auth = authContext.Content;

                if (auth.Status != AuthorizationStatus.Invalid)
                    continue;

                var domain = (auth.Wildcard ? "*." : "") + auth.Identifier.Value;
                using var socopDomain = Logger.CreateScoped(domain);

                var error = auth.Challenges.FirstOrDefault()?.Error;
                await Logger.LogError("域名认证失败", error);
                break;
            }
        }

        throw new BreakWorkException();
    }

    private async Task SubmitOrderCsrAsync(OrderInfo order)
    {
        using var socopDomain = Logger.CreateScoped("证书请求");

        var certKey = await CreateCertKeyAsync();

        await Logger.LogInfomation("提交");
        if (string.IsNullOrWhiteSpace(Setting.Subject))
            await AcmeClient.Finalize(order, certKey);
        else
            await AcmeClient.Finalize(order, certKey, Setting.Subject);
    }

    private async Task DownloadCertAsync(OrderInfo order)
    {
        using var socopDomain = Logger.CreateScoped("证书下载");
        await Logger.LogInfomation("开始下载证书");


        AcmeResponse<CertInfo> cert;
        if (string.IsNullOrWhiteSpace(Setting.ExpectedIssuer))
            cert = await AcmeClient.DownloadCert(order);
        else
            cert = await AcmeClient.DownloadCertWithIssure(order, Setting.ExpectedIssuer);

        // 保存证书到备份位置
        await File.WriteAllBytesAsync(ApplyExtendSetting.CertBackupPath, cert.Content.CertData);

        await Logger.LogInfomation($"下载完成");
    }

    public async Task ClearChallengeFileAsync()
    {
        foreach (var item in _challengeFiles)
        {
            if (File.Exists(item))
                File.Delete(item);
        }
    }

    public async Task CopyCertAsnc()
    {
        using var sc = Logger.CreateScoped("【证书更新】");

        var certLite = await CertLite.LoadFromFile(ApplyExtendSetting.CertBackupPath);
        var certKey = await EncryptHelper.LoadKeyFromFileAsync(ApplyExtendSetting.CertKeyBackupPath);

        if (certLite is null || certKey is null)
        {
            await Logger.LogError("无法读取新证书信息");
            throw new BreakWorkException();
        }

        File.Copy(ApplyExtendSetting.CertBackupPath, Setting.CertPath);
        await File.WriteAllTextAsync(Setting.CertKeyPath, certKey.PrivateKeyPem);

        var text = $"新证书有效期为【{certLite.NotBefore}】至【{certLite.NotAfter}】";
        await Logger.SendSuccess(text);
    }
}
