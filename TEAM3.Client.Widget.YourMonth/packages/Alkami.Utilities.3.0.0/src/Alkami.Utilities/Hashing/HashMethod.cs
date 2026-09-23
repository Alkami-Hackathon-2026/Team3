namespace Alkami.Utilities.Hashing
{
    /// <summary>
    /// An enumeration of the keyed hash algorithms available to the Compute method.
    /// </summary>
    public enum HashMethod
    {
        /// <summary>
        /// MD5 Algorithm
        /// </summary>
        HMACMD5 = 0,

#if NETFRAMEWORK
        /// <summary>
        /// RIPEMD160 Algorithm
        /// </summary>
        HMACRIPEMD160 = 1,
#endif

        /// <summary>
        /// SHA1 Algorithm
        /// </summary>
        HMACSHA1 = 2,

        /// <summary>
        /// SHA256 Algorithm
        /// </summary>
        HMACSHA256 = 3,

        /// <summary>
        /// SHA384 Algorithm
        /// </summary>
        HMACSHA384 = 4,

        /// <summary>
        /// SHA512 Algorithm
        /// </summary>
        HMACSHA512 = 5
    }
}
