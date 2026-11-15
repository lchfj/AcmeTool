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

using AcmeLib.Challenge;
using AcmeTool.Exceptions;
using AcmeTool.Record;
using AcmeTool.Setting;

namespace AcmeTool.Workflow;

public static class ApplyCertWorkController
{
    public static async Task LoadDnsPersist01Txt(ApplyCertWorkStep work, ApplyArgs args)
    {
        var support = await work.AcmeClient.IsSupportDnsPersist01();
        if (!support)
        {
            await work.Logger.LogError(Languages.LangString.Cur.DnsPersist01_Ca_UnSupport);
            return;
        }

        await work.LoadAcmeAccountAsync();

        ConsoleHelper.WriteLine(Languages.LangString.Cur.DnsPersist01_InputName);
        var domain = ConsoleHelper.ReadText();

        if (domain == "*")
        {
            ConsoleHelper.WriteLine(Languages.LangString.Cur.DnsPersist01_InputWildcard);
        }
        else
        {
            var text = string.Format(Languages.LangString.Cur.DnsPersist01_InputWildcard_Special, domain);
            ConsoleHelper.WriteLine(text);
        }
        var wildcard = ConsoleHelper.ReadYesOrNo();

        ConsoleHelper.WriteLine(Languages.LangString.Cur.DnsPersist01_InputValidateTime);
        var persistUntil = ConsoleHelper.GetDateTimeOffset();

        var txt = await work.AcmeClient.MakeDnsPersist01Text(domain, wildcard, persistUntil);
        ConsoleHelper.WriteLine();
        ConsoleHelper.WriteLine(Languages.LangString.Cur.DnsPersist01_TxtResult);
        ConsoleHelper.WriteLine(txt);
        ConsoleHelper.ReadText();
    }

    public static async Task ApplyCert(ApplyCertWorkStep work, ApplyArgs fullSetting)
    {
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

        if (checkResult.Result == UpdateSateEnum.NotUpdate)
            return;

        await work.LoadAcmeAccountAsync();

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
