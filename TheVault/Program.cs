using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using TheVault.Shared.Encryption;
using System.Security.Cryptography; // Needed for CryptographicException

class Program
{
    private const string ENCRYPTED_HEADER = "[ENCRYPTED]\n";

    public static void Main()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n=== The Vault ===");
            Console.WriteLine("1. Encrypt a file");
            Console.WriteLine("2. Decrypt a file");
            Console.WriteLine("3. Create a file");
            Console.WriteLine("4. List encrypted files");
            Console.WriteLine("0. Exit");
            Console.Write("Choose an option: ");

            string input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    Console.Clear();
                    HandleEncryption();
                    break;
                case "2":
                    Console.Clear();
                    HandleDecryption();
                    break;
                case "3":
                    Console.Clear();
                    GetAddPathFromUser();
                    break;
                case "4":
                    Console.Clear();
                    HandleListEncryptedFiles();
                    break;
                case "0":
                    return;
                default:
                    Console.Clear();
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }
        }
    }

    public static void HandleEncryption()
    {
        string filePath = GetFilePathFromUser();
        if (filePath == null) return;

        if (IsFileEncrypted(filePath))
        {
            Console.Clear();
            Console.WriteLine("File is already encrypted. Aborting.");
            return;
        }

        var keyGen = new GenerateKeysAndIvs();
        byte[] key = new byte[32];
        byte[] iv = new byte[16];
        keyGen.GenerateKeyBytes(key);
        keyGen.GenerateIVBytes(iv);

        EncryptFileInPlace(filePath, key, iv);

        string keyFile = filePath + ".keyinfo";
        SaveKeyInfo(keyFile, key, iv);
        Console.Clear();
        Console.WriteLine("File encrypted and key saved.");
        // Display the key in a readable Base64 format for the user to copy.
        Console.WriteLine($"Key is: {Convert.ToBase64String(key)}");
        Console.WriteLine($"It is saved in the same folder as your original document in {Path.GetFileName(keyFile)}.");
        Console.WriteLine("Press enter to continue.");
        Console.ReadKey();
    }

    public static void HandleDecryption()
    {
        string filePath = GetFilePathFromUser();
        if (filePath == null) return;

        if (!IsFileEncrypted(filePath))
        {
            Console.Clear();
            Console.WriteLine("File is not encrypted. Aborting.");
            return;
        }

        string keyFile = filePath + ".keyinfo";
        if (!File.Exists(keyFile))
        {
            Console.Clear();
            Console.WriteLine("Key file not found: " + keyFile);
            return;
        }

        // Load the key and IV from the saved file.
        LoadKeyInfo(keyFile, out byte[] key, out byte[] iv);

        // Prompt the user to enter the key and compare it.
        Console.WriteLine("Please enter your Encryption key:");
        string userInputKeyString = Console.ReadLine();

        // Convert the loaded key to a Base64 string for comparison.
        string savedKeyString = Convert.ToBase64String(key);

        if (userInputKeyString == savedKeyString)
        {
            try
            {
                // Decrypt the file using the key and IV from the file.
                DecryptFileInPlace(filePath, key, iv);
                Console.Clear();
                Console.WriteLine("File decrypted.");

                // Delete the key file after successful decryption.
                File.Delete(keyFile);
                Console.WriteLine($"Key file '{keyFile}' deleted.");
            }
            catch (CryptographicException)
            {
                // Catch a specific exception for bad key/IV, providing a better error message.
                Console.Clear();
                Console.WriteLine("Decryption failed. The key provided may be incorrect.");
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine($"An error occurred during decryption: {ex.Message}");
            }
        }
        else
        {
            Console.Clear();
            Console.WriteLine("Decryption failed. The key you entered is incorrect.");
        }

        Console.WriteLine("Press enter to continue.");
        Console.ReadKey();
    }

    public static void HandleListEncryptedFiles()
    {
        Console.WriteLine("\nEnter the path to the folder to scan:");
        string folderPath = Console.ReadLine();
        if (!Directory.Exists(folderPath))
        {
            Console.Clear();
            Console.WriteLine("Folder does not exist.");
            Console.WriteLine("Press enter to continue.");
            Console.ReadKey();
            return;
        }

        Console.Clear();
        Console.WriteLine($"Scanning '{folderPath}' for encrypted files...");

        string[] files = Directory.GetFiles(folderPath);

        bool foundEncrypted = false;
        foreach (string file in files)
        {
            if (IsFileEncrypted(file))
            {
                Console.WriteLine($"[ENCRYPTED] {Path.GetFileName(file)}");
                foundEncrypted = true;
            }
        }

        if (!foundEncrypted)
        {
            Console.WriteLine("No encrypted files found in this folder.");
        }

        Console.WriteLine("Press enter to continue.");
        Console.ReadKey();
    }


    public static string GetFilePathFromUser()
    {
        Console.WriteLine("\nEnter the path to the folder containing the file:");
        string folderPath = Console.ReadLine();
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("Folder does not exist.");
            return null;
        }

        Console.WriteLine("Enter the filename (e.g., file.txt):");
        string filename = Console.ReadLine();

        string filePath = Path.Combine(folderPath, filename);
        if (!File.Exists(filePath))
        {
            Console.Clear();
            Console.WriteLine("File does not exist.");
            return null;
        }

        return filePath;
    }
    public static string GetAddPathFromUser()
    {
        Console.WriteLine("\nEnter the path to the folder where you wanna add the .txt");
        string folderPath = Console.ReadLine();
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("Folder does not exist.");
            return null;
        }

        Console.WriteLine("Enter the filename (e.g., file.txt):");
        string filename = Console.ReadLine();
        if (filename.Length >= 50)
        {
            Console.Clear();
            Console.WriteLine("Max length is 30, press enter to continue");
            Console.ReadKey();
            Console.Clear();
            return null;
        }

        string filePath = Path.Combine(folderPath, filename);
        if (!File.Exists(filePath))
        {
            AddFileToPath(filePath);
        }
        else
        {
            Console.Clear();
            Console.WriteLine("File already exists.");
            return null;
        }

        return filePath;
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

    public static void AddFileToPath(string filePath)
    {
        using (FileStream fs = File.Create(filePath))
        {
            Console.WriteLine("What text do you want in the txt file.");
            string customtext = Console.ReadLine();
            byte[] info = new UTF8Encoding(true).GetBytes(customtext);
            fs.Write(info, 0, info.Length);
        }
        Console.Clear();
        Console.WriteLine("File has been created");
        return;
    }
}
