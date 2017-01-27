using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Http
{
    /// <summary>
    /// Utility for handling JSON objects
    /// </summary>
    public class JSON
    {
        /// <summary>
        /// Extracts and converts the Base64 encoded payload data into 
        /// a usable JSON string to be deserialized into a domain object
        /// </summary>
        /// <param name="dataMsg"></param>
        /// <returns></returns>
        public static string GetPayload(Message dataMsg)
        {
            // message is base64 encoded between <Binary> tag
            Regex regex = new Regex("<Binary>(.*?)</Binary>");
            // extract base64 data from raw data
            Match match = regex.Match(dataMsg.ToString());
            // get the encoded json data
            Group group = match.Groups[1];
            // convert to byte array
            byte[] payload = Convert.FromBase64String(group.Value);
            // convert to string
            string data = Encoding.UTF8.GetString(payload);
            return data;
        }
    }
}
