using System.IO;
using System.IO.Compression;

namespace Alkami.Utilities.LegacyIdentity
{
    /// <summary>
    /// 
    /// </summary>
    internal static class CompressionUtility
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uncompressedData"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] uncompressedData)
        {
            using var compressedStream = new MemoryStream();
            using var compressor = new GZipStream(compressedStream, CompressionMode.Compress);

            compressor.Write(uncompressedData, 0, uncompressedData.Length);
            compressor.Close();

            return compressedStream.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compressedData"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] compressedData)
        {
            using var compressedStream = new MemoryStream(compressedData);
            using var decompressedStream = new MemoryStream();
            using var descompressor = new GZipStream(compressedStream, CompressionMode.Decompress);

            descompressor.CopyTo(decompressedStream);

            return decompressedStream.ToArray();
        }
    }
}
