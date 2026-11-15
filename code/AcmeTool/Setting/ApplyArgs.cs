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

namespace AcmeTool.Setting;

public class ApplyArgs
{
    public required bool IsForceUpdate { get; init; }
    public required bool TestMessageSender { get; init; }
    public required bool IsLoadDnsPersist01Txt { get; init; }
    public required bool IsCheckHost { get; init; }
    public required string FilePath { get; init; }
    public required bool IsDecryptFile { get; init; }

    public static ApplyArgs Create(string[] args)
    {
        var settingFile = args?.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t) && t[0] != '-') ?? "setting.json";

        settingFile = new FileInfo(settingFile).FullName;


        return new ApplyArgs()
        {
            TestMessageSender = args?.Contains("-tm") ?? false,
            IsLoadDnsPersist01Txt = args?.Contains("-ld") ?? false,
            IsForceUpdate = args?.Contains("-fu") ?? false,
            IsCheckHost = args?.Contains("-ch") ?? false,
            IsDecryptFile = args?.Contains("-de") ?? false,

            FilePath = settingFile,
        };
    }
}