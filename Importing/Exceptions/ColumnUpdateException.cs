using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Classes.Importing.Exceptions
{
    public class ColumnUpdateException : Exception
    {
        public ColumnUpdateException(string msg, Exception innerException ) : base(msg, innerException) { }

    }

    public class RowError : Exception
    {
        public RowError(string msg) : base(msg) { }
    }
}
