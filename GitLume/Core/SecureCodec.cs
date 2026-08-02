using System;
using System.Text;

namespace GitLume.Core;

/// <summary>
/// 密码简单可逆加密（混淆级，非强加密）。
///
/// 加密算法：
///   1. 取固定密钥字符串，转 UTF-8 字节数组 key；
///   2. 明文转 UTF-8 字节数组 data；
///   3. 逐字节异或：data[i] = data[i] ^ key[i % key.Length]；
///   4. 结果 Base64 编码后存入配置。
///
/// 解密算法：
///   1. Base64 解码得到 data；
///   2. 用同一密钥逐字节异或还原；
///   3. UTF-8 解码得到明文。
///
/// 说明：这只是防"记事本直接打开看到明文"的混淆。真正需要高强度保护时，
/// 请改用 Windows DPAPI（System.Security.Cryptography.ProtectedData），
/// 但那只能在本机解密，不跨机器。
/// </summary>
public static class SecureCodec
{
    /// <summary>固定密钥（代码内置，和本类算法配套）。</summary>
    private const string SecretKey = "GitLume@2026#Enc#SimpleKey";

    public static string Encrypt(string plain)
    {
        if (string.IsNullOrEmpty(plain)) return "";
        var key = Encoding.UTF8.GetBytes(SecretKey);
        var data = Encoding.UTF8.GetBytes(plain);
        for (int i = 0; i < data.Length; i++)
            data[i] ^= key[i % key.Length];
        return Convert.ToBase64String(data);
    }

    public static string Decrypt(string encrypted)
    {
        if (string.IsNullOrEmpty(encrypted)) return "";
        try
        {
            var key = Encoding.UTF8.GetBytes(SecretKey);
            var data = Convert.FromBase64String(encrypted);
            for (int i = 0; i < data.Length; i++)
                data[i] ^= key[i % key.Length];
            return Encoding.UTF8.GetString(data);
        }
        catch
        {
            return ""; // 解密失败视为无密码
        }
    }
}
