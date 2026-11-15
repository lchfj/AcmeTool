
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
namespace AcmeTool;

public static class ConsoleHelper
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

    public static void WriteLine(string? txt = null)
    {
        Console.WriteLine(txt);
    }

    public static string ReadText()
    {
        var line = string.Empty;
        while (string.IsNullOrWhiteSpace(line))
        {
            line = Console.ReadLine();
        }

        return line.Trim();
    }
    public static bool ReadYesOrNo()
    {
        while (true)
        {
            var line = Console.ReadLine()?.Trim();
            if ("y".Equals(line, StringComparison.OrdinalIgnoreCase))
                return true;
            else if ("n".Equals(line, StringComparison.OrdinalIgnoreCase))
                return false;
        }
    }

    public static DateTimeOffset? GetDateTimeOffset()
    {
        while (true)
        {
            var line = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(line))
                return null;

            if (DateTimeOffset.TryParse(line, out var result))
                return result;
        }
    }

}