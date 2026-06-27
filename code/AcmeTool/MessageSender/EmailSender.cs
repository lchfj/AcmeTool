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

using System.Text;
using AcmeTool.Setting;
using MailKit.Net.Smtp;
using MimeKit;

namespace AcmeTool.MessageSender;

public class EmailSender
{
    private readonly ApplySetting ApplySetting;
    public EmailSender(ApplySetting applySetting)
    {
        ApplySetting = applySetting;
    }

    public async Task SendTestMessageAsync()
    {
        var setting = ApplySetting.ResultMessage?.Email;
        if (!(setting?.IsOK() ?? false))
        {
            ConsoleSender.SendInfomation("Email 配置无效，请检查");
            return;
        }

        await SendMessageCoreAsync("AcmeTool 消息测试", contentBuilder, null);

        ConsoleSender.SendInfomation(string.Empty, string.Empty, "Email 消息已发送，请查收");

        static string contentBuilder()
        {
            return "这是一条来自AcmeTool的测试消息";
        }
    }

    public Task SendSuccess(string message)
    {
        var title = "AcmeTool 证书更新成功通知";
        return SendMessageCoreAsync(title, contentBuilder, null);

        string contentBuilder()
        {

            var hostnameList = ApplySetting.HostList.Select(i => i.Value).ToList();

            var messageBuilder = new StringBuilder();

            messageBuilder.Append("可使用者为【").AppendJoin("】、【", hostnameList).Append("】的证书已更新成功，以下为详细信息。").AppendLine();
            messageBuilder.AppendLine();

            messageBuilder.AppendLine(message);
            return messageBuilder.ToString();
        }
    }

    public Task SendError(IEnumerable<string> message, string? attrFilePath)
    {
        var title = "AcmeTool 证书更新失败通知";
        return SendMessageCoreAsync(title, contentBuilder, attrFilePath);

        string contentBuilder()
        {
            var hostnameList = ApplySetting.HostList.Select(i => i.Value).ToList();

            var messageBuilder = new StringBuilder();

            messageBuilder.Append("可使用者为【").AppendJoin("】、【", hostnameList).Append("】的证书更新失败，以下为详细信息。").AppendLine();
            messageBuilder.AppendLine();

            foreach (var item in message)
                messageBuilder.AppendLine(item);
            return messageBuilder.ToString();
        }
    }



    private async Task SendMessageCoreAsync(string title, Func<string> bodyBuilder, string? attrFilePath)
    {
        if (!(ApplySetting.ResultMessage?.Email?.IsOK() ?? false))
        {
            return;
        }

        try
        {
            var body = bodyBuilder();
            await SendMessageCoreAsync(ApplySetting.ResultMessage.Email, title, body, attrFilePath);
        }
        catch (Exception ex)
        {
            ConsoleSender.SendError("", ex);
        }
    }


    private static async Task SendMessageCoreAsync(EmailSetting email, string title, string bodytext, string? attaPath)
    {
        var fromAddress = MailboxAddress.Parse(email.FromAddress!);
        fromAddress.Name = "AcmeTool";

        var toAddress = MailboxAddress.Parse(email.ToAddress!);

        var body = new BodyBuilder();

        body.TextBody = bodytext;
        body.BodyEncoding = Encoding.UTF8;

        if (!string.IsNullOrWhiteSpace(attaPath) && File.Exists(attaPath))
        {
            var data = await File.ReadAllBytesAsync(attaPath);
            body.Attachments.Add("log.txt", data);
        }

        var mimemessage = new MimeMessage();
        mimemessage.From.Add(fromAddress);
        mimemessage.To.Add(toAddress);
        mimemessage.Subject = title;
        mimemessage.Body = body.ToMessageBody();



        using var client = new SmtpClient();
        await client.ConnectAsync(email.SmtpServer!, email.Port, email.UseSsl);
        await client.AuthenticateAsync(email.Username!, email.Password!);

        await client.SendAsync(mimemessage);
    }

}
