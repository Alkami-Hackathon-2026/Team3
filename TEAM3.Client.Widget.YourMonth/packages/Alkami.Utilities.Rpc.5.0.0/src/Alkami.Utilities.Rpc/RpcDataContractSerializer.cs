using System;
using System.Runtime.Serialization;

namespace Alkami.Utilities.Rpc
{
    /// <summary>
    /// Exposes method for getting DataContractSerializer used in RPC communication
    /// </summary>
    public static class RpcDataContractSerializer
    {
        /// <summary>
        /// Retrieves default serializer for communication with containerized services.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static DataContractSerializer Get(Type objectType)
        {
            var serializer = new DataContractSerializer(objectType);
            serializer.SetSerializationSurrogateProvider(SerializationSurrogateProvider.GetDefault());
            return serializer;
        }
    }
}