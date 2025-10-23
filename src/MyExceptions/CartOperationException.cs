using System;

namespace Potratim.MyExceptions
{
    public class CartOperationException : Exception
    {
        public CartOperationException() : base() { }
        
        public CartOperationException(string message) : base(message) { }
        
        public CartOperationException(string message, Exception innerException) 
            : base(message, innerException) { }
            
        public Guid? UserId { get; set; }
        public Guid? GameId { get; set; }
    }
}