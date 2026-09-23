using System;
using Alkami.Data.Validations;

namespace Alkami.Exceptions
{
    internal class MicrosoftSqlExceptionParser : IAlkamiExceptionParser
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

            try
            {
                type = Type.GetType(
                        "Microsoft.Data.SqlClient.SqlException, Microsoft.Data.SqlClient, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5",
                        false);
            }
            catch (Exception)
            {
                //doing some pokemon catching just in case. An error should not be thrown.
                Console.WriteLine("Failed to load the Microsoft.Data.SqlClient.SqlException, this should not have happened.");
            }
            return type;
        }
    }
}
