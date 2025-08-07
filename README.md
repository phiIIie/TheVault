# The Vault

Welcome to **The Vault**, a simple yet effective command-line tool for encrypting and decrypting your files. This program is designed to provide a layer of security for your personal documents by using randomly generated, unique encryption keys for each file.

---

## Features

- **Encrypt a file**  
  Securely encrypts the contents of a plain text file. Adds a special header to mark it as encrypted and generates a unique `.keyinfo` file containing the encryption key and initialization vector (IV).

- **Decrypt a file**  
  Reverses the encryption process using the encrypted file and its corresponding `.keyinfo` file. After decryption, it restores the original content and deletes the `.keyinfo` file for added security.

- **Create a file**  
  Easily create a new `.txt` file and add initial content through the application interface.

- **List encrypted files**  
  Scans a specified folder and identifies files encrypted by The Vault.

- **Unit Test**  
  Verifies the core encryption and decryption logic is working correctly.

---

## Installation

To get started with The Vault, you'll need the **.NET SDK** installed on your machine.

**Clone the repository:**
```bash
git clone [repository-url]
cd TheVault
```

**Build the project:**
```bash
dotnet build
```

**Run the application:**
```bash
dotnet run
```

---

## How to Use

Once the program is running, you'll see a simple menu. Enter the number corresponding to the option you'd like to use.

### Options:

- **Encrypt a file**  
  Provide the folder path and filename. The program will encrypt the file and create a `.keyinfo` file.  
  ⚠️ *Make sure to back up the key file — it's required for decryption.*

- **Decrypt a file**  
  Provide the path to the encrypted file. The program will find its `.keyinfo` file and prompt for the encryption key.

- **Create a file**  
  Enter the path and filename for your new file, then input the content.

- **List encrypted files**  
  Enter a folder path, and the program will list all files encrypted by The Vault.

- **Run Unit Test**  
  Runs an internal test to verify the encryption/decryption logic. It creates and deletes a temporary file.

- **Exit**  
  Closes the application.

---

## Contributing

We welcome contributions! If you have ideas for new features, bug fixes, or improvements:

1. Fork the repository.  
2. Create a new branch:  
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. Make your changes and commit them:  
   ```bash
   git commit -m "Add new feature"
   ```
4. Push to the branch:  
   ```bash
   git push origin feature/your-feature-name
   ```
5. Open a Pull Request.
