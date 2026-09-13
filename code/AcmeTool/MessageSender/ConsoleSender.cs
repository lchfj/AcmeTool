

using AcmeTool.Languages;

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

namespace AcmeTool.MessageSender;

public static class ConsoleSender
{
    public static async Task DelayExitAsync()
    {
        Console.WriteLine(LangString.Cur.Console_ExitWait);

        var startTick = DateTime.UtcNow.Ticks;

        while (true)
        {
            await Task.Delay(100);
            if (DateTime.UtcNow.Ticks > startTick + TimeSpan.TicksPerMinute * 10)
                break;

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
            }
        }
    }

    public static void SendError(string message, Exception ex)
    {
        SendError([message]);

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(ex.ToString());
    }

    public static void SendError(params IEnumerable<string> message)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        SendInfomation(message);

        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static void SendInfomation(params IEnumerable<string> message)
    {
        foreach (var item in message)
        {
            Console.WriteLine(item);
        }
    }

    public static void SendSuccess(params IEnumerable<string> message)
    {
        Console.ForegroundColor = ConsoleColor.Green;

        SendInfomation(message);

        Console.ForegroundColor = ConsoleColor.Gray;
    }
}