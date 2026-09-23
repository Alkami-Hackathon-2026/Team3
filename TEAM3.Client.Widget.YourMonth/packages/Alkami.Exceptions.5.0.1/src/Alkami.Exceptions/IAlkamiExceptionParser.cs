using System;

namespace Alkami.Exceptions
{
    /// <summary>
    /// Allows you to handle an Alkami Exception
    /// </summary>
    public interface IAlkamiExceptionParser
    {
        AlkamiException Parse(Exception input);

        Type Handles { get; }
    }
}