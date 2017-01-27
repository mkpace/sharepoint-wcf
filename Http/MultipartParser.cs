using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amazon.Kingpin.WCF2.Http
{
    /// <summary>
    /// Multipart stream parser for reading multipart mime types
    /// </summary>
    public class MultipartParser
    {
        /// <summary>
        /// success flag for reading stream
        /// </summary>
        public bool Success { get; private set; }
        /// <summary>
        /// Content type of input stream
        /// </summary>
        public string ContentType { get; private set; }
        /// <summary>
        /// Name of the file
        /// </summary>
        public string Filename { get; private set; }
        /// <summary>
        /// File bytes from stream
        /// </summary>
        public byte[] FileContents { get; private set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="stream"></param>
        public MultipartParser(Stream stream)
        {
            this.Parse(stream, Encoding.UTF8);
        }

        /// <summary>
        /// Ctor with encoding override
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        public MultipartParser(Stream stream, Encoding encoding)
        {
            this.Parse(stream, encoding);
        }

        /// <summary>
        /// Parse the incoming multipart stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        private void Parse(Stream stream, Encoding encoding)
        {
            this.Success = false;

            // Read the stream into a byte array
            byte[] data = this.ToByteArray(stream);

            // Copy to a string for header parsing
            string content = encoding.GetString(data);

            // The first line should contain the delimiter
            int delimiterEndIndex = content.IndexOf("\r\n");

            if (delimiterEndIndex > -1)
            {
                // Find the next delimiter
                string delimiter = content.Substring(0, content.IndexOf("\r\n"));

                // Look for Content-Type block
                Regex re = new Regex(@"(?<=Content\-Type:)(.*?)(?=\r\n\r\n)");
                Match contentTypeMatch = re.Match(content);

                // Look for filename
                re = new Regex(@"(?<=filename\=\"")(.*?)(?=\"")");
                Match filenameMatch = re.Match(content);

                // Did we find the required values?
                if (contentTypeMatch.Success && filenameMatch.Success)
                {
                    // Set properties
                    this.ContentType = contentTypeMatch.Value.Trim();
                    this.Filename = filenameMatch.Value.Trim();

                    // Get the start & end indexes of the file contents
                    int startIndex = contentTypeMatch.Index + contentTypeMatch.Length + "\r\n\r\n".Length;

                    byte[] delimiterBytes = encoding.GetBytes("\r\n" + delimiter);
                    int endIndex = IndexOf(data, delimiterBytes, startIndex);

                    int contentLength = endIndex - startIndex;

                    // Extract the file contents from the byte array
                    byte[] fileData = new byte[contentLength];

                    Buffer.BlockCopy(data, startIndex, fileData, 0, contentLength);

                    this.FileContents = fileData;
                    this.Success = true;
                }
            }
        }

        /// <summary>
        /// Get the index of the bytes
        /// </summary>
        /// <param name="searchWithin"></param>
        /// <param name="serachFor"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private int IndexOf(byte[] searchWithin, byte[] serachFor, int startIndex)
        {
            int index = 0;
            int startPos = Array.IndexOf(searchWithin, serachFor[0], startIndex);

            if (startPos != -1)
            {
                while ((startPos + index) < searchWithin.Length)
                {
                    if (searchWithin[startPos + index] == serachFor[index])
                    {
                        index++;
                        if (index == serachFor.Length)
                        {
                            return startPos;
                        }
                    }
                    else
                    {
                        startPos = Array.IndexOf<byte>(searchWithin, serachFor[0], startPos + index);
                        if (startPos == -1)
                        {
                            return -1;
                        }
                        index = 0;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Convert stream to byte array
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private byte[] ToByteArray(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

    }
}
