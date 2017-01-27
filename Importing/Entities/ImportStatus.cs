using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Importing.Entities
{
    /// <summary>
    /// Import Error entity to track any import errors
    /// </summary>
    [DataContract()]
    public class ImportStatus
    {
        [DataMember(Name = "exception")]
        public Exception Exception { get; set; }

        /// <summary>
        /// Count of new items created
        /// </summary>
        [DataMember(Name = "createdRowCount")]
        public int CreatedRowCount;
        /// <summary>
        /// Count of items updated
        /// </summary>
        [DataMember(Name = "updatedRowCount")]
        public int UpdatedRowCount;
        /// <summary>
        /// Count of rows skipped
        /// </summary>
        [DataMember(Name = "skippedRowCount")]
        public int SkippedRowCount;

        [DataMember(Name = "skippedColumns")]
        public List<string> SkippedColumns { get; set; }

        [DataMember(Name = "skippedColumnCount")]
        public int SkippedColumnCount { get { return this.SkippedColumns.Count; } set { } }

        [DataMember(Name = "totalRowCount")]
        public int TotalRowCount { get; set; }

        [DataMember(Name = "importedRowCount")]
        public int ImportedRowCount
        {
            get { return this.TotalRowCount - this.SkippedRowCount; }
            private set { }
        }

        [DataMember(Name = "messages")]
        public List<string> Messages { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }
        
        public ImportStatus()
        {
            // status can be set to success | warning | error
            // this value is used by the toastr directive on the client
            this.Status = "success";
            this.TotalRowCount = 0;
            this.SkippedRowCount = 0;
            this.SkippedColumnCount = 0;
            this.Messages = new List<string>();
            this.SkippedColumns = new List<string>();
        }

        public void SkippedRow(string errMsg)
        {
            this.Messages.Add(errMsg);
            this.SkippedRowCount++;
        }

    }
}
