using System;
using Alkami.Data.Validations;

namespace Alkami.Exceptions
{
    internal class ArgumentExceptionParser : IAlkamiExceptionParser
    {
        private static readonly Type InnerType = typeof(ArgumentException);

        public AlkamiException Parse(Exception input)
        {
            var argumentException = input as ArgumentException;
            string message;

            if (argumentException != null)
                message = string.Format("Param: {0}, Message: {1}", argumentException.ParamName, argumentException.Message);
            else
                message = "Unhandled ArgumentException";

            return new AlkamiException(input, ErrorCode.SystemNonFatalError, SubCode.GeneralError, message);
        }

        public Type Handles { get { return InnerType; } }
    }
}