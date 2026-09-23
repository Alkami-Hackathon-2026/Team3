using System.Runtime.Serialization;

namespace Alkami.Utilities.Rpc
{
    /// <summary>
    /// Extends the <see cref="ISerializationSurrogateProvider"/> to allow adding in Alkami built <see cref="ISurrogateConverter"/>s
    /// </summary>
    public interface IAlkamiSerializationSurrogateProvider : ISerializationSurrogateProvider
    {
        /// <summary>
        /// Adds a <paramref name="surrogateConverter"/> to the SerializationSurrogateProvider
        /// </summary>
        /// <param name="surrogateConverter"></param>
        void AddSurrogateConverter(ISurrogateConverter surrogateConverter);
    }
}
