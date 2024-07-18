using System.Security.Cryptography;
using System.Xml.Serialization;

namespace internship.Core.FileAccess
{
    public class FileManager<T>
    {
        private static readonly byte[] EncryptionKey = new byte[]
    {
        0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef,
        0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54, 0x32, 0x10
    };

        public void SaveObjectData(string filePath, T obj)
        {
            // Serialize to XML
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, obj);
                string xmlString = textWriter.ToString();

                // Encrypt the XML string
                string encryptedString = EncryptString(xmlString);

                // Save the encrypted string to file
                File.WriteAllText(filePath, encryptedString);
            }
        }

        public T ReadObjectData(string filePath)
        {
            // Read the encrypted string from file
            string encryptedString = File.ReadAllText(filePath);

            // Decrypt the string
            string xmlString = DecryptString(encryptedString);

            // Deserialize from XML
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StringReader textReader = new StringReader(xmlString))
            {
                return (T)xmlSerializer.Deserialize(textReader);
            }
        }

        private string EncryptString(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = EncryptionKey;
                aesAlg.GenerateIV();
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        private string DecryptString(string cipherText)
        {
            byte[] fullCipher = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = EncryptionKey;
                byte[] iv = new byte[aesAlg.BlockSize / 8];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
