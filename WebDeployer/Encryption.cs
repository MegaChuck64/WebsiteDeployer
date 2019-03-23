using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebDeployer
{
    static class Encryption
    {


        public static byte[] StringToBytes(string stringToEncrypt, string optionalEntropy, DataProtectionScope scope)
        {
            return
                ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(stringToEncrypt)
                    , optionalEntropy != null ? Encoding.UTF8.GetBytes(optionalEntropy) : null
                    , scope);
        }

        public static string BytesToString(byte[] encryptedBytes, string optionalEntropy, DataProtectionScope scope)
        {
            return Encoding.UTF8.GetString(
                ProtectedData.Unprotect(
                        encryptedBytes
                    , optionalEntropy != null ? Encoding.UTF8.GetBytes(optionalEntropy) : null
                    , scope));
        }



    }

}
