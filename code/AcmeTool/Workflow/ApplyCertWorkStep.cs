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
using AcmeTool.Languages;
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
        using var scop = Logger.CreateScoped(LangString.Cur.ApplyWork_CheckApply_ScopedTitle);

        await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckApply_Start);



        var errorMessageList = new List<string>();



        if (Setting.HostList is not { Count: > 0 })
        {
            errorMessageList.Add(LangString.Cur.ApplyWork_CheckApply_Err_NoHostList);
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
                        errorMessageList.Add(string.Format(LangString.Cur.ApplyWork_CheckApply_Err_WildcardNotSupportHttp01, item.Value, ChallengeType.Http01));
                    }
                }
                else if (ChallengeType.DnsPersist01.Equals(item.ChallengeType))
                {
                    if (item.Type == AcmeLib.Identifier.IdentifierType.Ip)
                    {
                        errorMessageList.Add(string.Format(LangString.Cur.ApplyWork_CheckApply_Err_IpNotSupportDns01, item.Value, ChallengeType.DnsPersist01));
                    }
                }
                else
                {
                    errorMessageList.Add(string.Format(LangString.Cur.ApplyWork_CheckApply_Err_InvalidChallengeType, item.Value, item.ChallengeType));
                }
            }
            if (existsHttp01)
            {
                if (string.IsNullOrWhiteSpace(Setting.ChallengeFileDirectory))
                {
                    errorMessageList.Add(LangString.Cur.ApplyWork_CheckApply_Err_NoChallengeDir);
                }
                else if (!Directory.Exists(Setting.ChallengeFileDirectory))
                {
                    errorMessageList.Add(LangString.Cur.ApplyWork_CheckApply_Err_ChallengeDirNotExist);
                }
            }
        }


        if (string.IsNullOrWhiteSpace(Setting.CertPath))
        {
            errorMessageList.Add(LangString.Cur.ApplyWork_CheckApply_Err_NoCertPath);
        }
        else if (!Directory.Exists(Path.GetDirectoryName(Setting.CertPath)))
        {
            errorMessageList.Add(LangString.Cur.ApplyWork_CheckApply_Err_CertDirNotExist);
        }


        if (string.IsNullOrWhiteSpace(Setting.CertKeyPath))
        {
            errorMessageList.Add(LangString.Cur.ApplyWork_CheckApply_Err_NoCertKeyPath);
        }
        else if (!Directory.Exists(Path.GetDirectoryName(Setting.CertKeyPath)))
        {
            errorMessageList.Add(LangString.Cur.ApplyWork_CheckApply_Err_CertKeyDirNotExist);
        }

        if (errorMessageList.Count == 0)
        {
            await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckApply_Passed);
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

        using var scop12 = Logger.CreateScoped(LangString.Cur.ApplyWork_CheckHttp01_Scoped);

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

                    errors.Add(string.Format(LangString.Cur.ApplyWork_CheckHttp01_Err_Http01ValueMismatch, host.Value, text));
                    result = false;
                }
            }
            catch (Exception ex)
            {
                errors.Add(string.Format(LangString.Cur.ApplyWork_CheckHttp01_Err_Http01ReadFail, host.Value, ex.Message));
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
        using var s = Logger.CreateScoped(LangString.Cur.ApplyWork_LoadAccount_Scope);

        var accountKey = await EncryptHelper.LoadKeyFromFileAsync(AccountKeyPath);

        if (accountKey is not null)
        {
            await Logger.LogInfomation(LangString.Cur.ApplyWork_LoadAccount_LoadExistAccountKey);
        }
        else
        {
            await Logger.LogInfomation(LangString.Cur.ApplyWork_LoadAccount_CreateNewAccountKey);
            accountKey = KeyManager.CreateNew(Setting.KeyType ?? KeyTypeName.ES384);

            await EncryptHelper.StoreKeyToFileAsync(accountKey, AccountKeyPath);
        }

        var account = await AcmeClient.NewAccount(accountKey, true);
        await Logger.LogInfomation(LangString.Cur.ApplyWork_LoadAccount_CreateNewSuccess, account.Location!.AbsoluteUri);
    }

    public async Task<CertCheckResult> CheckCurrentCertStateAsync()
    {
        using var scoped = Logger.CreateScoped(LangString.Cur.ApplyWork_CheckCurrent_Scoped);

        {
            await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_Start);

            var currentCert = await CertLite.LoadFromFile(Setting.CertPath);
            var updateStat = await GetCertUpdateState(currentCert);
            if (updateStat == UpdateSateEnum.NotUpdate)
            {
                await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_NoNeedUpdate);
                return new CertCheckResult() { Result = UpdateSateEnum.NotUpdate };
            }
        }


        await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_NeedUpdate);

        await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_CheckLast);

        var lastRecord = ApplyRecord.LoadLast(Setting.AcmeDirectory, AcmeClient.AccountKey!, Setting.HostList);
        if (lastRecord is null)
        {
            await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_NoLastRecord);
            return new CertCheckResult() { Result = UpdateSateEnum.NormalUpdate };
        }

        // 有上次获取记录
        await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_LoadLastRecord);

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

                        await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_LastCertValidReplace);
                        return new CertCheckResult() { Result = UpdateSateEnum.Replace, LastRecord = lastRecord };
                    }
                    else
                    {
                        await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_LastCertInvalidUpdate);
                        return new CertCheckResult() { Result = updateStat, LastCert = lastCert };
                    }
                }
            }
            else if (lastOrder.Status is OrderStatus.Pending or OrderStatus.Processing or OrderStatus.Ready)
            {
                if (!lastOrder.Expires.HasValue || lastOrder.Expires.Value > DateTimeOffset.UtcNow)
                {
                    await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_FindUnfinishedRecord);

                    var order = await AcmeClient.TryGetOrder(lastOrder.OrderUrl);


                    if (order is null)
                    {
                        await lastRecord.UpdateOrderInfo(new OrderLite()
                        {
                            OrderUrl = lastOrder.OrderUrl,
                            Expires = lastOrder.Expires,
                            Status = OrderStatus.Invalid,
                        });

                        await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_LastOrderReadFail);
                        return new CertCheckResult() { Result = UpdateSateEnum.NormalUpdate };
                    }

                    await lastRecord.UpdateOrderInfo(order);
                    if (order.Content.Status is OrderStatus.Invalid)
                    {
                        await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_LastOrderNotContinue);
                        return new CertCheckResult() { Result = UpdateSateEnum.NormalUpdate };
                    }
                    else
                    {
                        await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_LastOrderCanContinue);
                        return new CertCheckResult() { Result = UpdateSateEnum.Continue, LastRecord = lastRecord };
                    }
                }
            }
        }

        await Logger.LogInfomation(LangString.Cur.ApplyWork_CheckCurrent_LastRecordInvalid);
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

    public async Task<AcmeResponse<OrderInfo>> CreateOrderAsync(CertLite? ariCert)
    {
        using var scoped = Logger.CreateScoped(LangString.Cur.ApplyWork_CreateOrder_Scoped);
        await Logger.LogInfomation(LangString.Cur.ApplyWork_CreateOrder_Start);

        var hostList = Setting.HostList.Select(i => i.ToIdentifierInfo()).ToImmutableList();
        var order = await AcmeClient.NewOrder(hostList, Setting.Profile, ariCert);

        await Logger.LogInfomation(LangString.Cur.ApplyWork_CreateOrder_OrderUrl, order.Location!.AbsoluteUri);
        return order;
    }

    public async Task ApplyCertAsync()
    {
        var orderLite = await ApplyExtendSetting.LoadOrderInfo();
        var orderUrl = orderLite?.OrderUrl ?? throw new NullReferenceException();


        using var socop = Logger.CreateScoped(LangString.Cur.ApplyWork_ApplyCert_Scoped);

        var cycleIndex = 0;
        while (true)
        {
            if (cycleIndex > 100)
            {
                await Logger.LogError(LangString.Cur.ApplyWork_ApplyCert_Err_ApplyCycleTimeout);
                return;
            }
            cycleIndex++;


            await Logger.LogInfomation(LangString.Cur.ApplyWork_ApplyCert_RefreshOrder);
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
                    await Logger.LogInfomation(LangString.Cur.ApplyWork_ApplyCert_WaitGenerating);
                    break;
                case OrderStatus.Valid:
                    await DownloadCertAsync(order);
                    return;
                case OrderStatus.Invalid:
                    await SendOrderErrorInfoAsync(order);
                    break;
                default:
                    await Logger.LogError(string.Format(LangString.Cur.ApplyWork_ApplyCert_Err_OrderStatusInvalid, order.Status));
                    return;
            }

            await Logger.LogInfomation(LangString.Cur.ApplyWork_ApplyCert_WaitNextCycle);
            await Task.Delay(10_000);
        }
    }

    private async Task<IKey> CreateCertKeyAsync()
    {
        using var scoped = Logger.CreateScoped(LangString.Cur.ApplyWork_CertKey_Scope);

        await Logger.LogInfomation(LangString.Cur.ApplyWork_CertKey_LoadExist);

        var certKey = await EncryptHelper.LoadKeyFromFileAsync(ApplyExtendSetting.CertKeyBackupPath);
        if (certKey is null)
        {
            await Logger.LogInfomation(LangString.Cur.ApplyWork_CertKey_CreateNew);
            certKey = KeyManager.CreateNew(Setting.KeyType ?? KeyTypeName.ES384);

            await EncryptHelper.StoreKeyToFileAsync(certKey, ApplyExtendSetting.CertKeyBackupPath);
        }

        return certKey;
    }

    private async Task AuthorizateOrderAsync(OrderInfo order)
    {
        using var socop = Logger.CreateScoped(LangString.Cur.ApplyWork_DomainAuth_Scoped);

        await Logger.LogInfomation(LangString.Cur.ApplyWork_DomainAuth_Start);
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
            await Logger.LogInfomation(string.Format(LangString.Cur.ApplyWork_DomainAuth_NotFound, authUrl));
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
                await Logger.LogInfomation(string.Format(LangString.Cur.ApplyWork_DomainAuth_StatusError, authUrl));
                return;
        }

        await Logger.LogInfomation(LangString.Cur.ApplyWork_DomainAuth_Challenge_Start);

        ChallengeInfo? selectedChallenge;
        var setting = Setting.HostList.Find(i => i.Value == domain);
        if (ChallengeType.Http01 == setting?.ChallengeType)
        {
            using var http01 = Logger.CreateScoped(ChallengeType.Http01);

            var httpChallenge = authContext.GetHttp01(AcmeClient);
            if (httpChallenge is null)
            {
                await Logger.LogError(string.Format(LangString.Cur.ApplyWork_DomainAuth_Http01_Err_NoChallenge, authUrl));
                throw new BreakWorkException();
            }

            selectedChallenge = httpChallenge.Challenge;

            await Logger.LogInfomation(LangString.Cur.ApplyWork_DomainAuth_Http01_CreateVerifyFile);
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
                await Logger.LogError(string.Format(LangString.Cur.ApplyWork_DomainAuth_DnsPersist01_Err_NoChallenge, authUrl));
                throw new BreakWorkException();
            }
        }
        else
        {
            await Logger.LogError(LangString.Cur.ApplyWork_DomainAuth_Err_Unsupported);
            throw new BreakWorkException();
        }

        await Logger.LogInfomation(LangString.Cur.ApplyWork_DomainAuth_Challenge_Acknowledge);
        await AcmeClient.AcknowledgeChallenge(selectedChallenge.Url);
    }

    private async Task SendOrderErrorInfoAsync(OrderInfo order)
    {
        if (order.Error is not null)
        {
            await Logger.LogError(LangString.Cur.ApplyWork_SendError_OrderError, order.Error);
        }
        else
        {
            using var socop = Logger.CreateScoped(LangString.Cur.ApplyWork_SendError_DomainError_Scoped);

            foreach (var authUrl in order.Authorizations)
            {
                var authContext = await AcmeClient.GetAuthorization(authUrl);
                var auth = authContext.Content;

                if (auth.Status != AuthorizationStatus.Invalid)
                    continue;

                var domain = (auth.Wildcard ? "*." : "") + auth.Identifier.Value;
                using var socopDomain = Logger.CreateScoped(domain);

                var error = auth.Challenges.FirstOrDefault()?.Error;
                await Logger.LogError(LangString.Cur.ApplyWork_SendError_DomainError_AuthFail, error);
                break;
            }
        }

        throw new BreakWorkException();
    }

    private async Task SubmitOrderCsrAsync(OrderInfo order)
    {
        using var socopDomain = Logger.CreateScoped(LangString.Cur.ApplyWork_SubmitCsr_Scope);

        var certKey = await CreateCertKeyAsync();

        await Logger.LogInfomation(LangString.Cur.ApplyWork_SubmitCsr_Send);
        if (string.IsNullOrWhiteSpace(Setting.Subject))
            await AcmeClient.Finalize(order, certKey);
        else
            await AcmeClient.Finalize(order, certKey, Setting.Subject);
    }

    private async Task DownloadCertAsync(OrderInfo order)
    {
        using var socopDomain = Logger.CreateScoped(LangString.Cur.ApplyWork_DownloadCert_Scoped);
        await Logger.LogInfomation(LangString.Cur.ApplyWork_DownloadCert_Start);


        AcmeResponse<CertInfo> cert;
        if (string.IsNullOrWhiteSpace(Setting.ExpectedIssuer))
            cert = await AcmeClient.DownloadCert(order);
        else
            cert = await AcmeClient.DownloadCertWithIssure(order, Setting.ExpectedIssuer);

        // 保存证书到备份位置
        await File.WriteAllBytesAsync(ApplyExtendSetting.CertBackupPath, cert.Content.CertData);

        await Logger.LogInfomation(LangString.Cur.ApplyWork_DownloadCert_Success);
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
        using var sc = Logger.CreateScoped(LangString.Cur.ApplyWork_CopyCert_Scope);

        var certLite = await CertLite.LoadFromFile(ApplyExtendSetting.CertBackupPath);
        var certKey = await EncryptHelper.LoadKeyFromFileAsync(ApplyExtendSetting.CertKeyBackupPath);

        if (certLite is null || certKey is null)
        {
            await Logger.LogError(LangString.Cur.ApplyWork_CopyCert_Err_LoadRecordCertFail);
            throw new BreakWorkException();
        }

        File.Copy(ApplyExtendSetting.CertBackupPath, Setting.CertPath);
        await File.WriteAllTextAsync(Setting.CertKeyPath, certKey.PrivateKeyPem);

        var text = string.Format(LangString.Cur.ApplyWork_CopyCert_Success, certLite.NotBefore, certLite.NotAfter);
        await Logger.SendSuccess(text);
    }
}
