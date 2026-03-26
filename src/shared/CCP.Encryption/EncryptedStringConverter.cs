using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChatApp.Encryption
{
    /// <summary>
    /// EF Core Value Converter der transparent encrypter/decrypter strings
    /// Bruges ved property configuration i DbContext
    /// </summary>
    public class EncryptedStringConverter : ValueConverter<string, string>
    {
        // Constructor der tager en encryption service instance
        public EncryptedStringConverter(IEncryptionService encryptionService)
            : base(
                // Konverter fra model til database (encrypt)
                v => encryptionService.Encrypt(v),

                // Konverter fra database til model (decrypt)
                v => encryptionService.Decrypt(v)
            )
        {
        }
    }
}
