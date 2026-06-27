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

using AcmeLib.ProblemDetail;
using AcmeTool.MessageSender;
using AcmeTool.Record;
using AcmeTool.Setting;

namespace AcmeTool.Workflow;

public class LogRecorder
{
    private readonly ApplySetting ApplySetting;
    private readonly Stack<string> Scoped;
    private readonly WebhookSender WebhookSender;
    private readonly EmailSender EmailSender;

    private ApplyRecord? OrderRecord = null;

    public LogRecorder(ApplySetting setting)
    {
        ApplySetting = setting;
        Scoped = new Stack<string>();
        WebhookSender = new WebhookSender(ApplySetting);
        EmailSender = new EmailSender(ApplySetting);
    }

    public IDisposable CreateScoped(string scoped)
    {
        Scoped.Push(scoped);
        return new ScopedDisposable(Scoped);
    }

    public void SetExtendSetting(ApplyRecord record)
    {
        OrderRecord = record;
    }

    public async Task LogError(string str, ProblemLite? error)
    {
        if (error is null)
        {
            await LogError(str);
            return;
        }

        var list = new List<string>(6);

        list.Add(str);
        list.Add("错误代码：" + error.Type);
        if (!string.IsNullOrWhiteSpace(error.Title))
            list.Add("错误标题：" + error.Title);
        if (!string.IsNullOrWhiteSpace(error.Detail))
            list.Add("错误详情：" + error.Detail);

        await LogError(list);
    }

    public async Task LogError(params IEnumerable<string> strings)
    {
        var scoped = string.Join(string.Empty, Scoped.Reverse().Select(t => "【" + t + "】"));
        if (!string.IsNullOrWhiteSpace(scoped))
            strings = strings.Prepend(scoped);


        var loggerText = CreateLoggerText(strings);

        ConsoleSender.SendError(loggerText);
        await OutPutToOrderRecord(loggerText);

        await WebhookSender.SendErrorAsync(strings);
        await EmailSender.SendError(strings, OrderRecord?.LoggerFile);
    }


    public async Task SendSuccess(string logText)
    {
        var loggerText = CreateLoggerText([logText]);

        ConsoleSender.SendSuccess(loggerText);
        await OutPutToOrderRecord(loggerText);

        await WebhookSender.SendSuccessAsync(logText);
        await EmailSender.SendSuccess(logText);
    }


    public async Task LogInfomation(params IEnumerable<string?> message)
    {
        var scoped = string.Join(string.Empty, Scoped.Reverse().Select(t => "【" + t + "】"));
        if (!string.IsNullOrWhiteSpace(scoped))
            message = message.Prepend(scoped);

        var loggerText = CreateLoggerText(message);

        ConsoleSender.SendInfomation(loggerText);
        await OutPutToOrderRecord(loggerText);
    }

    private static List<string> CreateLoggerText(IEnumerable<string?> strings)
    {
        var list = new List<string>(8);

        list.Add("----****----****----");
        list.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        list.AddRange(strings.Where(s => !string.IsNullOrWhiteSpace(s)).Cast<string>());
        list.Add(string.Empty);
        list.Add(string.Empty);

        return list;
    }

    private async Task OutPutToOrderRecord(IEnumerable<string> message)
    {
        if (OrderRecord is not null)
            await File.AppendAllLinesAsync(OrderRecord.LoggerFile, message);
    }

    public async Task TestMessageSendAsync()
    {
        await WebhookSender.SendTestMessageAsync();
        await EmailSender.SendTestMessageAsync();
    }

    private class ScopedDisposable : IDisposable
    {
        private readonly Stack<string> Scoped;

        public ScopedDisposable(Stack<string> scoped)
        {
            Scoped = scoped;
        }

        public void Dispose()
        {
            Scoped.TryPop(out var r);
        }
    }
}