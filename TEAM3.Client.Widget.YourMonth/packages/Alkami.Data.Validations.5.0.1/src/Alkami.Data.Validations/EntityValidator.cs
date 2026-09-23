using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Alkami.Data.Validations
{
    public static class EntityValidator
    {
        private static readonly ConcurrentDictionary<Type, EntityValidatorImpl> Validators = new();

        public static void AddValidator<T>(EntityValidatorImpl<T> validatorImpl) where T : class
        {
            Validators[typeof(T)] = validatorImpl;
        }

        public static bool Validate(this object src, out List<ValidationResult> results)
        {
            results = new List<ValidationResult>();

            if (src == null)
                return true;

            var originalType = src.GetType();
            var type = originalType;
            var foundValidator = false;

            while ((type != null) && (type != typeof(object)))
            {
                EntityValidatorImpl vadliator;

                if (Validators.TryGetValue(type, out vadliator))
                {
                    foundValidator = true;
                    List<ValidationResult> innerResults;

                    Validators[type].Validate(src, out innerResults);

                    results.AddRange(innerResults);
                }

                type = type.BaseType;
            }

            var iValidate = src as IValidate;

            if (iValidate != null)
            {
                foundValidator = true;
                results.AddRange(iValidate.Validate());
            }

            if (!foundValidator)
            {
                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.Informational,
                    Message = string.Format("There is no validator for this type - {0}", originalType.Name),
                    Severity = Severity.Warning
                });
            }

            return results.All(x => (x.Severity != Severity.Error) && (x.Severity != Severity.Fatal));
        }
    }
}
