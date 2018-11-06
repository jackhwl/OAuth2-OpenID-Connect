using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeManagementSystem.Concurrency
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException()
        {
            
        }

        public ConcurrencyException(string message) : base(message)
        {
            
        }
    }
}
