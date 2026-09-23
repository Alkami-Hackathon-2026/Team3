using System;
#if NETFRAMEWORK
using System.Data.SqlClient;
#endif
using Alkami.Data.Validations;

namespace Alkami.Exceptions
{
    internal class SqlExceptionParser : IAlkamiExceptionParser
    {
        private static readonly Type InnerType = GetSqlDataType();

        public AlkamiException Parse(Exception input)
        {
            var result = new AlkamiException(input, ErrorCode.Informational, SubCode.NoDataInRequest, input.Message);
            return result;
        }

        public Type Handles
        {
#pragma warning disable CS0612 // Type or member is obsolete
            get { return InnerType; }
#pragma warning restore CS0612 // Type or member is obsolete
        }

        private static Type GetSqlDataType()
        {
            Type type = null;
#if NETFRAMEWORK
            type = typeof(SqlException);
#else
            try
            {
                type = Type.GetType(
                        "System.Data.SqlClient.SqlException, System.Data.SqlClient, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                        false);
            }
            catch (Exception)
            {
                //doing some pokemon catching just in case. An error should not be thrown.
                Console.WriteLine("Failed to load the System.Data.SqlClient.SqlException, this should not have happened.");
            }
#endif
            return type;
        }
    }
}
