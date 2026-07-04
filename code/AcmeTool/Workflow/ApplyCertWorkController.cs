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

using AcmeLib.Exception;
using AcmeTool.Exceptions;
using AcmeTool.Languages;
using AcmeTool.Record;
using AcmeTool.Setting;

namespace AcmeTool.Workflow;

public static class ApplyCertWorkController
{
    public static async Task MainAsync(ApplyArgs args, ApplySetting setting)
    {
        var logger = new LogRecorder(setting);
        if (setting.AcmeDirectory is null || !setting.AcmeDirectory.IsAbsoluteUri)
        {
            await logger.LogError(LangString.Cur.ApplyCert_NoneAcme);
            await DelayExitAsync();
            return;
        }

        // 在这里初始化，主要为了保证 finally 删除生成的挑战文件
        var work = new ApplyCertWorkStep(setting, logger);

        try
        {
            await ApplyWork(work, args);
        }
        catch (BreakWorkException)
        {
            await DelayExitAsync();
        }
        catch (AcmeErrorException ex)
        {
            await logger.LogError(LangString.Cur.ApplyCert_AcmeResponseException, ex.AcmeError);
            await DelayExitAsync();
        }
        catch (Exception ex)
        {
            await logger.LogError(LangString.Cur.ApplyCert_UnhandelException, ex.ToString());
            await DelayExitAsync();
        }
        finally
        {
            await work.ClearChallengeFileAsync();
        }
    }

    private static async Task DelayExitAsync()
    {
        _ = Task.Run(async () =>
        {
            // 10分钟自动退出，防止长期运行
            await Task.Delay(10 * 60 * 1000);
            Environment.Exit(-1);
        });
        Console.WriteLine(LangString.Cur.ApplyCert_ExitWait);
        Console.ReadLine();
    }

    private static async Task ApplyWork(ApplyCertWorkStep work, ApplyArgs fullSetting)
    {
        if (fullSetting.TestMessageSender)
        {
            await work.Logger.TestMessageSendAsync();
            return;
        }


        await work.LoadAcmeAccountAsync();
        if (fullSetting.IsLoadAccountOnly)
            return;

        if (!await work.CheckApplySettingAsync())
            throw new BreakWorkException();


        if (fullSetting.IsCheckHost)
        {
            if (!await work.CheckHttp01HostAsync())
                throw new BreakWorkException();
        }

        var checkResult = new CertCheckResult() { Result = UpdateSateEnum.NormalUpdate };
        if (!fullSetting.IsForceUpdate)
            checkResult = await work.CheckCurrentCertStateAsync();

        switch (checkResult.Result)
        {
            case UpdateSateEnum.NotUpdate:
                return;
            case UpdateSateEnum.Replace:
                work.SetApplyRecord(checkResult.LastRecord!);
                await work.CopyCertAsnc();
                break;
            case UpdateSateEnum.NormalUpdate:
            case UpdateSateEnum.AriUpdate:
                {
                    var record = ApplyRecord.CreateNew(work.Setting.AcmeDirectory, work.AcmeClient.AccountKey!, work.Setting.HostList);
                    work.SetApplyRecord(record);


                    var order = await work.CreateOrderAsync(checkResult.LastCert);
                    await record.UpdateOrderInfo(order);


                    await work.ApplyCertAsync();
                    await work.CopyCertAsnc();
                }
                break;
            case UpdateSateEnum.Continue:
                work.SetApplyRecord(checkResult.LastRecord!);
                await work.ApplyCertAsync();
                await work.CopyCertAsnc();
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
