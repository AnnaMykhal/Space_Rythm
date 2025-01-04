using System;

namespace SpaceRythm.Exceptions;

public class AppException : Exception
{
    public AppException() : base() { }

    public AppException(string message) : base(message) { }

    public AppException(string message, Exception inner) : base(message, inner) { }
}
