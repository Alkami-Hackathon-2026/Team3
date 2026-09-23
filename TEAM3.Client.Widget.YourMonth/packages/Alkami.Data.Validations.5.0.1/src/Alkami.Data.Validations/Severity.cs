using System.Runtime.Serialization;

namespace Alkami.Data.Validations
{
    [DataContract]
    public enum Severity
    {
        [EnumMember]
        Warning = 0,

        [EnumMember]
        Error,

        [EnumMember]
        Fatal
    }
}