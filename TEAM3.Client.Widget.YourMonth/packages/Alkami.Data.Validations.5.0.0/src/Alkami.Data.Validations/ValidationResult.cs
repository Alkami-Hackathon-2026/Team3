using System.Runtime.Serialization;

namespace Alkami.Data.Validations
{
    [DataContract(IsReference = true)]
    public class ValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        public ValidationResult()
        {
            this.ItemIndex = -1;
        }

        [DataMember]
        public Severity Severity { get; set; }

        [DataMember]
        public string Field { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public ErrorCode ErrorCode { get; set; }

        [DataMember]
        public SubCode SubCode { get; set; }

        [DataMember]
        public string CustomSubCode { get; set; }

        [DataMember]
        public int ItemIndex { get; set; }

        [DataMember]
        public string Origin { get; set; }

        public override string ToString()
        {
            return string.Format("Severity: {0}, ErrorCode: {1}, SubCode: {2} - {3}{4}On Field [{5}] --- Message: {6}",
                Severity,
                ErrorCode,
                SubCode == SubCode.Custom ? $"(Custom) {CustomSubCode}" : SubCode.ToString(),
                (this.ItemIndex > -1) ? string.Format("ItemIndex: {0}, ", this.ItemIndex) : string.Empty,
                (!string.IsNullOrWhiteSpace(this.Origin)) ? string.Format("Origin: {0}, ", this.Origin) : string.Empty,
                Field,
                Message);
        }
    }
}