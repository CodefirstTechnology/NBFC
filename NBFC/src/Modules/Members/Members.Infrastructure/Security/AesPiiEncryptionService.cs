using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Patsanstha.Modules.Members.Application.Abstractions;
using Patsanstha.Modules.Members.Infrastructure.Options;

namespace Patsanstha.Modules.Members.Infrastructure.Security;

public sealed class AesPiiEncryptionService(IOptions<PiiEncryptionOptions> options) : IPiiEncryptionService
{
    private readonly byte[] _key = DeriveKey(options.Value.Key);

    public string Encrypt(string plainText)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var payload = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, payload, nonce.Length + tag.Length, cipherBytes.Length);

        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherText)
    {
        var payload = Convert.FromBase64String(cipherText);
        var nonce = payload[..12];
        var tag = payload[12..28];
        var cipherBytes = payload[28..];
        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }

    public string HashForLookup(string plainText)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainText));
        return Convert.ToHexString(bytes);
    }

    public string MaskAadhaar(string plainText) =>
        plainText.Length == 12
            ? $"XXXX-XXXX-{plainText[^4..]}"
            : "XXXX-XXXX-XXXX";

    public string MaskPan(string plainText) =>
        plainText.Length == 10
            ? $"XXXXX{plainText[5..9]}X"
            : "XXXXXXXXXX";

    private static byte[] DeriveKey(string keyMaterial)
    {
        if (string.IsNullOrWhiteSpace(keyMaterial))
        {
            throw new InvalidOperationException("PiiEncryption:Key is not configured.");
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(keyMaterial));
    }
}
