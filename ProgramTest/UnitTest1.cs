using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TheVault.Shared.Encryption;
using Xunit;

public class ProgramTests
{
    // The test to verify that a file can be encrypted and then decrypted successfully.
    [Fact]
    public void EncryptAndDecryptFile_ShouldRestoreOriginalContent()
    {
        // Arrange
        string testContent = "This is a test file for the encryption and decryption methods.";
        string testFileName = "temp_test_file.txt";
        string keyFileName = testFileName + ".keyinfo";

        // This is a stand-in for your program's methods, which should ideally be public for testing.
        var keyGen = new GenerateKeysAndIvs();
        byte[] key = new byte[32];
        byte[] iv = new byte[16];
        keyGen.GenerateKeyBytes(key);
        keyGen.GenerateIVBytes(iv);

        // Ensure the test starts with a clean slate
        if (File.Exists(testFileName)) File.Delete(testFileName);
        if (File.Exists(keyFileName)) File.Delete(keyFileName);

        File.WriteAllText(testFileName, testContent);

        try
        {
            // Act - Encrypt the file and save the key info
            EncryptFileInPlace(testFileName, key, iv);
            SaveKeyInfo(keyFileName, key, iv);

            // Assert that the file is now encrypted
            Assert.True(IsFileEncrypted(testFileName));
            Assert.NotEqual(testContent, File.ReadAllText(testFileName));

            // Act - Decrypt the file
            LoadKeyInfo(keyFileName, out byte[] loadedKey, out byte[] loadedIv);
            DecryptFileInPlace(testFileName, loadedKey, loadedIv);

            // Assert that the file is now decrypted and the content is the same
            string decryptedContent = File.ReadAllText(testFileName);
            Assert.Equal(testContent, decryptedContent);
            Assert.False(IsFileEncrypted(testFileName));
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFileName)) File.Delete(testFileName);
            if (File.Exists(keyFileName)) File.Delete(keyFileName);
        }
    }

    // Test to ensure decryption fails with an incorrect key
    [Fact]
    public void DecryptFile_WithIncorrectKey_ShouldThrowCryptographicException()
    {
        // Arrange
        string testContent = "This is a test file to check for incorrect key decryption.";
        string testFileName = "temp_badkey_test_file.txt";
        string keyFileName = testFileName + ".keyinfo";

        var keyGen = new GenerateKeysAndIvs();
        byte[] correctKey = new byte[32];
        byte[] correctIv = new byte[16];
        keyGen.GenerateKeyBytes(correctKey);
        keyGen.GenerateIVBytes(correctIv);

        byte[] incorrectKey = new byte[32];
        keyGen.GenerateKeyBytes(incorrectKey); // Generate a different key

        if (File.Exists(testFileName)) File.Delete(testFileName);
        if (File.Exists(keyFileName)) File.Delete(keyFileName);

        File.WriteAllText(testFileName, testContent);

        try
        {
            // Encrypt with the correct key
            EncryptFileInPlace(testFileName, correctKey, correctIv);

            // Act & Assert - Attempt to decrypt with the incorrect key and expect an exception
            Assert.Throws<CryptographicException>(() =>
            {
                DecryptFileInPlace(testFileName, incorrectKey, correctIv);
            });
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFileName)) File.Delete(testFileName);
            if (File.Exists(keyFileName)) File.Delete(keyFileName);
        }
    }

    // IMPORTANT: For these unit tests to work, you will need to make the following methods public
    // in your Program.cs file: EncryptFileInPlace, DecryptFileInPlace, SaveKeyInfo, LoadKeyInfo, and IsFileEncrypted.

    private const string ENCRYPTED_HEADER = "[ENCRYPTED]\n";

    public static void EncryptFileInPlace(string filePath, byte[] key, byte[] iv)
    {
        byte[] content = File.ReadAllBytes(filePath);
        byte[] encrypted = Encryption.Encrypt(content, key, iv);

        byte[] header = Encoding.UTF8.GetBytes(ENCRYPTED_HEADER);
        byte[] result = new byte[header.Length + encrypted.Length];

        Buffer.BlockCopy(header, 0, result, 0, header.Length);
        Buffer.BlockCopy(encrypted, 0, result, header.Length, encrypted.Length);

        File.WriteAllBytes(filePath, result);
    }

    public static void DecryptFileInPlace(string filePath, byte[] key, byte[] iv)
    {
        byte[] fullData = File.ReadAllBytes(filePath);
        byte[] headerBytes = Encoding.UTF8.GetBytes(ENCRYPTED_HEADER);

        if (fullData.Length <= headerBytes.Length)
        {
            Console.WriteLine("Encrypted file is too short or corrupted.");
            return;
        }

        byte[] encryptedData = new byte[fullData.Length - headerBytes.Length];
        Buffer.BlockCopy(fullData, headerBytes.Length, encryptedData, 0, encryptedData.Length);

        byte[] decrypted = Encryption.Decrypt(encryptedData, key, iv);
        File.WriteAllBytes(filePath, decrypted);
    }

    public static void SaveKeyInfo(string keyFilePath, byte[] key, byte[] iv)
    {
        string keyBase64 = Convert.ToBase64String(key);
        string ivBase64 = Convert.ToBase64String(iv);
        string content = $"Key={keyBase64}{Environment.NewLine}IV={ivBase64}";
        File.WriteAllText(keyFilePath, content);
    }

    public static void LoadKeyInfo(string keyFilePath, out byte[] key, out byte[] iv)
    {
        string[] lines = File.ReadAllLines(keyFilePath);
        string keyBase64 = lines[0].Substring(lines[0].IndexOf('=') + 1).Trim();
        string ivBase64 = lines[1].Substring(lines[1].IndexOf('=') + 1).Trim();

        key = Convert.FromBase64String(keyBase64);
        iv = Convert.FromBase64String(ivBase64);
    }

    public static bool IsFileEncrypted(string filePath)
    {
        byte[] headerBytes = Encoding.UTF8.GetBytes(ENCRYPTED_HEADER);
        byte[] buffer = new byte[headerBytes.Length];

        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            int bytesRead = fs.Read(buffer, 0, buffer.Length);
            if (bytesRead != headerBytes.Length)
                return false;

            return Encoding.UTF8.GetString(buffer) == ENCRYPTED_HEADER;
        }
    }
}
