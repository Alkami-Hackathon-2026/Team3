using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Alkami.Utilities.Hashing
{
    /// <summary>
    /// Static class that handles the computation of a hash based on a keyed hash algorithm.
    /// </summary>
    public static class HashUtility
    {
        /// <summary>
        /// Computes a hash of a specified key/message based on the supplied hash algorithm.
        /// </summary>
        /// <param name="messageBytes">The message bytes.</param>
        /// <param name="keyBytes">The key to hash the method with.</param>
        /// <param name="hashMethod">The method to use to perform the hash.</param>
        /// <returns>
        /// The computed hash represented as a hex string.
        /// </returns>
        public static string Compute(byte[] messageBytes, byte[] keyBytes, HashMethod hashMethod)
        {
            return ByteArrayToHexString(ComputeBinary(messageBytes, keyBytes, hashMethod));
        }

        /// <summary>
        /// Computes a hash of a specified key/message based on the supplied hash algorithm.
        /// </summary>
        /// <param name="messageBytes">The message bytes.</param>
        /// <param name="key">The key to hash the method with.</param>
        /// <param name="hashMethod">The method to use to perform the hash.</param>
        /// <returns>
        /// The computed hash represented as a hex string.
        /// </returns>
        public static string Compute(byte[] messageBytes, string key, HashMethod hashMethod)
        {
            if (string.IsNullOrEmpty(key) || messageBytes.Length == 0)
            {
                throw new Exception();
            }
            return ByteArrayToHexString(ComputeBinary(messageBytes, StringToByteArray(key), hashMethod));
        }

        /// <summary>
        /// Computes a hash of a specified key/message based on the supplied hash algorithm.
        /// </summary>
        /// <param name="message">The message to hash.</param>
        /// <param name="keyBytes">The key to hash the method with.</param>
        /// <param name="hashMethod">The method to use to perform the hash.</param>
        /// <param name="byteArrayToHexStringLower"></param>
        /// <returns>The computed hash represented as a hex string.</returns>
        public static string Compute(string message, byte[] keyBytes, HashMethod hashMethod, bool byteArrayToHexStringLower = false)
        {
            var messageBytes = StringToByteArray(message);
            return ByteArrayToHexString(ComputeBinary(messageBytes, keyBytes, hashMethod), byteArrayToHexStringLower);
        }

        /// <summary>
        /// Computes a hash of a specified key/message based on the supplied hash algorithm.
        /// </summary>
        /// <param name="message">The message to hash.</param>
        /// <param name="keyBytes">The key to hash the method with.</param>
        /// <param name="hashMethod">The method to use to perform the hash.</param>
        /// <returns>The computed hash represented as a hex string.</returns>
        public static byte[] ComputeBinary(string message, byte[] keyBytes, HashMethod hashMethod)
        {
            var messageBytes = StringToByteArray(message);
            return ComputeBinary(messageBytes, keyBytes, hashMethod);
        }

        /// <summary>
        /// Computes a hash of a specified key/message based on the supplied hash algorithm.
        /// </summary>
        /// <param name="messageBytes">The message to hash.</param>
        /// <param name="keyBytes">The key to hash the method with.</param>
        /// <param name="hashMethod">The method to use to perform the hash.</param>
        /// <returns>The computed hash represented as a hex string.</returns>
        public static byte[] ComputeBinary(byte[] messageBytes, byte[] keyBytes, HashMethod hashMethod)
        {
            using (var hmac = MkHmac(keyBytes, hashMethod))
            {
                return hmac.ComputeHash(messageBytes);
            }
        }

        private static HMAC MkHmac(byte[] keyBytes, HashMethod hashMethod)
        {
            switch (hashMethod)
            {
                case HashMethod.HMACMD5: return new HMACMD5(keyBytes);
#if NETFRAMEWORK
                case HashMethod.HMACRIPEMD160: return new HMACRIPEMD160(keyBytes);
#endif
                case HashMethod.HMACSHA1: return new HMACSHA1(keyBytes);
                case HashMethod.HMACSHA256: return new HMACSHA256(keyBytes);
                case HashMethod.HMACSHA384: return new HMACSHA384(keyBytes);
                case HashMethod.HMACSHA512: return new HMACSHA512(keyBytes);
                default: throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Computes a hash of a specified key/message based on the supplied hash algorithm.
        /// </summary>
        /// <param name="message">The message to hash.</param>
        /// <param name="textStringKey">The key to hash the method with as a text string.</param>
        /// <param name="hashMethod">The method to use to perform the hash.</param>
        /// <returns>The computed hash represented as a hex string.</returns>
        public static string Compute(string message, string textStringKey, HashMethod hashMethod)
        {
            var keyBytes = StringToByteArray(textStringKey);
            var messageBytes = StringToByteArray(message);
            return Compute(messageBytes, keyBytes, hashMethod);
        }

        /// <summary>
        /// Computes a hash of a specified key/message based on the supplied hash algorithm.
        /// </summary>
        /// <param name="message">The message to hash.</param>
        /// <param name="hexStringKey">The key to hash the method with as a HEX string.</param>
        /// <param name="hashMethod">The method to use to perform the hash.</param>
        /// <returns>The computed hash represented as a hex string.</returns>

        public static string ComputeFromHexString(string message, string hexStringKey, HashMethod hashMethod)
        {
            var keyBytes = HexStringToByteArray(hexStringKey);
            return Compute(message, keyBytes, hashMethod);
        }

        /// <summary>
        /// Bytes the array to hexadecimal string.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="isLower">If true, sets X2 to x2.</param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] bytes, bool isLower = false)
        {
            if (bytes == null || bytes.Length == 0)
                return String.Empty;
            return string.Join("", bytes.Select(x => x.ToString(isLower ? "x2" : "X2", CultureInfo.InvariantCulture)).ToArray());
        }

        /// <summary>
        /// Bytes the array to hexadecimal string lower.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string ByteArrayToHexStringLower(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return String.Empty;
            return string.Join("", bytes.Select(x => x.ToString("x2", CultureInfo.InvariantCulture)).ToArray());
        }

        /// <summary>
        /// Strings to byte array.
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        /// <summary>
        /// string to byte array.
        /// </summary>
        /// <param name="characters"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string characters)
        {
            return (characters == null) ? new byte[0] : Encoding.UTF8.GetBytes(characters);
        }
    }
}
