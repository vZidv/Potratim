using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace src.MyExceptions
{
    public class ImageSaveFailException : Exception
    {
        public ImageSaveFailException() : base()
        {
        }

        public ImageSaveFailException(string? message) : base(message)
        {
        }

        public ImageSaveFailException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}