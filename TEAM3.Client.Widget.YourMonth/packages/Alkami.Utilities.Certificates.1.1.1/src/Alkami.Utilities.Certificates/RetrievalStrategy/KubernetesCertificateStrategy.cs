#if NET6_0_OR_GREATER
using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using Alkami.Utilities.Certificates.Http;
using Alkami.Utilities.Certificates.Logging;
using Alkami.Utilities.Certificates.Models;
using Alkami.Utilities.Json;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace Alkami.Utilities.Certificates.RetrievalStrategy
{
    /// <inheritdoc cref="ICertificateRetrievalStrategy"/>
    public class KubernetesCertificateStrategy : ICertificateRetrievalStrategy
    {
        private readonly ILoggingAdapter _logger;
        internal static string CertificateSocketFile = "/app/sock-drawer/certificates.sock";
        internal static int UdsRetryAttempts = 5;

        /// <summary>
        /// Obsolete constructor kept for backwards compatibility.
        /// </summary>
        /// <param name="certificateStore">Unused. Here for backwards compatibility</param>
        /// <param name="httpClientFactory">Unused. Here for backwards compatibility</param>
        [Obsolete("Please either use the default constructor or constructor with ILogger parameter.")]
        public KubernetesCertificateStrategy(ICertificateStore certificateStore, ICertificateHttpClientFactory httpClientFactory)
            : this()
        {
        }
        
        /// <summary>
        /// Accepts the Microsoft Logger.
        /// </summary>
        /// <param name="logger"></param>
        public KubernetesCertificateStrategy(ILogger<KubernetesCertificateStrategy> logger)
        {
            _logger = new MicrosoftLoggingAdapter(logger);
        }

        /// <summary>
        /// Default constructor. Uses Common.Logging
        /// </summary>
        public KubernetesCertificateStrategy()
        {
            _logger = new CommonLoggingAdapter(LogManager.GetLogger<KubernetesCertificateStrategy>());
        }

        /// <inheritdoc cref="ICertificateRetrievalStrategy"/>
        public bool IsSupportedFindType(X509FindType findType)
        {
            switch (findType)
            {
                case X509FindType.FindByThumbprint:
                case X509FindType.FindBySubjectName:
                    return true;
                default:
                    return false;
            }
        }

        /// <inheritdoc cref="ICertificateRetrievalStrategy"/>
        public async Task<X509Certificate2?> GetCertificate(StoreName storeName, StoreLocation location, X509FindType findType, string? findValue)
        {
            try
            {
                var query = new UDSCertificateQueryParameters
                {
                    FindValue = findValue,
                    StoreName = storeName,
                    StoreLocation = location,
                    X509FindType = findType
                };

                var certificate = await HandleUdsRequest(query);

                if (certificate == null || string.IsNullOrWhiteSpace(certificate.Base64Encoded))
                    return null;

                return new X509Certificate2(Convert.FromBase64String(certificate.Base64Encoded), certificate.Passphrase);
            }
            catch (CryptographicException ce)
            {
                var msg = $"Unable to create an X509Certificate2 object from the certificate service. When searching by {findType} with value {findValue}.";
                _logger.LogError(ce, $"{msg} : {ce.Message}");
                throw new Exception(msg, ce);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to get requested certificate {findValue} by {findType}. Reason {ex.Message}");
                throw;
            }
        }

        private protected virtual async Task<AlkamiCertificate?> HandleUdsRequest(UDSCertificateQueryParameters queryParameters)
        {
            var delay = 1000;
            var response = string.Empty;

            for (var attempt = 0; attempt < UdsRetryAttempts; attempt++)
            {
                try
                {
                    _logger.LogTrace($"Attempting to connect to sidecar via UDS at {CertificateSocketFile}");
                    using var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    var endpoint = new UnixDomainSocketEndPoint(CertificateSocketFile);

                    await client.ConnectAsync(endpoint);
                    await using var stream = new NetworkStream(client);
                    await using var writer = new StreamWriter(stream);
                    using var reader = new StreamReader(stream);

                    _logger.LogTrace("Certificate UDS Connection established. Serializing request and writing to socket...");
                    var queryJson = JsonSerializer.Serialize(queryParameters, AlkamiJsonSerializerOptions.Default);
                    await writer.WriteLineAsync(queryJson);
                    await writer.FlushAsync();

                    response = await reader.ReadToEndAsync();
                    _logger.LogTrace("Received certificate UDS response from sidecar.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug($"Failed to connect to sidecar via UDS. Attempt #{attempt}");
                    if (attempt == UdsRetryAttempts - 1)
                        throw new Exception($"Failed to connect to the Alkami Certificate Sidecar via a Unix Domain Socket: {ex.Message}", ex);

                    await Task.Delay(delay);
                    delay <<= 1;
                }
            }

            try
            {
                return JsonSerializer.Deserialize<AlkamiCertificate>(response, AlkamiJsonSerializerOptions.Default);
            }
            catch (JsonException)
            {
                //An error string may have been returned, throw that onto an exception.
                throw new Exception($"Failed to get a valid response from the certificate sidecar: {response}");
            }
        }
    }
}
#endif