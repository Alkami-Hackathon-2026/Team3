using System;

namespace Alkami.Utilities.Rpc
{
    /// <summary>
    /// Contract for adding Converters to the <see cref="IAlkamiSerializationSurrogateProvider"/>
    /// </summary>
    public interface ISurrogateConverter
    {
        /// <summary>
        /// The type of the Surrogate that is used during conversion
        /// </summary>
        Type SurrogateType { get; }
        /// <summary>
        /// The type of the original object
        /// </summary>
        Type ContractType { get; }

        /// <summary>
        /// Converts an object of type <see cref="ContractType"/> to <see cref="SurrogateType"/>
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        object ConvertToSurrogate(object original);

        /// <summary>
        /// Converts an object of type <see cref="SurrogateType"/> to <see cref="ContractType"/>
        /// </summary>
        /// <param name="surrogate"></param>
        /// <returns></returns>
        object ConvertFromSurrogate(object surrogate);
    }
}
