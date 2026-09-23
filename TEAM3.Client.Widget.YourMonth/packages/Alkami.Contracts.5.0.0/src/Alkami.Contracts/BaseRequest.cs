using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;

[assembly: InternalsVisibleTo("Alkami.Extensions.Subscription.Wcf")]

namespace Alkami.Contracts
{
    [DataContract(IsReference = true)]
    public class BaseRequest
    {
        protected BaseRequest()
        {
            MessageIdentifier = Guid.NewGuid();
            // CorrelationId is generated if the provided base request doesn't have a CorrelationId
            CorrelationId = Guid.NewGuid().ToString();
        }

        [DataMember]
        public Guid? BankInstanceIdentifier { get; set; }

        [DataMember]
        public Guid? BankIdentifier { get; set; }

        [DataMember]
        public string BankUri { get; set; }

        [IgnoreDataMember]
        public ClaimsIdentity ClaimsIdentity
        {
            get { return this._claimsIdentity; }
            set
            {
                this._claimsIdentity = value;
                this.UpdateRequestForNewContext();
            }
        }

        [DataMember]
        public string CorrelationId { get; set; }

        [DataMember]
        public int MaxResults { get; set; }

        [DataMember]
        public Guid MessageIdentifier { get; private set; }

        [DataMember]
        public int Page { get; set; }

        [DataMember(EmitDefaultValue = true, Name = "sut")]
        internal byte[] SerializedUserToken { get; set; }

        [DataMember]
        public string SessionId { get; set; }

        [DataMember]
        public long? UserId { get; set; }

        [DataMember]
        public Guid? UserIdentifier { get; set; }

        [IgnoreDataMember]
        public ClaimsIdentitySerializationMethod ClaimsIdentitySerializationMethod
        {
            get
            {
                return (ClaimsIdentitySerializationMethod)this.ClaimsIdentitySerializationMethodValue.GetValueOrDefault((int)ClaimsIdentitySerializationMethod.DataContractSerializer);
            }
            set
            {
                var newValue = (int)value;

                if (this.ClaimsIdentitySerializationMethodValue != newValue)
                {
                    this.ClaimsIdentitySerializationMethodValue = newValue;
                    this.SerializedUserToken = null;
                }
            }
        }

        [DataMember]
        internal int? ClaimsIdentitySerializationMethodValue { get; set; }

        [IgnoreDataMember]
        internal ClaimsIdentity _claimsIdentity;

        public void CopyBaseFrom(BaseRequest requestToCopyFrom)
        {
            BankIdentifier = requestToCopyFrom.BankIdentifier;
            BankInstanceIdentifier = requestToCopyFrom.BankInstanceIdentifier;
            BankUri = requestToCopyFrom.BankUri;
            _claimsIdentity = requestToCopyFrom._claimsIdentity;
            if (!string.IsNullOrEmpty(requestToCopyFrom.CorrelationId))
                CorrelationId = requestToCopyFrom.CorrelationId;
            SerializedUserToken = requestToCopyFrom.SerializedUserToken;
            SessionId = requestToCopyFrom.SessionId;
            UserId = requestToCopyFrom.UserId;
            UserIdentifier = requestToCopyFrom.UserIdentifier;
            ClaimsIdentitySerializationMethodValue = requestToCopyFrom.ClaimsIdentitySerializationMethodValue;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("BaseRequest =>")
                .Append("CorrelationId: ").Append(CorrelationId).AppendLine()
                .Append("SessionId: ").Append(SessionId).AppendLine()
                .Append("MessageIdentifier: ").Append(MessageIdentifier).AppendLine()
                .Append("BankIdentifier: ").Append(BankIdentifier).AppendLine()
                .Append("UserIdentifier: ").Append(UserIdentifier).AppendLine()
                .Append("UserId: ").Append(UserId).AppendLine()
                .Append("BankUri: ").Append(BankUri).AppendLine()
                .Append("MaxResults: ").Append(MaxResults).AppendLine()
                .Append("Page: ").Append(Page).AppendLine()
                .Append("ClaimsIdentitySerializationMethod: ").Append(ClaimsIdentitySerializationMethod).AppendLine()
                .Append("TokenExists: ").Append((SerializedUserToken != null) && (SerializedUserToken.Length > 0)).AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Performs a member-wise clone of the request.
        /// </summary>
        /// <returns>A member-wise clone of the request.</returns>
        public object ShallowClone()
        {
            return this.MemberwiseClone();
        }

        private void UpdateRequestForNewContext()
        {
            this.SerializedUserToken = null;

            if ((this._claimsIdentity != null) && this._claimsIdentity.IsAuthenticated)
            {
                if (this.UserId.HasValue)
                    this.UserId = ParseLongClaim(this._claimsIdentity, "http://security.alkamitech.com/2011/07/claims/UserId");
                if (this.UserIdentifier.HasValue)
                    this.UserIdentifier = ParseGuidClaim(this._claimsIdentity, "http://security.alkamitech.com/2011/07/claims/UserIdentifier");
                if (this.BankIdentifier.HasValue)
                    this.BankIdentifier = ParseGuidClaim(this._claimsIdentity, "http://security.alkamitech.com/2011/07/claims/BankIdentifier");
                if (this.BankInstanceIdentifier.HasValue)
                    this.BankInstanceIdentifier = ParseGuidClaim(this._claimsIdentity, "http://security.alkamitech.com/2021/03/claims/BankInstanceIdentifier");
            }
            else
            {
                this.UserId = null;
                this.UserIdentifier = null;
            }
        }

        internal static long? ParseLongClaim(ClaimsIdentity claimsIdentity, string claimType)
        {
            var claimValue = claimsIdentity.FindFirst(claimType)?.Value;

            if (long.TryParse(claimValue, out var value))
                return value;
            else
                return null;
        }

        internal static Guid? ParseGuidClaim(ClaimsIdentity claimsIdentity, string claimType)
        {
            var claimValue = claimsIdentity.FindFirst(claimType)?.Value;

            if (Guid.TryParse(claimValue, out var value))
                return value;
            else
                return null;
        }
    }
}