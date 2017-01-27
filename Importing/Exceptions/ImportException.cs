using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Classes.Importing
{
    public class ImportException : Exception
    {
        public ImportException(string msg) : base(msg) { }
    }
}
