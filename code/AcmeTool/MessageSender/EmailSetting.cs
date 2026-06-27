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
using MimeKit;

namespace AcmeTool.MessageSender;

public class EmailSetting
{
    public string? SmtpServer { get; init; }
    public int Port { get; init; }
    public bool UseSsl { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }

    public string? FromAddress { get; init; }
    public string? ToAddress { get; init; }

    [MemberNotNullWhen(true, nameof(SmtpServer), nameof(Username), nameof(Password), nameof(FromAddress), nameof(ToAddress))]
    public bool IsOK()
    {
        if (string.IsNullOrWhiteSpace(SmtpServer))
            return false;

        if (Port <= 0)
            return false;

        if (string.IsNullOrWhiteSpace(Username))
            return false;
        if (string.IsNullOrWhiteSpace(Password))
            return false;


        if (string.IsNullOrWhiteSpace(FromAddress) || !MailboxAddress.TryParse(FromAddress, out var _))
            return false;
        if (string.IsNullOrWhiteSpace(ToAddress) || !MailboxAddress.TryParse(ToAddress, out var _))
            return false;

        return true;
    }
}