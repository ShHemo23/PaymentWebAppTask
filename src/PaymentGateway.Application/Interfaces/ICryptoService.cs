namespace PaymentGateway.Application.Interfaces;

public interface ICryptoService
{
    byte[] Encrypt(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> nonce);
    byte[] Decrypt(ReadOnlySpan<byte> ciphertext, ReadOnlySpan<byte> nonce);
} 