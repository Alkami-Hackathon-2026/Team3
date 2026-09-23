using System;
using System.Collections.Generic;
using Alkami.Data.Validations;

namespace Alkami.Contracts
{
    public class BaseRequestValidator : EntityValidatorImpl<BaseRequest>
    {
        protected override List<ValidationResult> ValidateInternal(BaseRequest src)
        {
            var results = new List<ValidationResult>();

            if (src.BankIdentifier.GetValueOrDefault() == Guid.Empty)
            {
                if (String.IsNullOrWhiteSpace(src.BankUri))
                {
                    results.Add(new ValidationResult()
                    {
                        ErrorCode = ErrorCode.ValidationError,
                        Field = "BankIdentifier",
                        Severity = Severity.Fatal,
                        Message = string.Format("BankIdentifier must be a valid Guid or you must include a bankUri. You sent {0}.",
                            src.BankIdentifier.HasValue ? src.BankIdentifier.Value.ToString() : "<Null>")
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(src.BankUri))
            {
                Uri uri;
                if (!Uri.TryCreate(src.BankUri, UriKind.RelativeOrAbsolute, out uri))
                {
                    results.Add(new ValidationResult()
                    {
                        ErrorCode = ErrorCode.ValidationError,
                        Field = "BankUri",
                        Severity = Severity.Fatal,
                        Message = string.Format("BankUri must be a valid Uri. You sent {0}", src.BankUri)
                    });
                }
            }

            if (src.MaxResults == 0)
            {
                src.MaxResults = 100;
                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Field = "MaxResults",
                    Severity = Severity.Warning,
                    Message = "You should always send it a max result and not the default of Zero. It will be modified to 100"
                });
            }

            return results;
        }
    }
}