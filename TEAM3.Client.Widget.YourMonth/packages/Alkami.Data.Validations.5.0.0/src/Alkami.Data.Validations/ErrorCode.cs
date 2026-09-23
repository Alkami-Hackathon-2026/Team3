using System.Runtime.Serialization;

namespace Alkami.Data.Validations
{
    public enum ErrorCode : int
    {
        /// <summary>
        /// DO NOT USE!
        /// </summary>
        [EnumMember(Value = "0")]
        Unknown = 0,
        /// <summary>
        /// Informational – Some kind of information warning that should not stop processing.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("100")]
        [EnumMember(Value = "100")]
        Informational = 100,

        /// <summary>
        /// ValidationError – Data values were out of range based on acceptable values.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("200")]
        [EnumMember(Value = "200")]
        ValidationError = 200,

        /// <summary>
        /// DataRequestError – The data request was not well-formed.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("300")]
        [EnumMember(Value = "300")]
        DataRequestError = 300,

        /// <summary>
        /// UnsupportedError – The request is not supported by the server implementation.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("310")]
        [EnumMember(Value = "310")]
        UnsupportedError = 310,

        /// <summary>
        /// PermissionError – The client lacks the necessary security permission to access the object or operation.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("320")]
        [EnumMember(Value = "320")]
        PermissionError = 320,

        /// <summary>
        /// SystemNonFatalError – temporary system error occured such as a temporary system outage, etc.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("400")]
        [EnumMember(Value = "400")]
        SystemNonFatalError = 400,

        /// <summary>
        /// SystemFatalError – critical system error occurred such as full system outage, etc.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("500")]
        [EnumMember(Value = "500")]
        SystemFatalError = 500,
    }
}