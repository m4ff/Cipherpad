using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Cipherpad
{
    class IncorrectPasswordException : Exception { }

    static class Crypto
    {
        const int PBKDFIterations = 128000;

        const int SaltLength = 16;
        const int IVLength = 16;
        const int HashLength = 32;

        public static CryptoStream OpenEncryptionStream(string password, Stream outStream)
        {
            outStream.WriteByte(0); // version byte

            using (var random = new RNGCryptoServiceProvider())
            {
                var verifySalt = new byte[SaltLength];
                random.GetBytes(verifySalt);

                outStream.Write(verifySalt, 0, SaltLength); // write password verification salt

                using (var derive = new Rfc2898DeriveBytes(password, verifySalt, PBKDFIterations))
                {
                    var verify = derive.GetBytes(HashLength);
                    outStream.Write(verify, 0, HashLength); // write password verification hash
                }

                var keySalt = new byte[SaltLength];
                random.GetBytes(keySalt);

                outStream.Write(keySalt, 0, SaltLength); // write key salt

                var iv = new byte[IVLength];
                random.GetBytes(iv);
                outStream.Write(iv, 0, IVLength); // write IV

                using (var derive = new Rfc2898DeriveBytes(password, keySalt, PBKDFIterations))
                {
                    using (var aes = new AesCryptoServiceProvider())
                    {
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.IV = iv;
                        aes.Key = derive.GetBytes(32);

                        return new CryptoStream(outStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                    }
                }
            }
        }
        

        public static CryptoStream OpenDecryptionStream(string password, Stream inStream)
        {
            inStream.ReadByte(); // version byte, unused, but reserved for future versions

            var verifySalt = new byte[SaltLength];
            inStream.Read(verifySalt, 0, SaltLength); // read password verification salt

            using (var derive = new Rfc2898DeriveBytes(password, verifySalt, PBKDFIterations))
            {
                var verify = derive.GetBytes(HashLength);
                var verifyRead = new byte[HashLength];

                inStream.Read(verifyRead, 0, HashLength); // read password verification hash

                if (!verify.SequenceEqual(verifyRead))
                {
                    throw new IncorrectPasswordException();
                }
            }            

            var keySalt = new byte[SaltLength];
            inStream.Read(keySalt, 0, SaltLength); // read key salt

            var iv = new byte[IVLength];
            inStream.Read(iv, 0, IVLength); // read IV

            using (var derive = new Rfc2898DeriveBytes(password, keySalt, PBKDFIterations))
            {
                using (var aes = new AesCryptoServiceProvider())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.IV = iv;
                    aes.Key = derive.GetBytes(32);

                    return new CryptoStream(inStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                }
            }
        }
    }
}
