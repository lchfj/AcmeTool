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

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using AcmeTool.Record;
using AcmeTool.Setting;
using AcmeTool.Workflow;

namespace AcmeTool;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ApplySetting))]
[JsonSerializable(typeof(OrderLite))]
[JsonSerializable(typeof(EncryptHelper.KeyStore))]
public partial class JsonHelper : JsonSerializerContext
{
    public static JsonHelper Beautiful { get; } = new JsonHelper(GetBaseSeting());

    private static JsonSerializerOptions GetBaseSeting()
    {
        var _jsonOption = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        _jsonOption.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        _jsonOption.WriteIndented = true;
        _jsonOption.AllowTrailingCommas = true;
        _jsonOption.ReadCommentHandling = JsonCommentHandling.Skip;

        return _jsonOption;
    }

    internal static T? Deserialize<T>(string json)
    {
        var typeInfo = GetTypeInfo<T>();
        return JsonSerializer.Deserialize(json, typeInfo);
    }
    internal static ValueTask<T?> Deserialize<T>(Stream stream)
    {
        var typeInfo = GetTypeInfo<T>();
        return JsonSerializer.DeserializeAsync(stream, typeInfo);
    }

    internal static string Serialize<T>(T obj)
    {
        var typeInfo = GetTypeInfo<T>();
        return JsonSerializer.Serialize(obj, typeInfo);
    }

    internal static async Task Serialize<T>(Stream stream, T store)
    {
        var typeInfo = GetTypeInfo<T>();
        await JsonSerializer.SerializeAsync(stream, store, typeInfo);
    }

    private static JsonTypeInfo<T> GetTypeInfo<T>()
    {
        var type = typeof(T);

        return (JsonTypeInfo<T>)Beautiful.Options.GetTypeInfo(type);
    }
}