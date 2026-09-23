namespace Alkami.Data.Validations
{
    /// <summary>
    /// The type of comparison to use for a validation check.
    /// </summary>
    public enum Comparison
    {
        /// <summary>
        /// The values are equal.
        /// </summary>
        Equal,

        /// <summary>
        /// The left value is greater than the right.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// The left value is greater than or equal to the right.
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// The left value is less than the right.
        /// </summary>
        LessThan,

        /// <summary>
        /// The left value is less than or equal to the right.
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// The left value is not equal to the right.
        /// </summary>
        NotEqual,
    }
}