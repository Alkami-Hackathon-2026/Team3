using System.Collections.Generic;

namespace Alkami.Data.Validations
{
    /// <summary>
    /// The <see cref="IValidate"/> interface allows for classes to declare their validation without
    /// requiring a concrete (non-generic) class.
    /// </summary>
    internal interface IValidate
    {
        /// <summary>
        /// Validates the current object producing a collection of <see cref="ValidationResult"/>s.
        /// </summary>
        /// <returns>A collection of <see cref="ValidationResult"/>s.</returns>
        List<ValidationResult> Validate();
    }
}