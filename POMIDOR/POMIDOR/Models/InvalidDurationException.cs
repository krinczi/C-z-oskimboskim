using System;

namespace POMIDOR.Models
{
    public sealed class InvalidDurationException : Exception
    {
        public InvalidDurationException(string message) : base(message) { }
    }
}
