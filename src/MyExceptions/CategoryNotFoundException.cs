using System;

namespace Potratim.MyExceptions
{
    public class CategoryNotFoundException : Exception
    {
        public CategoryNotFoundException() : base() { }
        
        public CategoryNotFoundException(string message) : base(message) { }
        
        public CategoryNotFoundException(string message, Exception innerException) 
            : base(message, innerException) { }
            
        public int CategoryId { get; set; }
    }
}