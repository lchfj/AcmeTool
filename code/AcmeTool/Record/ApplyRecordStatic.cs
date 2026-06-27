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

using AcmeLib;
using AcmeLib.Key;
using AcmeTool.Setting;

namespace AcmeTool.Record;

public partial class ApplyRecord
{
    private static readonly string OrderFloderName = "orders";
    private static readonly string ApplyIdFormat = "D16";

    private static readonly string OrderInfoFileName = "order.json";
    private static readonly string BackupCertFileName = "cert.crt";
    private static readonly string BackupCertKeyFileName = "cert.key";
    private static readonly string LoggerFileName = "cert.log";


    public static ApplyRecord CreateNew(Uri acmeDirectoryUrl, IKey acmeAccount, IEnumerable<ApplyHostInfo> hostList)
    {
        var recordList = GetRecordList(acmeDirectoryUrl, acmeAccount, hostList);
        foreach (var item in recordList)
        {
            // 查看订单信息是否存在，如果存在，就不能作为新的使用
            var infoPath = Path.Combine(item, OrderInfoFileName);
            if (File.Exists(infoPath))
                break;

            return new ApplyRecord()
            {
                OrderInfoFile = infoPath,
                CertBackupPath = Path.Combine(item, BackupCertFileName),
                CertKeyBackupPath = Path.Combine(item, BackupCertKeyFileName),
                LoggerFile = Path.Combine(item, LoggerFileName),
            };
        }



        var directory = GetRecordDirectory(acmeDirectoryUrl, acmeAccount, hostList);
        var applyId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(ApplyIdFormat);

        directory = Path.Combine(directory, applyId);
        Directory.CreateDirectory(directory);

        return new ApplyRecord()
        {
            OrderInfoFile = Path.Combine(directory, OrderInfoFileName),
            CertBackupPath = Path.Combine(directory, BackupCertFileName),
            CertKeyBackupPath = Path.Combine(directory, BackupCertKeyFileName),
            LoggerFile = Path.Combine(directory, LoggerFileName),
        };
    }


    public static ApplyRecord? LoadLast(Uri acmeDirectoryUrl, IKey acmeAccount, IEnumerable<ApplyHostInfo> hostList)
    {
        var recordList = GetRecordList(acmeDirectoryUrl, acmeAccount, hostList);
        foreach (var item in recordList)
        {
            var infoPath = Path.Combine(item, OrderInfoFileName);
            if (!File.Exists(infoPath))
                continue;

            return new ApplyRecord()
            {
                OrderInfoFile = infoPath,
                CertBackupPath = Path.Combine(item, BackupCertFileName),
                CertKeyBackupPath = Path.Combine(item, BackupCertKeyFileName),
                LoggerFile = Path.Combine(item, LoggerFileName),
            };
        }

        return null;
    }

    private static IEnumerable<string> GetRecordList(Uri acmeDirectoryUrl, IKey acmeAccount, IEnumerable<ApplyHostInfo> hostList)
    {
        var directory = GetRecordDirectory(acmeDirectoryUrl, acmeAccount, hostList);
        if (!Directory.Exists(directory))
            yield break;

        var subDirectory = Directory.GetDirectories(directory);
        subDirectory = subDirectory.OrderDescending().ToArray();

        foreach (var item in subDirectory)
        {
            var directoryName = new DirectoryInfo(item).Name;
            if (long.TryParse(directoryName, out var _))
                yield return item;
        }
    }

    private static string GetRecordDirectory(Uri acmeDirectoryUrl, IKey acmeAccount, IEnumerable<ApplyHostInfo> hostList)
    {
        var traceId = CreateTraceId(acmeDirectoryUrl, acmeAccount, hostList);
        return Path.GetFullPath(Path.Combine(OrderFloderName, traceId));
    }


    private static string CreateTraceId(Uri acmeDirectoryUrl, IKey acmeAccount, IEnumerable<ApplyHostInfo> hostList)
    {
        var list = new List<string>(8);

        list.Add(acmeDirectoryUrl.AbsoluteUri);
        list.Add(acmeAccount.GetKeyThumbprint());

        static string changer(ApplyHostInfo h) => $"{h.Type}|{h.Value}";
        list.AddRange(hostList.Select(changer).Order());

        var key = string.Join('\n', list);

        return Sha256Helper.HashDataToBase64Url(key);
    }
}