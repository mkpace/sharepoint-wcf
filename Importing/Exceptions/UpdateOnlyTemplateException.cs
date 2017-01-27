using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Kingpin.WCF2.Classes.Importing
{
    public class UpdateOnlyTemplateException : Exception
    {
        public UpdateOnlyTemplateException(string msg) : base(msg) { }
    }
}
