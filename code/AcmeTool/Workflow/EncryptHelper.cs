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

using System.Buffers;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using AcmeLib.Key;
using AcmeTool.Languages;
using AcmeTool.Setting;

namespace AcmeTool.Workflow;

public static class EncryptHelper
{
    public static async Task DecryptKey(ApplyArgs args)
    {
        if (!string.IsNullOrWhiteSpace(args.FilePath))
        {
            if (File.Exists(args.FilePath))
            {
                try
                {
                    var key = await LoadKeyFromFileAsync(args.FilePath);
                    if (key is not null)
                    {
                        Console.WriteLine(LangString.Cur.Main_Dk_Success);
                        Console.WriteLine();
                        Console.WriteLine(key.PrivateKeyPem);
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine(LangString.Cur.Main_Dk_LoadFail);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(LangString.Cur.Main_Dk_ErrorLoad + "：\r\n" + ex.ToString());
                }

            }
            else
            {
                Console.WriteLine(LangString.Cur.Main_Dk_NoFile);
            }
        }
        else
        {
            Console.WriteLine(LangString.Cur.Main_Dk_ArgNone);
        }

        Console.ReadLine();
    }
    public static async Task StoreKeyToFileAsync(IKey key, string path)
    {
        var thumbprint = key.GetKeyThumbprint();
        var encryptPem = EncryptString(thumbprint, key.PrivateKeyPem);

        var store = new KeyStore() { Pem = encryptPem, Thumbprint = thumbprint };

        using var stream = File.OpenWrite(path);

        stream.SetLength(0);
        await JsonHelper.Serialize(stream, store);
    }

    public static async Task<IKey?> LoadKeyFromFileAsync(string path)
    {
        if (!File.Exists(path))
            return null;

        KeyStore? store;
        using (var stream = File.OpenRead(path))
            store = await JsonHelper.Deserialize<KeyStore>(stream);

        if (store is null)
            return null;

        var pem = DecryptString(store.Thumbprint, store.Pem);
        if (string.IsNullOrWhiteSpace(pem))
            return null;

        return KeyManager.LoadKeyFromPem(pem);
    }


    public static string EncryptString(string key, string value)
    {
        var plantext = Encoding.UTF8.GetBytes(value);

        using var aes = CreateAesWithKey(key);

        Span<byte> iv = stackalloc byte[aes.BlockSize / 8];

        using var mp = MemoryPool<byte>.Shared.Rent(aes.GetCiphertextLengthCbc(plantext.Length));
        var ciphertext = mp.Memory.Span;

        aes.TryEncryptCbc(plantext, iv, ciphertext, out var ciphertextLength);

        return Base64Url.EncodeToString(ciphertext.Slice(0, ciphertextLength));
    }

    public static string? DecryptString(string key, string value)
    {
        var ciphertext = Base64Url.DecodeFromChars(value);

        using var aes = CreateAesWithKey(key);

        Span<byte> iv = stackalloc byte[aes.BlockSize / 8];

        using var mp = MemoryPool<byte>.Shared.Rent(ciphertext.Length);
        var plantext = mp.Memory.Span;

        try
        {
            if (!aes.TryDecryptCbc(ciphertext, iv, plantext, out var plantextLength))
            {
                return null;
            }

            return Encoding.UTF8.GetString(plantext.Slice(0, plantextLength));
        }
        catch
        {
            return null;
        }
    }

    private static Aes CreateAesWithKey(string key)
    {
        Span<byte> span = stackalloc byte[128 / 8];
        ReadOnlySpan<byte> salt = [201, 115, 112, 154, 43, 221, 72, 64];

        Rfc2898DeriveBytes.Pbkdf2(key, salt, span, 17, HashAlgorithmName.SHA256);

        var aes = Aes.Create();
        aes.SetKey(span);
        return aes;
    }

    public class KeyStore
    {
        public required string Thumbprint { get; init; }
        public required string Pem { get; init; }
    }
}
