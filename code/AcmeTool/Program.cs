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

using AcmeTool.Languages;
using AcmeTool.Setting;
using AcmeTool.Workflow;

Task.Run(() => MainAsync(ApplyArgs.Create(args))).ConfigureAwait(false).GetAwaiter().GetResult();

static async Task MainAsync(ApplyArgs args)
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
        await DecryptKey(args);
        return;
    }


    var setting = await ApplySetting.LoadSetting(args.FilePath);
    if (setting is null)
        return;

    if (args.TestMessageSender)
    {
        var logger = new LogRecorder(setting);
        await logger.TestMessageSendAsync();
    }
    else
    {
        await ApplyCertWorkController.MainAsync(args, setting);
    }
}


static async Task DecryptKey(ApplyArgs args)
{
    if (!string.IsNullOrWhiteSpace(args.FilePath))
    {
        if (File.Exists(args.FilePath))
        {
            try
            {
                var key = await EncryptHelper.LoadKeyFromFileAsync(args.FilePath);
                if (key is not null)
                {
                    Console.WriteLine(LangString.Cur.Main_Dk_Success);
                    Console.WriteLine();
                    Console.WriteLine(key.PrivateKeyPem);
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine(LangString.Cur.Main_Dk_LoadFail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(LangString.Cur.Main_Dk_ErrorLoad + "：\r\n" + ex.ToString());
            }

        }
        else
        {
            Console.WriteLine(LangString.Cur.Main_Dk_NoFile);
        }
    }
    else
    {
        Console.WriteLine(LangString.Cur.Main_Dk_ArgNone);
    }

    Console.ReadLine();
}