using System;

namespace Potratim.MyExceptions
{
    public class ValidationException : Exception
    {
        public ValidationException() : base() { }
        
        public ValidationException(string message) : base(message) { }
        
        public ValidationException(string message, Exception innerException) 
            : base(message, innerException) { }
            
        public string? PropertyName { get; set; }
        public object? AttemptedValue { get; set; }
    }
}