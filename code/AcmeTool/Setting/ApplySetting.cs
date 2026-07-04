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
using System.Globalization;
using System.Reflection;
using System.Text;
using AcmeTool.Languages;

namespace AcmeTool.Setting;

public class ApplySetting
{
    public static readonly string TemplateSetting = GetTemplateSettingString();
    private static string GetTemplateSettingString()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var name = CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "zh" ? "AcmeTool.Setting.SettingTemplate.json" : "AcmeTool.Setting.SettingTemplateEn.json";

        using var baseStream = assembly.GetManifestResourceStream(name);
        if (baseStream is null)
            return string.Empty;

        using var reader = new StreamReader(baseStream, Encoding.UTF8, true);
        return reader.ReadToEnd();
    }


    public required Uri AcmeDirectory { get; init; }
    public required ImmutableList<ApplyHostInfo> HostList { get; init; }
    public required string CertPath { get; init; }
    public required string CertKeyPath { get; init; }


    public string? Profile { get; init; }
    public string? ChallengeFileDirectory { get; init; }
    public string? KeyType { get; init; }
    public int? RemainDayForUpdate { get; init; }
    public string? ExpectedIssuer { get; init; }
    public string? Subject { get; init; }
    public MessageSetting? ResultMessage { get; init; }


    public static async Task<ApplySetting?> LoadSetting(string settingFile)
    {
        settingFile = Path.GetFullPath(settingFile ?? "setting.json");

        Console.Write(LangString.Cur.ApplySettting_Path);
        Console.WriteLine(settingFile);
        Console.WriteLine();
        Console.WriteLine();



        if (File.Exists(settingFile))
        {
            var json = await File.ReadAllTextAsync(settingFile);
            if (!string.IsNullOrWhiteSpace(json))
            {
                if (json != TemplateSetting)
                {
                    var setting = JsonHelper.Deserialize<ApplySetting>(json);
                    return setting;
                }
            }
            else
            {
                await File.WriteAllTextAsync(settingFile, TemplateSetting);
            }
        }
        else
        {
            await File.WriteAllTextAsync(settingFile, TemplateSetting);
        }


        Console.WriteLine(LangString.Cur.ApplySettting_Created);
        return null;
    }
}
