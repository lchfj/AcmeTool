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
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AcmeTool.Languages;
using AcmeTool.Setting;

namespace AcmeTool.MessageSender;

public class WebhookSender
{
    private readonly ApplySetting ApplySetting;

    public WebhookSender(ApplySetting applySetting)
    {
        ApplySetting = applySetting;
    }

    public async Task SendTestMessageAsync()
    {
        var setting = ApplySetting.ResultMessage?.Webhook;
        if (!(setting?.IsOK() ?? false))
        {
            ConsoleSender.SendInfomation(LangString.Cur.Webhook_Test_ConfigInvalid);
            return;
        }

        await SendMessageCoreAsync(contentBuilder);

        ConsoleSender.SendInfomation(string.Empty, string.Empty, LangString.Cur.Message_Test_WebhookHint);

        static string contentBuilder()
        {
            return LangString.Cur.Message_Test_Content;
        }
    }


    public Task SendSuccessAsync(string message)
    {
        return SendMessageCoreAsync(contentBuilder);

        string contentBuilder()
        {

            var hostnameList = ApplySetting.HostList.Select(i => i.Value)
                .Select(i => "【" + i + "】")
                .ToList();

            var messageBuilder = new StringBuilder();
            messageBuilder.Append("[✔️]").Append(LangString.Cur.Message_CertUpdate_SuccessTitle).Append("[✔️]").AppendLine();

            messageBuilder.AppendLine();

            messageBuilder.AppendFormat(LangString.Cur.Message_CertUpdate_SuccessMessage, string.Join(string.Empty, hostnameList)).AppendLine();

            messageBuilder.AppendLine();
            messageBuilder.AppendLine(LangString.Cur.Message_CertUpdate_DetailTitle);
            messageBuilder.AppendLine();

            messageBuilder.AppendLine(message);

            return messageBuilder.ToString();
        }
    }

    public Task SendErrorAsync(IEnumerable<string> message)
    {
        return SendMessageCoreAsync(contentBuilder);

        string contentBuilder()
        {
            var hostnameList = ApplySetting.HostList.Select(i => i.Value)
                .Select(i => "【" + i + "】")
                .ToList();

            var messageBuilder = new StringBuilder();


            messageBuilder.Append("[❌]").Append(LangString.Cur.Message_CertUpdate_FailTitle).Append("[❌]").AppendLine();

            messageBuilder.AppendLine();
            messageBuilder.AppendFormat(LangString.Cur.Message_CertUpdate_FailMassage, string.Join(string.Empty, hostnameList)).AppendLine();

            messageBuilder.AppendLine();
            messageBuilder.AppendLine(LangString.Cur.Message_CertUpdate_DetailTitle);
            messageBuilder.AppendLine();

            foreach (var item in message)
                messageBuilder.AppendLine(item);

            return messageBuilder.ToString();
        }


    }


    private async Task SendMessageCoreAsync(Func<string> contentBuilder)
    {
        if (!(ApplySetting.ResultMessage?.Webhook?.IsOK() ?? false))
        {
            return;
        }

        try
        {
            var text = contentBuilder();
            await SendMessageCoreAsync(ApplySetting.ResultMessage.Webhook, text);
        }
        catch (Exception ex)
        {
            ConsoleSender.SendError(LangString.Cur.Message_WebhookSendFailed, ex);
        }
    }


    private static async Task SendMessageCoreAsync(WebhookSetting webhookSetting, string text)
    {
        var jsonObjcject = JsonNode.Parse(webhookSetting.MessageJson!)!;
        jsonObjcject = ChangeMessage(jsonObjcject, text);

        var contextText = jsonObjcject.ToJsonString();
        var content = new StringContent(contextText, Encoding.UTF8, "application/json");


        using var httpclient = new HttpClient() { Timeout = TimeSpan.FromMinutes(1) };
        using var r = await httpclient.PostAsync(webhookSetting.TargetUrl, content);

        r.EnsureSuccessStatusCode();
    }


    [return: NotNullIfNotNull(nameof(node))]
    private static JsonNode? ChangeMessage(JsonNode? node, string remessage)
    {
        if (node is null)
            return null;

        const string original = "[REAL_MESSAGE_PLACEHOLDER]";
        if (node.GetValueKind() == JsonValueKind.String)
        {
            var originalValue = (string?)node.AsValue();
            if (!string.IsNullOrEmpty(originalValue) && originalValue.Contains(original))
            {
                var updatedValue = originalValue.Replace(original, remessage);
                return JsonValue.Create(updatedValue);
            }
            else
            {
                return node;
            }
        }
        else if (node.GetValueKind() == JsonValueKind.Object)
        {
            var obj = node.AsObject();
            for (var i = 0; i < obj.Count; i++)
            {
                var val = obj[i];

                if (val is not null)
                    obj[i] = ChangeMessage(val, remessage);
            }
            return obj;
        }
        else if (node.GetValueKind() == JsonValueKind.Array)
        {
            var array = node.AsArray();
            for (var i = 0; i < array.Count; i++)
            {
                var val = array[i];

                if (val is not null)
                    array[i] = ChangeMessage(val, remessage);
            }
            return array;
        }
        else
        {
            return node;
        }
    }

}
