using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Importing.Entities
{
    public class LookupField
    {
        public string ListName { get; set; }
        public string FieldName { get; set; }
        public string ForeignKeyField { get; set; }
    }
}
