using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Alkami.Data.Validations;

namespace Alkami.Contracts
{
    [DataContract(IsReference = true)]
    public class BaseResponse
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BaseResponse"/> class.
        /// </summary>
        public BaseResponse()
        {
            this.ValidationResults = new List<ValidationResult>();
        }

        [DataMember]
        public TimeSpan Elapsed { get; private set; }

        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public Guid MessageIdentifier { get; private set; }

        [DataMember]
        public string CorrelationId { get; private set; }

        [DataMember]
        public int Page { get; set; }

        [DataMember]
        public int TotalResults { get; set; }

        [DataMember]
        public string Server { get; set; }

        [DataMember]
        public int ProcessId { get; set; }

        [DataMember]
        public string SystemMessage { get; set; }

        [DataMember]
        public List<ValidationResult> ValidationResults { get; set; }

        public void CorrelateWithRequest(BaseRequest request, TimeSpan elapsed)
        {
            this.CorrelationId = request.CorrelationId;
            this.MessageIdentifier = request.MessageIdentifier;
            this.Elapsed = elapsed;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("BaseResponse =>")
                .Append("Elapsed: ").Append(Elapsed).AppendLine()
                .Append("HasError: ").Append(HasError).AppendLine()
                .Append("CorrelationId: ").Append(CorrelationId).AppendLine()
                .Append("MessageIdentifier: ").Append(MessageIdentifier).AppendLine()
                .Append("Page: ").Append(Page).AppendLine()
                .Append("TotalResults: ").Append(TotalResults).AppendLine()
                .Append("Server: ").Append(Server).AppendLine()
                .Append("ProcessId: ").Append(ProcessId).AppendLine()
                .Append("SystemMessage: ").Append(SystemMessage).AppendLine();

            if (this.ValidationResults != null)
            {
                for (var i = 0; i < this.ValidationResults.Count; ++i)
                {
                    var vr = this.ValidationResults[i];

                    sb.Append("ValidationResult[").Append(i).Append("] - ").AppendLine(vr.ToString());
                }
            }

            return sb.ToString();
        }

        public void AddValidationResultsFrom<T>(params T[] childResponses) where T : BaseResponse
        {
            if (this.ValidationResults == null)
                this.ValidationResults = new List<ValidationResult>();

            foreach (var childResponse in childResponses)
            {
                if (childResponse != null)
                {
                    this.HasError |= childResponse.HasError;

                    if (childResponse.ValidationResults != null)
                        this.ValidationResults.AddRange(childResponse.ValidationResults);
                }
            }
        }
    }

    [DataContract(IsReference = true)]
    public class BaseResponse<T> : BaseResponse
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BaseResponse{T}"/> class.
        /// </summary>
        public BaseResponse()
        {
            this.ItemList = new List<T>();
        }

        [DataMember]
        public List<T> ItemList { get; set; }

        public override string ToString()
        {
            return string.Format("{0}{1}Values: {2}", base.ToString(), Environment.NewLine, string.Join(",", ItemList));
        }
    }
}