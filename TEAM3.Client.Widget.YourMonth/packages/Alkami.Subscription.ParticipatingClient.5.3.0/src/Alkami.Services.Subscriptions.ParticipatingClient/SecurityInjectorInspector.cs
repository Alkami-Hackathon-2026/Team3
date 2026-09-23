using Alkami.Contracts;
using Alkami.Monitoring;
using Alkami.Security;
using Alkami.Utilities.LegacyIdentity;
using Common.Logging;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    public class SecurityInjectorInspector : IParameterInspector
    {
        public static Func<ClaimsIdentity> LegacyShimFactory = _LegacyShimFactory;

        internal static ClaimsIdentity _LegacyShimFactory()
        {
            var claimsIdentity = Thread.CurrentPrincipal?.Identity as ClaimsIdentity;

            if (ClaimsUtility.IsAuthenticated(claimsIdentity))
                return claimsIdentity;
            else
                return null;
        }

        private static readonly ILog Logger = LogManager.GetLogger<SecurityInjectorInspector>();
        private static readonly DataContractSerializer Serializer;

        static SecurityInjectorInspector()
        {
            Serializer = new DataContractSerializer(typeof(ClaimsIdentity), new[] { typeof(GenericIdentity), typeof(WindowsIdentity) });
#if NET6_0_OR_GREATER
            Serializer.SetSerializationSurrogateProvider(Utilities.Rpc.SerializationSurrogateProvider.GetDefault());
#endif
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            Logger.Trace(t =>
            {
                if (correlationState is DateTime)
                {
                    t("{0} took {1} and returned {2}", operationName, DateTime.Now - (DateTime)correlationState, returnValue);
                }
            });
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            Logger.Trace(t => t("Request: {0}", inputs[0]));

            var input = inputs[0] as BaseRequest;

            if (input == null)
                throw new ArgumentNullException("inputs", "You can only send a single request object of type BaseRequest");
            if ((input.SerializedUserToken != null) && (input.SerializedUserToken.Length > 0))
                return DateTime.Now;

            var identity = input.ClaimsIdentity ?? LegacyShimFactory();

            if (identity != null)
            {
                try
                {
                    switch (input.ClaimsIdentitySerializationMethod)
                    {
                        default:
                        case ClaimsIdentitySerializationMethod.DataContractSerializer:
                            input.SerializedUserToken = SerializeViaDataContractSerializer(identity);
                            break;
                        case ClaimsIdentitySerializationMethod.Json:
                            input.SerializedUserToken = SerializeViaJson(identity);
                            break;
                        case ClaimsIdentitySerializationMethod.CompressedJson:
                            input.SerializedUserToken = SerializeViaCompressedJson(identity);
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Logger.Warn(w => w("An Error occurred trying to serialize the identity({0}) with the following: {1}", identity.GetType(), exception.ToString()));
                    Metric.NoticeError(exception);
                }
            }

            return DateTime.Now;
        }

        protected byte[] SerializeViaDataContractSerializer(ClaimsIdentity identity)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.WriteObject(ms, identity);
                return ms.ToArray();
            }
        }

        protected byte[] SerializeViaJson(ClaimsIdentity identity)
        {
            var serializer = new ClaimsIdentitySerializer();
            var str = serializer.ToJson(identity);

            return Encoding.UTF8.GetBytes(str);
        }

        protected byte[] SerializeViaCompressedJson(ClaimsIdentity identity)
        {
            var serializer = new ClaimsIdentitySerializer();

            return serializer.ToCompressed(identity);
        }
    }
}
