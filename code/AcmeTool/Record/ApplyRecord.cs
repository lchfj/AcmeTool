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

using AcmeLib.Cert;
using AcmeLib.HttpClient;
using AcmeLib.Order;

namespace AcmeTool.Record;

public partial class ApplyRecord
{
    public required string OrderInfoFile { get; init; }

    public required string LoggerFile { get; set; }

    public required string CertBackupPath { get; set; }

    public required string CertKeyBackupPath { get; set; }


    public async Task<OrderLite?> LoadOrderInfo()
    {
        if (!File.Exists(OrderInfoFile))
            return null;

        using var stream = File.OpenRead(OrderInfoFile);
        return await JsonHelper.Deserialize<OrderLite>(stream);
    }

    public Task<CertLite?> LoadCert()
    {
        return CertLite.LoadFromFile(CertBackupPath);
    }

    public async Task UpdateOrderInfo(AcmeResponse<OrderInfo> order)
    {
        var or = new OrderLite()
        {
            OrderUrl = order.Location!,
            Status = order.Content.Status,
            Expires = order.Content.Expires,
        };
        await UpdateOrderInfo(or);
    }

    public async Task UpdateOrderInfo(OrderLite orderLite)
    {
        var json = JsonHelper.Serialize(orderLite);
        await File.WriteAllTextAsync(OrderInfoFile, json);
    }
}
