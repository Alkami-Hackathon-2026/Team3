using System;
using System.Diagnostics.CodeAnalysis;

namespace Alkami.Utilities.Certificates.Models
{
    [ExcludeFromCodeCoverage]
    internal class AlkamiCertificate
    {
        /// <summary>
        /// Gets or sets the subject name of the certificate.
        /// </summary>
        public string? SubjectName { get; set; }

        /// <summary>
        /// Gets or sets the thumbprint of the certificate.
        /// </summary>
        public string? Thumbprint { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the certificate.
        /// </summary>
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the start date of the certificate.
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the certificate.
        /// </summary>
        public DateTime ExpirationDateTime { get; set; }

        /// <summary>
        /// Gets or sets the base64 encoding of the certificate.
        /// </summary>
        public string? Base64Encoded { get; set; }

        /// <summary>
        /// Gets or sets the passphrase of the certificate.
        /// </summary>
        public string? Passphrase { get; set; }

        /// <summary>
        /// Gets or sets the type of the certificate.
        /// </summary>
        public CertificateAuthorityType Type { get; set; }
    }

    internal enum CertificateAuthorityType
    {
        /// <summary>
        /// Represents an intermediate certificate, which is used to sign other certificates.
        /// </summary>
        Intermediate,

        /// <summary>
        /// Represents a root certificate, which is a top-level certificate in a certificate chain.
        /// </summary>
        Root,

        /// <summary>
        /// Represents a certificate that does not have an autority.
        /// </summary>
        None
    }
}
