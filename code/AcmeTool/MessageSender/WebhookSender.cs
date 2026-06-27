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
            ConsoleSender.SendInfomation("Webhook 配置无效，请检查");
            return;
        }

        await SendMessageCoreAsync(contentBuilder);

        ConsoleSender.SendInfomation(string.Empty, string.Empty, "Webhook 消息已发送，请查收");

        static string contentBuilder()
        {
            return "这是一条来自AcmeTool的测试消息";
        }
    }


    public Task SendSuccessAsync(string message)
    {
        return SendMessageCoreAsync(contentBuilder);

        string contentBuilder()
        {

            var hostnameList = ApplySetting.HostList.Select(i => i.Value).ToList();

            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("[✔️]AcmeTool 证书更新成功通知[✔️]");


            messageBuilder.Append("可使用者为【").AppendJoin("】、【", hostnameList).Append("】的证书已更新成功。").AppendLine();

            messageBuilder.AppendLine();
            messageBuilder.AppendLine("详细信息：");
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
            var hostnameList = ApplySetting.HostList.Select(i => i.Value).ToList();

            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("[❌]AcmeTool 证书更新失败通知[❌]");

            messageBuilder.AppendLine();
            messageBuilder.Append("可使用者为【").AppendJoin("】、【", hostnameList).Append("】的证书更新失败。").AppendLine();

            messageBuilder.AppendLine();
            messageBuilder.AppendLine("详细信息：");
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
            ConsoleSender.SendError("发送Webhook失败", ex);
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
