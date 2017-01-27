using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Diagnostics;

namespace Amazon.Kingpin.WCF2.Http
{

    [DataContractAttribute(Name = "response")]
    public class Response<T>
    {
        [DataMember(Name = "elapsed")]
        public long Elapsed { get; set; }
        [DataMember(Name = "diagnostics", EmitDefaultValue = false)]
        public string Diagnostics { get; set; }
        [DataMember(Name = "count", EmitDefaultValue = false)]
        public int Count { get; set; }
        [DataMember(Name = "item", EmitDefaultValue = false)]
        public T Item { get; set; }
        [DataMember(Name = "items", EmitDefaultValue = false)]
        public List<T> Items { get; set; }

        public Response(T items, long elapsed)
        {
            this.Item = items;
            this.Elapsed = elapsed;
        }
        public Response(T items, string diagnostics)
        {
            this.Item = items;
            this.Diagnostics = diagnostics;
        }

        public Response(T item, KPTimer timer)
        {
            this.Item = item;
            this.Elapsed = timer.ElapsedMilliseconds();
        }
        public Response(List<T> items, KPTimer timer)
        {
            this.Items = items;
            this.Count = items.Count;
            this.Elapsed = timer.ElapsedMilliseconds();
        }
        public Response(List<T> items, string diagnostics)
        {
            this.Items = items;
            this.Count = items.Count;
            this.Diagnostics = diagnostics;
        }
    }
}
