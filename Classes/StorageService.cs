using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace FlatFileStorage;

public class StorageService
{
    private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

    //Preconfigured Encryption Parameters
    public static readonly int BlockBitSize = 128;
    public static readonly int KeyBitSize = 256;

    //Preconfigured Password Key Derivation Parameters
    public static readonly int SaltBitSize = 64;
    public static readonly int Iterations = 10000;
    public static readonly int MinPasswordLength = 12;

    public readonly string FilePath;
    //78 digit
    private readonly byte[] cryptKey = Encoding.ASCII.GetBytes("OsXgqeyw65nqrvgv1iIzD9XJFcDUxqK/");
    private readonly byte[] authKey = Encoding.ASCII.GetBytes("eFKuLC44l1rGHTGwGRQV+b//XcG8SkFg");
    private readonly AppSettings _config;
    public StorageService()
    {
        // Get config
        var configJson = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "config.json"));
        _config = JsonSerializer.Deserialize<AppSettings>(configJson);

        // Working directory
        FilePath = Path.Combine(Directory.GetCurrentDirectory(), _config.WorkingDirectory);
        if (!Directory.Exists(FilePath))
        {
            Directory.CreateDirectory(FilePath);
        }
    }
    public bool SendToFile(StorageList storageList, string user, string file)
    {
        try
        {
            // Send to file
            string json = JsonSerializer.Serialize(storageList);
            string encrypted = SimpleEncrypt(json, cryptKey, authKey);
            using StreamWriter outputFile = new(Path.Combine(FilePath, user, file));
            outputFile.Write(encrypted);
            outputFile.Close();
        }
        catch
        {
            return false;
        }
        return true;
    }
    /**
     * Get contents of file, or return a new empty set
     */
    public StorageList ReadFromFile(string user, string name)
    {
        StorageList response = new();
        // If we just created the file new, return an empty set
        if (CreateFile(user, name))
            return response;

        // File already existed, parse its contents
        string text = File.ReadAllText(Path.Combine(FilePath, user, name));
        // If it was empty, return an empty set
        if (string.IsNullOrEmpty(text)) return response;
        string decryptedText = SimpleDecrypt(text, cryptKey, authKey);
        response = JsonSerializer.Deserialize<StorageList>(decryptedText);
        return response;
    }
    public bool DeleteFile(string user, string name)
    {
        try
        {
            string path = Path.Combine(FilePath, user, name);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
        return true;
    }
    /**
     * Creates file at given string path and returns true.
     * If file already exists, returns false
     */
    public bool CreateFile(string user, string name)
    {
        string path = Path.Combine(FilePath, user, name);
        try
        {
            if (!File.Exists(path))
            {
                var file = File.Create(path);
                file.Close();
                return true;
            }
            else
            {
                return false;
            }

        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Simple Encryption (AES) then Authentication (HMAC) for a UTF8 Message.
    /// </summary>
    /// <param name="secretMessage">The secret message.</param>
    /// <param name="cryptKey">The crypt key.</param>
    /// <param name="authKey">The auth key.</param>
    /// <param name="nonSecretPayload">(Optional) Non-Secret Payload.</param>
    /// <returns>
    /// Encrypted Message
    /// </returns>
    /// <exception cref="System.ArgumentException">Secret Message Required!;secretMessage</exception>
    /// <remarks>
    /// Adds overhead of (Optional-Payload + BlockSize(16) + Message-Padded-To-Blocksize +  HMac-Tag(32)) * 1.33 Base64
    /// </remarks>
    private static string SimpleEncrypt(string secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
    {
        if (string.IsNullOrEmpty(secretMessage))
            throw new ArgumentException("Secret Message Required!", nameof(secretMessage));

        var plainText = Encoding.UTF8.GetBytes(secretMessage);
        var cipherText = SimpleEncrypt(plainText, cryptKey, authKey, nonSecretPayload);
        return Convert.ToBase64String(cipherText);
    }
    /// <summary>
    /// Simple Authentication (HMAC) then Decryption (AES) for a secrets UTF8 Message.
    /// </summary>
    /// <param name="encryptedMessage">The encrypted message.</param>
    /// <param name="cryptKey">The crypt key.</param>
    /// <param name="authKey">The auth key.</param>
    /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
    /// <returns>
    /// Decrypted Message
    /// </returns>
    /// <exception cref="System.ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
    private static string SimpleDecrypt(string encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
    {
        if (string.IsNullOrWhiteSpace(encryptedMessage))
            throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));

        var cipherText = Convert.FromBase64String(encryptedMessage);
        var plainText = SimpleDecrypt(cipherText, cryptKey, authKey, nonSecretPayloadLength);
        return plainText == null ? null : Encoding.UTF8.GetString(plainText);
    }
    private static byte[] SimpleEncrypt(byte[] secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
    {
        //User Error Checks
        if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
            throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), nameof(cryptKey));

        if (authKey == null || authKey.Length != KeyBitSize / 8)
            throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), nameof(authKey));

        if (secretMessage == null || secretMessage.Length < 1)
            throw new ArgumentException("Secret Message Required!", nameof(secretMessage));

        //non-secret payload optional
        nonSecretPayload ??= [];

        byte[] cipherText;
        byte[] iv;

        using (var aes = new AesManaged
        {
            KeySize = KeyBitSize,
            BlockSize = BlockBitSize,
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7
        })
        {

            //Use random IV
            aes.GenerateIV();
            iv = aes.IV;

            using var encrypter = aes.CreateEncryptor(cryptKey, iv);
            using var cipherStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
            using (var binaryWriter = new BinaryWriter(cryptoStream))
            {
                //Encrypt Data
                binaryWriter.Write(secretMessage);
            }

            cipherText = cipherStream.ToArray();

        }

        //Assemble encrypted message and add authentication
        using var hmac = new HMACSHA256(authKey);
        using var encryptedStream = new MemoryStream();
        using (var binaryWriter = new BinaryWriter(encryptedStream))
        {
            //Prepend non-secret payload if any
            binaryWriter.Write(nonSecretPayload);
            //Prepend IV
            binaryWriter.Write(iv);
            //Write Ciphertext
            binaryWriter.Write(cipherText);
            binaryWriter.Flush();

            //Authenticate all data
            var tag = hmac.ComputeHash(encryptedStream.ToArray());
            //Postpend tag
            binaryWriter.Write(tag);
        }
        return encryptedStream.ToArray();

    }

    private static byte[] SimpleDecrypt(byte[] encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
    {
        //Basic Usage Error Checks
        if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
            throw new ArgumentException(String.Format("CryptKey needs to be {0} bit!", KeyBitSize), nameof(cryptKey));

        if (authKey == null || authKey.Length != KeyBitSize / 8)
            throw new ArgumentException(String.Format("AuthKey needs to be {0} bit!", KeyBitSize), nameof(authKey));

        if (encryptedMessage == null || encryptedMessage.Length == 0)
            throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));

        using var hmac = new HMACSHA256(authKey);
        var sentTag = new byte[hmac.HashSize / 8];
        //Calculate Tag
        var calcTag = hmac.ComputeHash(encryptedMessage, 0, encryptedMessage.Length - sentTag.Length);
        var ivLength = BlockBitSize / 8;

        //if message length is to small just return null
        if (encryptedMessage.Length < sentTag.Length + nonSecretPayloadLength + ivLength)
            return null;

        //Grab Sent Tag
        Array.Copy(encryptedMessage, encryptedMessage.Length - sentTag.Length, sentTag, 0, sentTag.Length);

        //Compare Tag with constant time comparison
        var compare = 0;
        for (var i = 0; i < sentTag.Length; i++)
            compare |= sentTag[i] ^ calcTag[i];

        //if message doesn't authenticate return null
        if (compare != 0)
            return null;

        using var aes = new AesManaged
        {
            KeySize = KeyBitSize,
            BlockSize = BlockBitSize,
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7
        };

        //Grab IV from message
        var iv = new byte[ivLength];
        Array.Copy(encryptedMessage, nonSecretPayloadLength, iv, 0, iv.Length);

        using var decrypter = aes.CreateDecryptor(cryptKey, iv);
        using var plainTextStream = new MemoryStream();
        using (var decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
        using (var binaryWriter = new BinaryWriter(decrypterStream))
        {
            //Decrypt Cipher Text from Message
            binaryWriter.Write(
              encryptedMessage,
              nonSecretPayloadLength + iv.Length,
              encryptedMessage.Length - nonSecretPayloadLength - iv.Length - sentTag.Length
            );
        }
        //Return Plain Text
        return plainTextStream.ToArray();
    }
}