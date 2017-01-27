using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Importing.Entities
{
    public class ImportConfig
    {
        public int TeamId { get; set; }
        public string FilePath { get; set; }
        public string ListName { get; set; }
        public string ImportType { get; set; }
    }
}
