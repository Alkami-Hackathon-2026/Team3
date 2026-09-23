using System;
using System.Collections.Concurrent;
using Alkami.Data.Validations;

namespace Alkami.Exceptions
{
    public partial class AlkamiException : Exception
    {
        public ErrorCode ErrorCode { get; private set; }

        public SubCode SubCode { get; private set; }

        static AlkamiException()
        {
            //register the ones we know about
            RegisterHandler(new ArgumentExceptionParser());
            RegisterHandler(new NullReferenceExceptionParser());
            AddParserOnlyIfNotNull(new SqlExceptionParser());
            AddParserOnlyIfNotNull(new MicrosoftSqlExceptionParser());
            RegisterHandler(new AggregateExceptionParser());
            RegisterHandler(new SortOrderArgumentExceptionParser());
        }

        private static void AddParserOnlyIfNotNull(IAlkamiExceptionParser parser)
        {
            if (parser.Handles != null)
            {
                RegisterHandler(parser);
            }
        }

        public static void Initialize()
        {
        }

        public static AlkamiException Parse(Exception innerException, bool auditError = false)
        {
            IAlkamiExceptionParser handler;
            AlkamiException resultingException;

            var exceptionType = innerException.GetType();

            do
            {
                if (Handlers.TryGetValue(exceptionType, out handler))
                    break;

                exceptionType = exceptionType.BaseType;
            }
            while (exceptionType != null);

            if (handler != null)
                resultingException = handler.Parse(innerException);
            else
                resultingException = new AlkamiException(innerException);

            return resultingException;
        }

        public AlkamiException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }

        public AlkamiException(Exception innerException, ErrorCode errorCode, SubCode subCode, string message)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            SubCode = subCode;
            CustomizableMessage = message;
        }

        private string CustomizableMessage { get; set; }

        private static readonly ConcurrentDictionary<Type, IAlkamiExceptionParser> Handlers =
            new ConcurrentDictionary<Type, IAlkamiExceptionParser>();

        public static void RegisterHandler(IAlkamiExceptionParser parser)
        {
            Handlers[parser.Handles] = parser;
        }

        public override string Message
        {
            get
            {
                return string.IsNullOrWhiteSpace(CustomizableMessage) ? base.Message : CustomizableMessage;
            }
        }
    }
}
