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
using AcmeTool.Setting;
using AcmeTool.Workflow;

namespace AcmeTool;

internal static class Program
{
    private static Task Main(string[] args)
    {
        return MainAsync(ApplyArgs.Create(args));
    }

    private static async Task MainAsync(ApplyArgs args)
    {
        Console.WriteLine(@"AcmeTool is a console program for update cert by acme protocol.

Copyright (C) 2026 lchfj.cn

This program is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.


The project source repository at :
https://github.com/lchfj/AcmeTool

The program is based on
AcmeLib      : https://github.com/lchfj/AcmeLib
MailKitLite  : https://github.com/jstedfast/MailKit



");


        Console.Write(LangString.Cur.Main_WorkDirectory);
        Console.WriteLine(Path.GetFullPath("."));

        if (args.IsDecryptFile)
        {
            await EncryptHelper.DecryptKey(args);
            return;
        }


        var setting = await ApplySetting.LoadSetting(args.FilePath);
        if (setting is null)
            return;


        var logger = new LogRecorder(setting);
        if (args.TestMessageSender)
        {
            await logger.TestMessageSendAsync();
            return;
        }


        if (setting.AcmeDirectory is null || !setting.AcmeDirectory.IsAbsoluteUri)
        {
            await logger.LogError(LangString.Cur.ApplyCert_NoneAcme);
            await ConsoleHelper.DelayExitAsync();
            return;
        }

        var work = new ApplyCertWorkStep(setting, logger);
        if (args.IsLoadDnsPersist01Txt)
        {
            await RunWork(ApplyCertWorkController.LoadDnsPersist01Txt, work, args);
        }
        else
        {
            await RunWork(ApplyCertWorkController.ApplyCert, work, args);
        }
    }

    private static async Task RunWork(Func<ApplyCertWorkStep, ApplyArgs, Task> ApplyWork, ApplyCertWorkStep work, ApplyArgs args)
    {
        try
        {
            await ApplyWork(work, args);
        }
        catch (BreakWorkException)
        {
            await ConsoleHelper.DelayExitAsync();
        }
        catch (AcmeErrorException ex)
        {
            await work.Logger.LogError(LangString.Cur.ApplyCert_AcmeResponseException, ex.AcmeError);

            await ConsoleHelper.DelayExitAsync();
        }
        catch (Exception ex)
        {
            await work.Logger.LogError(LangString.Cur.ApplyCert_UnhandelException, ex.ToString());

            await ConsoleHelper.DelayExitAsync();
        }
        finally
        {
            await work.ClearChallengeFileAsync();
        }
    }
}