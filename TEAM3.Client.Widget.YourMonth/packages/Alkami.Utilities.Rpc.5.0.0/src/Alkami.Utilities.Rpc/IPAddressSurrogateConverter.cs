using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;

namespace Alkami.Utilities.Rpc
{
    /// <summary>
    /// A Surrogate Converter for <see cref="IPAddress"/>
    /// </summary>
    public class IPAddressSurrogateConverter : ISurrogateConverter
    {
        /// <inheritdoc />
        public Type SurrogateType => typeof(IPAddressSurrogate);

        /// <inheritdoc />
        public Type ContractType => typeof(IPAddress);

        /// <inheritdoc />
        public object ConvertFromSurrogate(object obj)
        {
            if (obj is not IPAddressSurrogate surrogate)
            {
                return null;
            }

            if (surrogate.m_Family == AddressFamily.InterNetwork)
            {
                var result = new IPAddress(surrogate.m_Address);
                return result;
            }

            Type typ = typeof(IPAddress);
#if NET6_0_OR_GREATER
            FieldInfo typeNumbers = typ.GetField("_numbers", BindingFlags.NonPublic | BindingFlags.Instance);

            var resultV6 = new IPAddress(new byte[16], surrogate.m_ScopeId);
            typeNumbers.SetValue(resultV6, surrogate.m_Numbers ?? new ushort[8]);
            return resultV6;

#else
            FieldInfo typeNumbers = typ.GetField("m_Numbers", BindingFlags.NonPublic | BindingFlags.Instance);

            var resultV6 = new IPAddress(new byte[16], surrogate.m_ScopeId);
            typeNumbers.SetValue(resultV6, surrogate.m_Numbers);

            return resultV6;
#endif
        }

        /// <inheritdoc />
        public object ConvertToSurrogate(object obj)
        {
            if (obj is not IPAddress ipAddress)
            {
                return null;
            }

            Type typ = typeof(IPAddress);

#if NET6_0_OR_GREATER
            FieldInfo typeAddress = typ.GetField("_addressOrScopeId", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo typeNumbers = typ.GetField("_numbers", BindingFlags.NonPublic | BindingFlags.Instance);

            var numbers = (ushort[])typeNumbers.GetValue(ipAddress);
            if (numbers == null || numbers.Length == 0)
            {
                numbers = new ushort[8];
            }

            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                return new IPAddressSurrogate
                {
                    m_Address = (uint)typeAddress.GetValue(ipAddress),
                    m_Family = ipAddress.AddressFamily,
                    m_Numbers = numbers,
                    m_ScopeId = 0,
                    m_HashCode = 0, //hashcode is recalculated so do not need to actually 
                };
            }

            return new IPAddressSurrogate
            {
                m_Address = 0,
                m_Family = ipAddress.AddressFamily,
                m_Numbers = numbers,
                m_ScopeId = (uint)typeAddress.GetValue(ipAddress),
                m_HashCode = 0,  //hashcode is recalculated so do not need to actually
            };
#else
            FieldInfo typeAddress = typ.GetField("m_Address", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo typeNumbers = typ.GetField("m_Numbers", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo typeScope = typ.GetField("m_ScopeId", BindingFlags.NonPublic | BindingFlags.Instance);

            return new IPAddressSurrogate
            {
                m_Address = (long)typeAddress.GetValue(ipAddress),
                m_Family = ipAddress.AddressFamily,
                m_Numbers = (ushort[])typeNumbers.GetValue(ipAddress),
                m_ScopeId = (long)typeScope.GetValue(ipAddress),
                m_HashCode = 0,  //hashcode is recalculated so do not need to actually
            };
#endif

        }

    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/System.Net")]
    internal class IPAddressSurrogate
    {
        [DataMember(Name = "m_Address")]
        public long m_Address { get; set; }

        [DataMember(Name = "m_Family")]
        public AddressFamily m_Family { get; set; }

        [DataMember(Name = "m_Numbers")]
        public ushort[] m_Numbers { get; set; }

        [DataMember(Name = "m_ScopeId")]
        public long m_ScopeId { get; set; }

        [DataMember(Name = "m_HashCode")]
        public int m_HashCode { get; set; }
    }
}
