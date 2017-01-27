using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Exceptions
{
    public class NoDataException : Exception
    {
        public NoDataException(string message)
            : base(message) { }

        public NoDataException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
