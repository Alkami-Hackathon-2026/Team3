using System;
using System.Linq;
using Alkami.Data.Validations;

namespace Alkami.Exceptions
{
    internal class AggregateExceptionParser : IAlkamiExceptionParser
    {
        private static readonly Type InnerType = typeof(AggregateException);

        public AlkamiException Parse(Exception input)
        {
            var aggregateException = input as AggregateException;

            var parsedExceptions =
                aggregateException.InnerExceptions.Select(x => AlkamiException.Parse(x))
                .ToArray();

            var combinedMessages = string.Join(Environment.NewLine, parsedExceptions.Select((x, i) => string.Format("{0} - ErrorCode: [{1}], SubCode: [{2}], Message: [{3}]", i, x.ErrorCode, x.SubCode, x.Message)));

            return new AlkamiException(input, ErrorCode.SystemFatalError, SubCode.GeneralError, combinedMessages);
        }

        public Type Handles { get { return InnerType; } }
    }
}