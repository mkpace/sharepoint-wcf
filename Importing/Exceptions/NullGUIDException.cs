using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Classes.Importing.Exceptions
{
    public class NullGUIDException : Exception
    {
        public NullGUIDException(string msg) : base(msg) { }
    }
}
