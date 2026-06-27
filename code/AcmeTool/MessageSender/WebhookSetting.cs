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

using System.Diagnostics.CodeAnalysis;

namespace AcmeTool.MessageSender;

public class WebhookSetting
{
    public Uri? TargetUrl { get; init; }
    public string? MessageJson { get; init; }


    [MemberNotNullWhen(true, nameof(TargetUrl), nameof(MessageJson))]
    public bool IsOK()
    {
        if (TargetUrl is null)
            return false;

        if (string.IsNullOrWhiteSpace(MessageJson))
            return false;

        var js = MessageJson.Trim();
        return js.Length > 2 && js[0] == '{' && js[^1] == '}';
    }
}
