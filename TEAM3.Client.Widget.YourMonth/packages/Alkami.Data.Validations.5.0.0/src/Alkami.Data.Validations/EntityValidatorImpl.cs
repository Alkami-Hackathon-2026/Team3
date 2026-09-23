using System;
using System.Collections.Generic;
using System.Linq;

namespace Alkami.Data.Validations
{
    public abstract class EntityValidatorImpl
    {
        public Type Type { get; internal set; }

        public bool Validate(object src, out List<ValidationResult> results)
        {
            return ValidateTypedInternal(src, out results);
        }

        protected abstract bool ValidateTypedInternal(object src, out List<ValidationResult> results);
    }

    public abstract class EntityValidatorImpl<T> : EntityValidatorImpl where T : class
    {
        protected EntityValidatorImpl()
        {
            Type = typeof(T);
        }

        public bool Validate(T src, out List<ValidationResult> results)
        {
            results = ValidateInternal(src);

            return results.All(x => (x.Severity != Severity.Error) && (x.Severity != Severity.Fatal));
        }

        protected override bool ValidateTypedInternal(object src, out List<ValidationResult> results)
        {
            var boxed = src as T;

            if (boxed == null)
            {
                results = new List<ValidationResult>()
                {
                    new ValidationResult()
                    {
                        ErrorCode = ErrorCode.ValidationError,
                        Severity = Severity.Warning,
                        Message = string.Format("Could not convert object to {0}", Type.Name)
                    }
                };
            }
            else
            {
                results = ValidateInternal(boxed);
            }

            return results.All(x => (x.Severity != Severity.Error) && (x.Severity != Severity.Fatal));
        }

        protected abstract List<ValidationResult> ValidateInternal(T src);
    }
}