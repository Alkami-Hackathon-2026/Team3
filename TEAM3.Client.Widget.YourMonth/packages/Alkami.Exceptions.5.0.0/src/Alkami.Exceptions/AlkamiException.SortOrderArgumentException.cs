using System;
using Alkami.Data.Validations;

namespace Alkami.Exceptions
{
    internal class SortOrderArgumentExceptionParser : IAlkamiExceptionParser
    {
        private static readonly Type InnerType = typeof(SortOrderArgumentException);

        public AlkamiException Parse(Exception input)
        {
            var sortOrderArgumentException = input as SortOrderArgumentException;
            var message = sortOrderArgumentException != null ? sortOrderArgumentException.Message : "Unhandled SortOrderArgumentException";

            return new AlkamiException(input, ErrorCode.SystemNonFatalError, SubCode.ValueUnsupported, message);
        }

        public Type Handles { get { return InnerType; } }
    }
}