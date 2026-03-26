using System.Security.Cryptography;

namespace ChatApp.Encryption.Tests;

public class AesEncryptionServiceTests
{
    // Helper metode til at lave en valid test key
    private string CreateValidKey()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    [Fact]
    public void Test1_BasicEncryptionWorks()
    {
        // Arrange
        var key = CreateValidKey();
        var service = new AesEncryptionService(CreateValidKey());
        var original = "Hello World";

        // Act
        var encrypted = service.Encrypt(original);
        var decrypted = service.Decrypt(encrypted);

        // Assert
        Assert.Equal(original, decrypted);
        Assert.NotEqual(original, encrypted);
    }

    [Fact]
    public void Test2_SameTextTwice_IsDifferent()
    {
        // Arrange
        var service = new AesEncryptionService(CreateValidKey());
        var text = "Secret";

        // Act
        var encrypted1 = service.Encrypt(text);
        var encrypted2 = service.Encrypt(text);

        // Assert - Hver encryption skal være unik!
        Assert.NotEqual(encrypted1, encrypted2);
        Assert.Equal(text, service.Decrypt(encrypted1));
        Assert.Equal(text, service.Decrypt(encrypted2));
    }

    [Fact]
    public void Test3_WrongKey_CannotDecrypt()
    {
        // Arrange
        var service1 = new AesEncryptionService(CreateValidKey());
        var service2 = new AesEncryptionService(CreateValidKey()); // Forskellig key

        var encrypted = service1.Encrypt("Secret");

        // Act & Assert - Skal throw exception
        Assert.Throws<CryptographicException>(() => service2.Decrypt(encrypted));
    }

    [Fact]
    public void Test4_CorruptedData_CannotDecrypt()
    {
        // Arrange
        var service = new AesEncryptionService(CreateValidKey());
        var encrypted = service.Encrypt("Hello");

        // Ødelæg data
        var corrupted = "XXX" + encrypted.Substring(3);

        // Act & Assert
        Assert.Throws<CryptographicException>(() => service.Decrypt(corrupted));
    }

    [Fact]
    public void Test5_SpecialCharacters_Work()
    {
        // Arrange
        var service = new AesEncryptionService(CreateValidKey());
        var text = "Æble Ørn Åge test@example.com 中文 🚀";

        // Act
        var encrypted = service.Encrypt(text);
        var decrypted = service.Decrypt(encrypted);

        // Assert
        Assert.Equal(text, decrypted);
    }
}
