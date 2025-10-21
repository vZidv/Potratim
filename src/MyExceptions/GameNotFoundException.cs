using System;

namespace Potratim.MyExceptions
{
    public class GameNotFoundException : Exception
    {
        public GameNotFoundException() : base() { }
        
        public GameNotFoundException(string message) : base(message) { }
        
        public GameNotFoundException(string message, Exception innerException) 
            : base(message, innerException) { }
            
        public Guid GameId { get; set; }
    }
}