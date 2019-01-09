using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace io.cryptol.cloudwave
{
    public class EncSettings
    {
        public string inputFile { get; set; }
        public string outputFile { get; set; }
        public string password { get; set; }
        public enum encType
        {
            Encrypt,
            Decrypt
        }
        public encType Type { get; set; }
        
    }
    public class Cryptol
    {
        static RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();

        public string CryptolLoad(EncSettings settings)
        {
            string control = FileControl(settings);

            if (!control.Contains("Code 05"))
            {
                return control;
            }

            if(settings.Type == EncSettings.encType.Encrypt)
            {
                try
                {
                    string result = EncryptFile(settings);
                    if(result.Contains("Code 01"))
                    {
                        return "Encryption Done! --> " + result;
                    }
                    else if(result.Contains("Code 00"))
                    {
                        return "Encryption Error! --> " + result;
                    }
                }
                catch(Exception ex)
                {
                    return "Encryption Error --> " + ex.ToString();
                }
            }
            else if(settings.Type == EncSettings.encType.Decrypt)
            {
                try
                {
                    string result = DecryptFile(settings);
                    if (result.Contains("Code 01"))
                    {
                        return "Decryption Done! --> " + result;
                    }
                    else if (result.Contains("Code 00"))
                    {
                        return "Decryption Error! --> " + result;
                    }
                }
                catch (Exception ex)
                {
                    return "Decryption Error --> " + ex.ToString();
                }
            }

            return "Unknown Error!";
        }

        string FileControl(EncSettings settings)
        {
            if(!File.Exists(settings.inputFile))
            {
                return "Code 03: inputFile does not Exist! Stopping...";
            }
            if(File.Exists(settings.outputFile))
            {
                return "Code 04: outputFile already Exist! Please remove it before Encrypt/Decrypt. Stopping...";
            }
            if (settings.password == string.Empty || settings.password == null || settings.password == "")
            {
                return "Code 06: password is Empty! Please insert a password before Encrypt/Decrypt. Stopping...";
            }

            return "Code 05: FileControl passed!";
        }
        string EncryptFile(EncSettings settings)
        {

            long totalBytes = new FileInfo(settings.inputFile).Length;
            byte[] salt = new byte[16];
            rand.GetBytes(salt);
            byte[] IV = new byte[16];
            rand.GetBytes(IV);
            byte[] key = new Rfc2898DeriveBytes(settings.password, salt, 77).GetBytes(32);
            string cryptFile = settings.outputFile;
            try
            {
                using (FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create))
                {
                    fsCrypt.Write(salt, 0, salt.Length);
                    fsCrypt.Write(IV, 0, IV.Length);
                    using (RijndaelManaged RMCrypto = new RijndaelManaged())
                    {
                        using (CryptoStream cs = new CryptoStream(fsCrypt, RMCrypto.CreateEncryptor(key, IV), CryptoStreamMode.Write))
                        {
                            using (FileStream fsIn = new FileStream(settings.inputFile, FileMode.Open))
                            {
                                byte[] buffer = new byte[1024 * 1024];
                                int data;
                                long bytesRead = 0;
                                while ((data = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    bytesRead += data;
                                    cs.Write(buffer, 0, data);
                                }
                                return "Code 01 : File successfully Encrypted!";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Code 00 : Error while Encrypting File " + ex.ToString();
            }
        }

        string DecryptFile(EncSettings settings)
        {
            long totalBytes = new FileInfo(settings.inputFile).Length;
            byte[] salt = new byte[16];
            byte[] IV = new byte[16];
            try
            {
                using (FileStream fsCrypt = new FileStream(settings.inputFile, FileMode.Open))
                {
                    fsCrypt.Read(salt, 0, salt.Length);
                    fsCrypt.Read(IV, 0, IV.Length);
                    byte[] key = new Rfc2898DeriveBytes(settings.password, salt, 77).GetBytes(32);
                    using (RijndaelManaged RMCrypto = new RijndaelManaged())
                    {
                        using (CryptoStream cs = new CryptoStream(fsCrypt, RMCrypto.CreateDecryptor(key, IV), CryptoStreamMode.Read))
                        {
                            using (FileStream fsOut = new FileStream(settings.outputFile, FileMode.Create))
                            {
                                byte[] buffer = new byte[1024 * 1024];
                                int data;
                                long bytesRead = 0;
                                while ((data = cs.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    bytesRead += data;
                                    fsOut.Write(buffer, 0, data);
                                }
                                return "Code 01 : File successfully Decrypted!";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Code 00 : Error while Decrypting File " + ex.ToString();
            }
        }
    }
}
