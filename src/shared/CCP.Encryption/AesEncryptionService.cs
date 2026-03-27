using System.Security.Cryptography;
using System.Text;

namespace ChatApp.Encryption;

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private const int NonceSize = 12; // 96 bits for GCM
    private const int TagSize = 16; // 128 bits for GCM

    //constructor der tager encryption key som base64 string
    public AesEncryptionService(string base64Key)
    {
        _key = Convert.FromBase64String(base64Key);

        //valider at key er 256 bits (32 bytes)
        if(_key.Length != 32)
        {
            throw new ArgumentException("Encryption key must be 256 bits (32 bytes) long.");
        }
    }

    public string Encrypt(string plainText)
    {
        //Håndter null og tomme strings
        if (string.IsNullOrEmpty(plainText))
            return plainText ?? string.Empty;

        //Konvater plainText til bytes
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        //generér en random nonce
        byte[] nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        //Buffer til encrypted data + authentication tag
        byte[] cipherBytes = new byte[plainBytes.Length];
        byte[] tag = new byte[TagSize];

        // Encrypt med AES-GCM
        using (var aesGcm = new AesGcm(_key, TagSize))
        {
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        // Kombiner nonce + tag + ciphertext til én byte array
        byte[] result = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, result, NonceSize + TagSize, cipherBytes.Length);

        // Return som base64 string
        return Convert.ToBase64String(result);
    }

    public string Decrypt(string encryptedText)
    {
        // Håndter null og tomme strings
        if (string.IsNullOrEmpty(encryptedText))
            return encryptedText ?? string.Empty;

        try
        {
            // Konverter fra base64 tilbage til bytes
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            // Valider minimum længde
            if (encryptedBytes.Length < NonceSize + TagSize)
                throw new CryptographicException("Encrypted data er ugyldig");

            // Udtræk nonce, tag og ciphertext
            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] cipherBytes = new byte[encryptedBytes.Length - NonceSize - TagSize];

            Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(encryptedBytes, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(encryptedBytes, NonceSize + TagSize, cipherBytes, 0, cipherBytes.Length);

            // Buffer til decrypted data
            byte[] plainBytes = new byte[cipherBytes.Length];

            // Decrypt med AES-GCM
            using (var aesGcm = new AesGcm(_key, TagSize))
            {
                aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
            }

            // Konverter bytes tilbage til string
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception ex)
        {
            // Log fejl i produktion - her smider vi videre
            throw new CryptographicException("Decryption fejlede - muligvis forkert key eller korrupt data", ex);
        }
    }

}
