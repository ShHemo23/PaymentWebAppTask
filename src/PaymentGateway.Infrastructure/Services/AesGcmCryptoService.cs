using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using PaymentGateway.Application.Interfaces;

namespace PaymentGateway.Infrastructure.Services;

public sealed class AesGcmCryptoService : ICryptoService
{
    private readonly byte[] _key;

    public AesGcmCryptoService(IConfiguration configuration)
    {
        var keyBase64 = configuration["EncryptionSettings:Key"];
        if (string.IsNullOrWhiteSpace(keyBase64))
        {
            throw new InvalidOperationException("Encryption key not configured.");
        }
        _key = Convert.FromBase64String(keyBase64);
        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption key must be 256 bits (32 bytes).");
    }

    public byte[] Encrypt(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> nonce)
    {
        Span<byte> tag = stackalloc byte[16];
        var cipher = new byte[plaintext.Length];
        using var aes = new AesGcm(_key, 16);
        aes.Encrypt(nonce, plaintext, cipher, tag);
        return Combine(nonce.ToArray(), tag.ToArray(), cipher);
    }

    public byte[] Decrypt(ReadOnlySpan<byte> ciphertextWithMeta, ReadOnlySpan<byte> _)
    {
        // stored format = nonce(12) | tag(16) | cipher
        var nonce = ciphertextWithMeta.Slice(0, 12);
        var tag = ciphertextWithMeta.Slice(12, 16);
        var cipher = ciphertextWithMeta.Slice(28);
        var plaintext = new byte[cipher.Length];
        using var aes = new AesGcm(_key, 16);
        aes.Decrypt(nonce, cipher, tag, plaintext);
        return plaintext;
    }

    private static byte[] Combine(params byte[][] arrays)
    {
        var len = arrays.Sum(a => a.Length);
        var result = new byte[len];
        int offset = 0;
        foreach (var arr in arrays)
        {
            Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
            offset += arr.Length;
        }
        return result;
    }
} 