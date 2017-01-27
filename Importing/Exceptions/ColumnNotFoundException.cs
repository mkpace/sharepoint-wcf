using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Classes.Importing.Exceptions
{
    public class ColumnNotFoundException : Exception
    {
        public ColumnNotFoundException(string msg) : base(msg) { }
    }
}
