using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Alkami.Utilities.Cryptography
{
    /// <summary>
    /// The AsymmetricEncryption contains an Encryptor and Decryptor that can be used to
    /// security encrypt large amounts of data securely. This was originally made for log 
    /// encryption. It is currently only used by log encryption.
    /// </summary>
    /// <remarks>
    /// Since asymmetric encryption is only designed to encrypt small amounts of data, 
    /// encryption on the majority of the payload is done using a symmetric encryption
    /// provider using random Key/IV. The certificate and asymmetric encryption is 
    /// used to only encrypt the Key/IV. The following table documents the format of the
    /// encrypted string.
    ///     <list type="table">
    ///         <listheader>
    ///             <name>Name</name>
    ///             <term>Data Type</term>
    ///             <encoding>Encoding</encoding>
    ///             <encryption>Encryption</encryption>
    ///             <remarks>Remarks</remarks>
    ///         </listheader>
    ///         <item>
    ///             <name>Thumbprint length</name>
    ///             <term>ushort</term>
    ///             <encoding></encoding>
    ///             <encryption></encryption>
    ///             <remarks>The length of the Thumbprint byte array.</remarks>
    ///         </item>
    ///         <item>
    ///             <name>Thumbprint</name>
    ///             <term>byte array</term>
    ///             <encoding>ASCII</encoding>
    ///             <encryption></encryption>
    ///             <remarks>The thumbprint identifies the certificate that was used to encrypt the cipher. That certificate will be needed to decrypt the encrypted cargo.</remarks>
    ///         </item>
    ///         <item>
    ///             <name>Cipher length</name>
    ///             <term>ushort</term>
    ///             <encoding></encoding>
    ///             <encryption></encryption>
    ///             <remarks>The length of the Cipher byte array.</remarks>
    ///         </item>
    ///         <item>
    ///             <name>Cipher</name>
    ///             <term>byte array</term>
    ///             <encoding>ASCII</encoding>
    ///             <encryption>Encrypted with certificate</encryption>
    ///             <remarks>The cipher contains the Key and IV that was used to encrypt the cargo. It is encrypted with the certificate.</remarks>
    ///         </item>
    ///         <item>
    ///             <name>Encrypted Cargo length</name>
    ///             <term>ushort</term>
    ///             <encoding></encoding>
    ///             <encryption></encryption>
    ///             <remarks>The length of the encrypted Cargo byte array.</remarks>
    ///         </item>
    ///         <item>
    ///             <name>Cargo</name>
    ///             <term>byte array</term>
    ///             <encoding>UTF8</encoding>
    ///             <encryption>Symmetric encrypted using random Key/IV stored in Cipher</encryption>
    ///             <remarks>Contains the encrypted cargo that was encrypted using symmetric encryption using a random Key and IV that has been stored in the Cipher.</remarks>
    ///         </item>
    ///         <item>
    ///             <name>Cargo length</name>
    ///             <term>ushort</term>
    ///             <encoding></encoding>
    ///             <encryption></encryption>
    ///             <remarks>The original length of the Cargo byte array before encryption.</remarks>
    ///         </item>
    ///     </list>
    /// </remarks>
    public static class AsymmetricEncryption
    {
        /// <summary>
        ///  The limit of bytes that we can encrypt and decrypt successfully
        /// </summary>
        internal const int EncryptionByteLimit = 65_517;

        /// <summary>
        /// The Encrypt method encrypts a string using the specified certificate.
        /// </summary>
        /// <param name="payload">The payload to encrypt.</param>
        /// <param name="certificate">The X.509 certificate to use to encrypt the payload.</param>
        /// <returns>Returns a string that contains the encrypted payload.</returns>
        public static string EncryptWithThumbprint(string payload, X509Certificate2 certificate)
        {
            // Check that the input string is under the limit for the number of bytes
            // that we can decrypt successfully. If not, we should throw that to the caller.
            var payloadByteLength = Encoding.UTF8.GetByteCount(payload);
            if (payloadByteLength > EncryptionByteLimit)
            {
                throw new ArgumentException("Input string too long, must be less than 65,517 bytes");
            }

            using (var encryptor = new Encryptor(payload, certificate))
            {
                return encryptor.GetEncryptedString();
            }
        }

        /// <summary>
        /// The Decrypt method decrypts a string that was encrypted by the <see cref="Encryptor"/>.
        /// </summary>
        /// <param name="payload">The encrypted payload to decrypt.</param>
        /// <returns>Returns the decrypted payload.</returns>
        public static string DecryptWithThumbprint(string payload)
        {
            using (var decryptor = new Decryptor(payload))
            {
                return decryptor.GetDecryptedString();
            }
        }

        /// <summary>
        /// The Decrypt method decrypts a string that was encrypted by the <see cref="Encryptor"/>.
        /// </summary>
        /// <param name="payload">The encrypted payload to decrypt.</param>
        /// <param name="certificate">The certificate containing the private key for decrypting the payload.</param>
        /// <returns>Returns the decrypted payload.</returns>
        public static string Decrypt(string payload, X509Certificate2 certificate)
        {
            using (var decryptor = new Decryptor(payload, certificate))
            {
                return decryptor.GetDecryptedString();
            }
        }

        /// <summary>
        /// The Encryptor is a sealed call that is used to assymetrically encrypt a payload.
        /// The <see cref="Decryptor"/> class decrypts a payload encrypted using this method.
        /// </summary>
        public sealed class Encryptor : IDisposable
        {
            private byte[] thumbprintAsBytes;
            private byte[] payloadAsBytes;
            private RSA publicKey;
            private byte[] cipherEncryptedAsBytes;
            private byte[] payloadEncryptedAsBytes;
            private bool ownsCryptoProvider;

            /// <summary>
            /// The constructor that creates an instance of the Encryptor class.
            /// </summary>
            /// <param name="payload">The payload to encrypt.</param>
            /// <param name="certificate">The X.509 certificate to use to encrypt the cipher.</param>
            /// <param name="cryptoProvider">The symmetric cryptography provider to use to encrypt the payload using the cipher.</param>
            public Encryptor(string payload, X509Certificate2 certificate, SymmetricAlgorithm cryptoProvider = null)
            {
                Payload = payload;
                Certificate = certificate;
                if (cryptoProvider == null)
                {
                    CryptoProvider = Aes.Create();
                    ownsCryptoProvider = true;
                }
                else
                {
                    CryptoProvider = cryptoProvider;
                }
            }

            /// <summary>
            /// Gets the encrypted string using the assigned properties of the Encryptor instance.
            /// </summary>
            /// <returns>Returns the encrypted value as a string.</returns>
            public string GetEncryptedString()
            {
                using (var stream = new MemoryStream(Payload.Length * 2))
                {
                    WriteBytesToStream(stream, ThumbprintAsBytes);
                    WriteBytesToStream(stream, CipherEncryptedAsBytes);
                    WriteBytesToStream(stream, PayloadEncryptedAsBytes);

                    return Convert.ToBase64String(stream.ToArray());
                }
            }

            private X509Certificate2 Certificate { get; }
            private SymmetricAlgorithm CryptoProvider { get; }
            private string Payload { get; }

            private string Thumbprint
            {
                get { return Certificate.Thumbprint; }
            }

            private byte[] ThumbprintAsBytes
            {
                get { return thumbprintAsBytes ?? (thumbprintAsBytes = Encoding.ASCII.GetBytes(Thumbprint)); }
            }

            private byte[] PayloadAsBytes
            {
                get { return payloadAsBytes ?? (payloadAsBytes = Encoding.UTF8.GetBytes(Payload)); }
            }

            private RSA PublicKey
            {
                get { return publicKey ?? (publicKey = Certificate.GetRSAPublicKey()); }
            }

            private byte[] CipherEncryptedAsBytes
            {
                get
                {
                    if (cipherEncryptedAsBytes != null)
                        return cipherEncryptedAsBytes;
                    byte[] cipherbytes;
                    using (var cipherStream = new MemoryStream())
                    {
                        WriteBytesToStream(cipherStream, CryptoProvider.Key);
                        WriteBytesToStream(cipherStream, CryptoProvider.IV);
                        cipherbytes = cipherStream.ToArray();
                    }
                    cipherEncryptedAsBytes = PublicKey.Encrypt(cipherbytes, RSAEncryptionPadding.OaepSHA1);

                    return cipherEncryptedAsBytes;
                }
            }

            private byte[] PayloadEncryptedAsBytes
            {
                get
                {
                    if (payloadEncryptedAsBytes != null)
                        return payloadEncryptedAsBytes;
                    using (var stream = new MemoryStream(PayloadAsBytes.Length * 2))
                    using (var cryptStream = new CryptoStream(stream, CryptoProvider.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        WriteBytesToStream(cryptStream, PayloadAsBytes);
                        cryptStream.FlushFinalBlock();
                        payloadEncryptedAsBytes = stream.ToArray();
                    }
                    return payloadEncryptedAsBytes;
                }
            }

            private void WriteBytesToStream(Stream stream, byte[] bytes, bool writeLength = true)
            {
                if (writeLength)
                    WriteLengthToStream(stream, bytes.Length);
                stream.Write(bytes, 0, bytes.Length);
            }

            private void WriteLengthToStream(Stream stream, int value)
            {
                byte[] bytes = BitConverter.GetBytes((short)value);
                stream.Write(bytes, 0, bytes.Length);
            }

            void IDisposable.Dispose()
            {
                if (ownsCryptoProvider)
                {
                    CryptoProvider?.Dispose();
                    ownsCryptoProvider = false;
                }
            }
        }

        /// <summary>
        /// The Decryptor is a sealed call that is used to assymetrically decrypt a payload
        /// that was encrypted using <see cref="Encryptor"/>. You should use WebDecrypt to
        /// decrypt any logs.
        /// </summary>
        public sealed class Decryptor : IDisposable
        {
            private X509Certificate2 certificate;
            private Tuple<byte[], byte[]> cipher;
            private SymmetricAlgorithm cryptoProvider;
            private string payload;
            private RSA privateKey;
            private bool ownsCryptoProvider;

            /// <summary>
            /// The constructor that creates an instance of the Decryptor class.
            /// </summary>
            /// <param name="payload">The payload to decrypt.</param>
            /// <param name="certificate">
            /// The certificate to use to decrypt the payload. If this is null, the certificate will
            /// be found based on the thumbprint of the certificate that encrypted the message. If
            /// the certificate's doesn't private key doesn't match the public key that encrypted
            /// the payload, the decryption will fail.
            /// </param>
            /// <param name="cryptoProvider">
            /// The SymmetricAlgorithm cryptography provider to use to decrypt the payload. If this
            /// is null, the RijndaelManaged crypto provider will be used. If the crypto provider
            /// doesn't match the provider that was used to encrypt the message, the results are
            /// unknown but probably wrong.
            /// </param>
            public Decryptor(string payload, X509Certificate2 certificate = null, SymmetricAlgorithm cryptoProvider = null)
            {
                PayloadAsBytes = Convert.FromBase64String(payload);
                Certificate = certificate;
                CryptoProvider = cryptoProvider;
            }

            /// <summary>
            /// Gets the decrypted string using the assigned properties of the Decryptor instance.
            /// </summary>
            /// <returns>Returns the decrypted value as a string.</returns>
            public string GetDecryptedString()
            {
                using (var stream = new MemoryStream(PayloadAsBytes))
                {
                    ThumbprintAsBytes = ReadBytesFromStream(stream);
                    EncryptedCipherAsBytes = ReadBytesFromStream(stream);
                    EncryptedPayloadAsBytes = ReadBytesFromStream(stream);
                }

                return Payload;
            }

            private X509Certificate2 Certificate
            {
                get
                {
                    if (certificate == null)
                    {
                        certificate = FindCertificateForThumbprint(Thumbprint);
                    }

                    if (certificate == null)
                        throw new Exception($"Unable to locate certificate for the following thumbprint: {Thumbprint}");

                    return certificate;
                }
                set { certificate = value; }
            }

            private X509Certificate2 FindCertificateForThumbprint(string thumbprint)
            {
                // Loop through the Personal and Trusted People stores within the LocalMachine and CurrentUser
                // looking for the appropriate certificate with the thumbprint that was used to encrypt the payload
                foreach (var store in new[] { StoreName.My, StoreName.TrustedPeople })
                {
                    foreach (var location in new[] { StoreLocation.LocalMachine, StoreLocation.CurrentUser })
                    {
                        var cert = CertificateUtility.GetCertificateIfPresent(store, location, X509FindType.FindByThumbprint, thumbprint);
                        if (cert != null)
                            return cert;
                    }
                }

                return null;
            }

            private SymmetricAlgorithm CryptoProvider
            {
                get
                {
                    if (cryptoProvider != null)
                        return cryptoProvider;

                    cryptoProvider = Aes.Create();
                    cryptoProvider.Key = Cipher.Item1;
                    cryptoProvider.IV = Cipher.Item2;

                    ownsCryptoProvider = true;

                    return cryptoProvider;
                }
                set
                {
                    cryptoProvider = value;
                    ownsCryptoProvider = false;
                }
            }

            private string Payload
            {
                get
                {
                    if (payload != null)
                        return payload;
                    using (var stream = new MemoryStream(EncryptedPayloadAsBytes))
                    using (var cryptStream = new CryptoStream(stream, CryptoProvider.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        payload = Encoding.UTF8.GetString(ReadBytesFromStream(cryptStream));
                    }
                    return payload;
                }
            }

            private string Thumbprint
            {
                get { return Encoding.ASCII.GetString(ThumbprintAsBytes); }
            }

            private byte[] ThumbprintAsBytes { get; set; }

            private byte[] EncryptedPayloadAsBytes { get; set; }

            private byte[] PayloadAsBytes { get; }

            private RSA PrivateKey
            {
                get { return privateKey ?? (privateKey = Certificate.GetRSAPrivateKey()); }
            }

            private byte[] EncryptedCipherAsBytes { get; set; }

            private Tuple<byte[], byte[]> Cipher
            {
                get
                {
                    if (cipher != null)
                        return cipher;
                    using (var stream = DecryptionCipherMemoryStream())
                    {
                        byte[] key = ReadBytesFromStream(stream);
                        byte[] iv = ReadBytesFromStream(stream);
                        cipher = new Tuple<byte[], byte[]>(key, iv);
                    }

                    return cipher;
                }
            }

            private MemoryStream DecryptionCipherMemoryStream()
            {
                MemoryStream stream;
                try
                {
                    stream = new MemoryStream(PrivateKey.Decrypt(EncryptedCipherAsBytes, RSAEncryptionPadding.OaepSHA1));
                }
                catch (CryptographicException)
                {
                    stream = new MemoryStream(PrivateKey.Decrypt(EncryptedCipherAsBytes, RSAEncryptionPadding.OaepSHA1));
                }
                return stream;
            }

            private byte[] ReadBytesFromStream(Stream stream, int bytesToRead = 0)
            {
                return LogUtility.ReadBytesFromStream(stream, bytesToRead);
            }

            void IDisposable.Dispose()
            {
                if (ownsCryptoProvider)
                {
                    CryptoProvider?.Dispose();
                    ownsCryptoProvider = false;
                }
            }
        }
    }
}
