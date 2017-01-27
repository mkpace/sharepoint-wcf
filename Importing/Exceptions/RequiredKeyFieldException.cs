using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Kingpin.WCF2.Classes.Importing
{
    public class RequiredKeyFieldException : Exception
    {
        public RequiredKeyFieldException(string msg) : base(msg) { }
        public RequiredKeyFieldException(string msg, Exception innerException) : base(msg, innerException) { }
    }
}
