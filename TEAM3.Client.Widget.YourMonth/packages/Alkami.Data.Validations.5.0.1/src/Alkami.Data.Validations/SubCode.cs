using System.Runtime.Serialization;

namespace Alkami.Data.Validations
{
    public enum SubCode : int
    {
        /// <summary>
        /// Default.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        [EnumMember(Value = "0")]
        None = 0,

        /// <summary>
        /// Custom subcode. True subcode value is in the CustomSubCode.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        [EnumMember(Value = "1")]
        Custom = 1,

        /// <summary>
        /// No data in request.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("300")]
        [EnumMember(Value = "300")]
        NoDataInRequest = 300,

        /// <summary>
        /// No matching records were found.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("302")]
        [EnumMember(Value = "302")]
        NoRecordsFound = 302,

        /// <summary>
        /// Bad request.  Data in the request was invalid.
        /// This error code should only be used if there is not a more detailed
        /// error listed below that matches the situation.  Any use of this error
        /// code should be communicated to the CUFX team so that the condition
        /// can be covered in future versions of the error object.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("400")]
        [EnumMember(Value = "400")]
        BadRequest = 400,

        /// <summary>
        /// Login Invalid.  The user token or credentials were invalid.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("401")]
        [EnumMember(Value = "401")]
        LoginInvalid = 401,

        /// <summary>
        /// Session token expired. The provided session token was expired or otherwise invalid.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("402")]
        [EnumMember(Value = "402")]
        SessionTokenExpired = 402,

        /// <summary>
        /// User token expired. The provided user token expired.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("403")]
        [EnumMember(Value = "403")]
        UserTokenExpired = 403,

        /// <summary>
        /// Conflict.  The entity submitted for creation already exists.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("409")]
        [EnumMember(Value = "409")]
        SubCode409 = 409,

        /// <summary>
        /// Incomplete request.  The request omitted the following required fields: '%s'
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("411")]
        [EnumMember(Value = "411")]
        IncompleteRequest = 411,

        /// <summary>
        /// Previous request required.  A '%s' request must be made before this one, and was not.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("412")]
        [EnumMember(Value = "412")]
        PreviousRequestRequired = 412,

        /// <summary>
        /// Value out of acceptable range.  The value ‘%s’ is not supported for the field ‘%s’ by this
        /// service. Valid values must be between ‘%s’ and ‘%s’.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("413")]
        [EnumMember(Value = "413")]
        ValueOutOfRange = 413,

        /// <summary>
        /// Unsupported value.  The value ‘%s’ is not supported for the field ‘%s’ by this service. Valid
        /// values include '%s'.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("415")]
        [EnumMember(Value = "415")]
        ValueUnsupported = 415,

        /// <summary>
        /// Unprocessable entity.  The request contained the following references to entities that could not be found: '%s'.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("422")]
        [EnumMember(Value = "422")]
        UnprocessableEntity = 422,

        /// <summary>
        /// Account locked.  The login was valid but the account was disabled, locked or otherwise inaccessible.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("423")]
        [EnumMember(Value = "423")]
        AccountLocked = 423,

        /// <summary>
        /// MFA login failed.  The MFA login information provided was incorrect.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("424")]
        [EnumMember(Value = "424")]
        MfaLoginFailed = 424,

        /// <summary>
        /// MAC invalid.  The MAC was invalid or missing.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("425")]
        [EnumMember(Value = "425")]
        MacInvalid = 425,

        /// <summary>
        /// Encryption error.  The provided encrypted data could not be decrypted.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("426")]
        [EnumMember(Value = "426")]
        EncryptionError = 426,

        /// <summary>
        /// Too many requests.  The user has sent too many requests in a given amount of time.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("429")]
        [EnumMember(Value = "429")]
        TooManyRequests = 429,

        /// <summary>
        /// Invalid language.  The requested language '%s' is not supported.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("430")]
        [EnumMember(Value = "430")]
        InvalidLanguage = 430,

        /// <summary>
        /// Invalid email format.  The format of the email was invalid.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("431")]
        [EnumMember(Value = "431")]
        InvalidEmailFormat = 431,

        /// <summary>
        /// Invalid phone format.  The format of the phone was invalid.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("432")]
        [EnumMember(Value = "432")]
        InvalidPhoneFormat = 432,

        /// <summary>
        /// Invalid data source ID.  The data source ID was not recognized.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("433")]
        [EnumMember(Value = "433")]
        InvalidDataSourceId = 433,

        /// <summary>
        /// Invalid FI ID.  The financial institution ID was not recognized.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("434")]
        [EnumMember(Value = "434")]
        InvalidFinancialInstitutionId = 434,

        /// <summary>
        /// Unable to parse request. Invalid JSON/XML.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("435")]
        [EnumMember(Value = "435")]
        MalformedRequest = 435,

        /// <summary>
        /// Access denied.  Access to resource requested was denied.	subCode may contain additional details.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("436")]
        [EnumMember(Value = "436")]
        AccessDenied = 436,

        /// <summary>
        /// Artifact not found.	The artifact ID could not be found in the repository.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("437")]
        [EnumMember(Value = "437")]
        ArtifactNotFound = 437,

        /// <summary>
        /// Update to field not allowed.	The repository does not allow updates to '%s'.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("438")]
        [EnumMember(Value = "438")]
        UpdateToFieldNotAllowed = 438,

        /// <summary>
        /// Artifact could not be decompressed.	 artifactCompressionType did not result in a successful
        ///decompression of the artifact.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("439")]
        [EnumMember(Value = "439")]
        ArtifactCannotBeDecompressed = 439,

        /// <summary>
        /// MIME type not supported. 	Repository rejected the MIME type.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("440")]
        [EnumMember(Value = "440")]
        MimeTypeNotSupported = 440,

        /// <summary>
        /// Artifact has been archived.	The artifact with the given artifact ID has been archived out of the repository.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("441")]
        [EnumMember(Value = "441")]
        ArtifactArchived = 441,

        /// <summary>
        /// Artifact too large.	The artifact was rejected because the artifact has exceeded the size limit.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("442")]
        [EnumMember(Value = "442")]
        ArtifactTooLarge = 442,

        /// <summary>
        /// Artifact too small.	The artifact was rejected because the artifact is smaller than its minimum size limit.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("443")]
        [EnumMember(Value = "443")]
        ArtifactTooSmall = 443,

        /// <summary>
        /// Artifact rejected. 	The repository has rejected the artifact. See the sub error codes for the specific reason.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("444")]
        [EnumMember(Value = "444")]
        ArtifactRejected = 444,

        /// <summary>
        /// Invalid data length in field '%s'.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("445")]
        [EnumMember(Value = "445")]
        InvalidDataLength = 445,

        /// <summary>
        /// Invalid format.  The format of the request is not supported.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("475")]
        [EnumMember(Value = "475")]
        InvalidFormat = 475,

        /// <summary>
        /// Invalid environment.  The environment specified in MessageContext is disallowed by the system as configured.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("476")]
        [EnumMember(Value = "476")]
        InvalidEnvironment = 476,

        /// <summary>
        /// Transaction dates are out of order.  The transaction end date is before the transaction start date
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("497")]
        [EnumMember(Value = "497")]
        DatesOutOfOrder = 497,

        /// <summary>
        /// Transaction date range too wide.  Date range is too wide for the data source to handle.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("498")]
        [EnumMember(Value = "498")]
        DateRangeTooLarge = 498,

        /// <summary>
        /// General Error.  Review subCode for more information.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("499")]
        [EnumMember(Value = "499")]
        GeneralError = 499,

        /// <summary>
        /// Service is temporarily unavailable.  Try again later.
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute("503")]
        [EnumMember(Value = "503")]
        ServiceTemporarilyUnavailable = 503,
    }
}