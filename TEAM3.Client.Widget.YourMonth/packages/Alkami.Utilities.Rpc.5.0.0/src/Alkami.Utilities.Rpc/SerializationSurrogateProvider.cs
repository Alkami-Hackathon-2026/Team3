using System;
using System.Collections.Generic;

namespace Alkami.Utilities.Rpc
{
    /// <summary>
    /// Can be used in .NET 6 code that needs to serialize special types with DataContractSerializer
    /// </summary>
    public sealed class SerializationSurrogateProvider : IAlkamiSerializationSurrogateProvider
    {
        /// <summary>
        /// Gets a new instance of the Default <see cref="IAlkamiSerializationSurrogateProvider"/>
        /// </summary>
        public static IAlkamiSerializationSurrogateProvider GetDefault()
        {
            var value = new SerializationSurrogateProvider();
#if NET6_0_OR_GREATER
            value.AddSurrogateConverter(new IPAddressSurrogateConverter());
            value.AddSurrogateConverter(new ClaimsIdentitySurrogateConverter());
#endif
            return value;
        }

        private readonly Dictionary<Type, ISurrogateConverter> _surrogateConverters;

        /// <summary>
        /// Constructor
        /// </summary>
        public SerializationSurrogateProvider()
        {
            _surrogateConverters = new Dictionary<Type, ISurrogateConverter>();
        }

        /// <inheritdoc />
        public void AddSurrogateConverter(ISurrogateConverter surrogateConverter)
        {
            _surrogateConverters.Add(surrogateConverter.SurrogateType, surrogateConverter);
            _surrogateConverters.Add(surrogateConverter.ContractType, surrogateConverter);
        }

        /// <inheritdoc />
        public object GetDeserializedObject(object objectOverTheWire, Type targetType)
        {
            var surrogateConverter = GetSurrogateConverter(targetType);
            return surrogateConverter != null
                                ? surrogateConverter.ConvertFromSurrogate(objectOverTheWire)
                                : objectOverTheWire;
        }

        /// <inheritdoc />
        public object GetObjectToSerialize(object contractObj, Type targetType)
        {
            var surrogateConverter = GetSurrogateConverter(targetType);
            return surrogateConverter != null ? surrogateConverter.ConvertToSurrogate(contractObj)
                                             : contractObj;
        }

        /// <inheritdoc />
        public Type GetSurrogateType(Type type)
        {
            var surrogateConverter = GetSurrogateConverter(type);
            return surrogateConverter != null ? surrogateConverter.SurrogateType
                                             : type;
        }

        private ISurrogateConverter GetSurrogateConverter(Type typeToConvert)
        {
            _surrogateConverters.TryGetValue(typeToConvert, out var surrogateConverter);
            return surrogateConverter;
        }
    }
}
