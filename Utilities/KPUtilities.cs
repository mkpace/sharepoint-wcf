using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.DataPersistence.Helpers;

namespace Amazon.Kingpin.WCF2.Utilities
{
    public class KPUtilities
    {
        // match html tags
        private static Regex REGEX_HTML_TAGS = new Regex("<.*?>", RegexOptions.Compiled);

        /// <summary>
        /// SharePoint adds div tags and others in the middle of text that aren't friendly to reports. This removes them.
        /// </summary>
        /// <param name="strVal">The string to clean</param>
        /// <param name="removeFormatting">If true, bold and italics are removed</param>
        /// <returns>string.</returns>
        public static string StripHTML(string strVal, bool removeFormatting)
        {
            StringBuilder sb = new StringBuilder();
            int start;
            int end;
            string[] htmlTags = new string[] { "<div ", "<em ", "<p ", "<em ", "<br ", "<span " };

            if (!string.IsNullOrEmpty(strVal))
            {
                /* We need to check for attributes in the HTML tags that we wish to remove
                 * if there are attributes we need to remove them as well*/
                foreach (string tag in htmlTags)
                {
                    while (strVal.Contains(tag))
                    {
                        start = strVal.IndexOf(tag);
                        end = strVal.IndexOf(">", start + 1);
                        strVal = strVal.Substring(0, start) + strVal.Substring(end + 1);
                    }
                }

                // convert url encoded characters
                strVal = WebUtility.HtmlDecode(strVal);
                //sb.Replace("&amp;", "&");
                //sb.Replace("&#58;", ":");

                sb = new StringBuilder(strVal);
                if (sb.Length > 0)
                {
                    sb.Replace("<div>", string.Empty);
                    sb.Replace("</div>", string.Empty);
                    sb.Replace("<br />", string.Empty);
                    sb.Replace("<br>", string.Empty);
                    sb.Replace("<br/>", string.Empty);
                    sb.Replace("&nbsp;", " ");
                    sb.Replace("&#160;", " ");
                    sb.Replace(";#", string.Empty);
                    sb.Replace("<break>", string.Empty);
                    sb.Replace("<em>", string.Empty);
                    sb.Replace("</em>", string.Empty);
                    sb.Replace("<p>", string.Empty);
                    sb.Replace("</p>", string.Empty);
                    sb.Replace("</span>", string.Empty);

                    if (removeFormatting)
                    {
                        sb.Replace("<b>", string.Empty);
                        sb.Replace("</b>", string.Empty);
                        sb.Replace("<i>", string.Empty);
                        sb.Replace("</i>", string.Empty);
                        sb.Replace("<strong>", string.Empty);
                        sb.Replace("</strong>", string.Empty);
                        sb.Replace("<li>", string.Empty);
                        sb.Replace("</li>", string.Empty);
                        sb.Replace("<ol>", string.Empty);
                        sb.Replace("</ol>", string.Empty);
                        sb.Replace("<ul>", string.Empty);
                        sb.Replace("</ul>", string.Empty);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Handles converting string to int
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static int ParseInt(string strValue)
        {
            int value = -1;
            int.TryParse(strValue, out value);
            return value;
        }

        /// <summary>
        /// Handles converting string to DateTime (nullable)
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static DateTime? ParseDateTime(string strValue)
        {
            DateTime value;
            DateTime? nullableDate = (DateTime?)null;
            if (DateTime.TryParse(strValue, out value))
                nullableDate = (DateTime?)value;

            return nullableDate;
        }

        public static KPListItem UpdateItemFields(KPListItem updateItem, KPListItem originalItem)
        {
            KPListItem updatedItem = new KPListItem();
            string fieldValue = string.Empty;
            string updateValue = string.Empty;
            foreach (KeyValuePair<string, KPItem> kvp in originalItem)
            {
                if (kvp.Value.Type == EntityConstants.ItemTypes.NOTE)
                    // strip all characters
                    fieldValue = KPUtilities.StripHTML(kvp.Value.Value, false);
                else if (!string.IsNullOrEmpty(kvp.Value.Value) && kvp.Value.Type == EntityConstants.ItemTypes.DATETIME)
                    // convert to DateTime string in SP format
                    fieldValue = Convert.ToDateTime(kvp.Value.Value).ToString("yyyy-MM-dd");
                else
                    // plain old value - do nothing
                    fieldValue = kvp.Value.Value;

                // check for ignored fields
                if (!IgnoredFields(kvp.Key) && updateItem.ContainsKey(kvp.Key))
                {
                    if (kvp.Value.Type == EntityConstants.ItemTypes.NOTE)
                        updateValue = KPUtilities.StripHTML(updateItem[kvp.Key].Value, false);

                    else if (kvp.Value.Type == EntityConstants.ItemTypes.DATETIME)
                        updateValue = Convert.ToDateTime(updateItem[kvp.Key].Value).ToString("yyyy-MM-dd");

                    else
                        updateValue = updateItem[kvp.Key].Value;

                    // check if fields values are the same
                    if (updateValue != fieldValue)
                    {
                        kvp.Value.Value = updateValue;
                        // if changed then add to updatedItem fields
                        updatedItem.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            return updatedItem;
        }

        /// <summary>
        /// Handles converting values to correct
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string ConvertToSPType(KPItem item)
        {
            return ConvertToSPType(item.Value, item.Type);
        }

        /// <summary>
        /// Converts the value to a SharePoint data type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ConvertToSPType(string value, string type)
        {
            string returnValue = string.Empty;

            // need to parse different inputs
            DateTime date;

            switch (type)
            {
                case EntityConstants.ItemTypes.DATE:
                case EntityConstants.ItemTypes.DATETIME:
                    if (string.IsNullOrEmpty(value))
                        return null;
                    if (!DateTime.TryParse(value, out date))
                    {
                        double dateVal;
                        if (double.TryParse(value, out dateVal))
                            date = DateTime.FromOADate(dateVal);
                    }
                    // check for invalid date & bail
                    if (date.Equals(new DateTime()))
                        return null;

                    if (type.Equals(EntityConstants.ItemTypes.DATE))
                        returnValue = date.ToString("yyyy-MM-dd");
                    else
                        returnValue = date.ToString("o");

                    break;
                case EntityConstants.ItemTypes.NOTE:
                    returnValue = StripHTML(value, false);
                    returnValue = WebUtility.HtmlEncode(returnValue);
                    break;
                default:
                    returnValue = CleanSPValues(value);
                    break;
            }

            return returnValue;
        }

        internal static string CleanSPValues(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                // clean any SP type delimiters
                return Regex.Replace(value, @";#[0-9]+", "");
            }
            else
            {
                return null;
            }
            
        }

        /// <summary>
        /// Simple HTML Tag stripper
        /// TODO: replace this with the other version or make more robust
        /// </summary>
        /// <param name="itemValue"></param>
        /// <returns></returns>
        internal static string StripHtmlTags(string itemValue)
        {
            return REGEX_HTML_TAGS.Replace(itemValue, string.Empty);
        }

        /// <summary>
        /// Checks for fields that should be ignored when updating an item
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected static bool IgnoredFields(string fieldName)
        {
            List<string> ignoredFields = new List<string>() { 
                "KPID", "KPGUID", "Created", "CreatedBy", "Modified", "ModifiedBy"
            };
            return ignoredFields.Contains(fieldName);
        }
    }
}
