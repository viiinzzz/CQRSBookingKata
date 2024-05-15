/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.IO.Compression;
using System.Text;
using SimpleBase;
using System.Security.Cryptography;

namespace VinZ.Common;


public static partial class StringHelper
{
    public static string CreateSecretAes58()
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();

        var key = aes.Key;
        var secret = Base58.Bitcoin.Encode(key);

        return secret;
    }

    public static string EncryptAes58(this string str, string key58, Encoding? encoding = default)
    {
        if (string.IsNullOrEmpty(key58))
        {
            return str;
        }

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = Base58.Bitcoin.Decode(key58);
        //aes.GenerateIV();
        var iv = new byte[16];
        aes.IV = iv;

        var transform = aes.CreateEncryptor();

        using var encrypted = new MemoryStream();
        using var xStream = new CryptoStream(encrypted, transform, CryptoStreamMode.Write);

        var bytes = (encoding ?? Encoding.UTF8).GetBytes(str);
        xStream.Write(bytes);
        xStream.Dispose();
        
        var ret = Base58.Bitcoin.Encode(encrypted.ToArray());
        return ret;
    }

    public static string DecryptAes58(this string str, string key58, Encoding? encoding = default)
    {
        if (string.IsNullOrEmpty(key58))
        {
            return str;
        }

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = Base58.Bitcoin.Decode(key58);
        var iv = new byte[16];
        aes.IV = iv;
        
        var transform = aes.CreateDecryptor();

        using var encrypted = new MemoryStream(Base58.Bitcoin.Decode(str));
        using var xStream = new CryptoStream(encrypted, transform, CryptoStreamMode.Read);
        using var xReader = new StreamReader(xStream, encoding ?? Encoding.UTF8);

        var ret = xReader.ReadToEnd();
        return ret;
    }


    public static byte[] Compress(this byte[] bytesIn)
    {
        using var bytesOutStream = new MemoryStream();
        using var zip = new GZipStream(bytesOutStream, CompressionLevel.Optimal);

        zip.Write(bytesIn, 0, bytesIn.Length);
        zip.Flush();

        return bytesOutStream.ToArray();
    }

    public static byte[] Decompress(this byte[] bytesIn)
    {
        using var bytesInStream = new MemoryStream(bytesIn);
        using var bytesOutStream = new MemoryStream();
        using var unzip = new GZipStream(bytesInStream, CompressionMode.Decompress);

        unzip.CopyTo(bytesOutStream);
        unzip.Flush();

        return bytesOutStream.ToArray();
    }

    public static string Compress58(this string str, bool asIsOnFail = false, Encoding? encoding = default)
    {
        try
        {
            return Base58.Bitcoin.Encode((encoding ?? Encoding.UTF8).GetBytes(str).Compress());
        }
        catch
        {
            if (asIsOnFail)
            {
                return str;
            }

            throw new Exception("Compress cannot be applied");
        }
    }

    public static string Decompress58(this string str, bool asIsOnFail = false, Encoding? encoding = default)
    {
        try
        {
            return (encoding ?? Encoding.UTF8).GetString(Base58.Bitcoin.Decode(str).Decompress());
        }
        catch
        {
            if (asIsOnFail)
            {
                return str;
            }

            throw new Exception("Decompress cannot be applied");
        }
    }
}