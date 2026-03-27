using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Encryption
{
    // Interface for encryption og decryption af følsomme data
    public interface IEncryptionService
    {
        // Encrypter plaintext til en base64-encoded encrypted string
        string Encrypt(string plainText);

        // Decrypter en encrypted base64 string tilbage til plaintext
        string Decrypt(string encryptedText);
    }
}
