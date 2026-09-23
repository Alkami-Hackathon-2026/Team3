using System;
using Alkami.Data.Validations;

namespace Alkami.Exceptions
{
    internal class NullReferenceExceptionParser : IAlkamiExceptionParser
    {
        private static readonly Type InnerType = typeof(NullReferenceException);

        public AlkamiException Parse(Exception input)
        {
            var nullReferenceException = input as NullReferenceException;
            var message = nullReferenceException != null ? nullReferenceException.Message : "Unhandled NullReferenceException";

            return new AlkamiException(input, ErrorCode.SystemNonFatalError, SubCode.None, message);
        }

        public Type Handles { get { return InnerType; } }
    }
}