using System;
using System.Collections.Generic;
using System.Text;

namespace EvoS.Framework.Exceptions
{
    public class RegisterGameException : Exception
    {
        public RegisterGameException(string message) : base(message)
        {
        }
    }
}
