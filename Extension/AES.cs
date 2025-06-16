using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace cms_api.Extension
{
    public class AES
    {
        public AES()
        {
        }

        public string AesEncryptECB(string content, string aesKey = "p3s6v8y/B?E(H+Mb")
        {
            byte[] byteKEY = Encoding.UTF8.GetBytes(aesKey);

            byte[] byteContnet = Encoding.UTF8.GetBytes(content);

            var _aes = new RijndaelManaged();
            _aes.Padding = PaddingMode.PKCS7;
            _aes.Mode = CipherMode.ECB;
            _aes.Key = byteKEY;

            var _crypto = _aes.CreateEncryptor();
            byte[] decrypted = _crypto.TransformFinalBlock(byteContnet, 0, byteContnet.Length);

            _crypto.Dispose();

            return Convert.ToBase64String(decrypted);
        }

        public string AesDecryptECB(string decryptStr, string aesKey = "p3s6v8y/B?E(H+Mb")
        {
            byte[] byteKEY = Encoding.UTF8.GetBytes(aesKey);
            byte[] byteDecrypt = System.Convert.FromBase64String(decryptStr);

            var _aes = new RijndaelManaged();
            _aes.Padding = PaddingMode.PKCS7;
            _aes.Mode = CipherMode.ECB;
            _aes.Key = byteKEY;

            var _crypto = _aes.CreateDecryptor();
            byte[] decrypted = _crypto.TransformFinalBlock(byteDecrypt, 0, byteDecrypt.Length);

            _crypto.Dispose();

            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
