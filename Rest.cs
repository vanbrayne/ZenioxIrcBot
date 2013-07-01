using System;
using System.Collections.Generic;

namespace ZenioxBot
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    public static class Rest
    {
        public static string Post(Uri baseAddress, string path, params KeyValuePair<string, string>[] keyValuePairs)
        {
            var content = new FormUrlEncodedContent(keyValuePairs);
            using (var client = new HttpClient())
            {
                client.BaseAddress = baseAddress;

                var result = client.PostAsync(path, content).Result;
                return result.Content.ReadAsStringAsync().Result;
            }
        }

        public static string Get(Uri baseAddress, string path, params KeyValuePair<string, string>[] keyValuePairs )
        {
            var question = path + "?";
            question += keyValuePairs.Select(p => p.Key + "=" + WebUtility.UrlEncode(p.Value)).Aggregate((current, p) => current + "&" + p);
            using (var client = new HttpClient())
            {
                client.BaseAddress = baseAddress;

                var result = client.GetAsync(question).Result;
                return result.Content.ReadAsStringAsync().Result;
            }
        }

        /// <summary>
        /// Find the interesting part of a message
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="start">The text that ends where the interesting part of the message starts.</param>
        /// <param name="end">The text starts where the interesting part of the message ends.</param>
        /// <returns></returns>
        public static string FindPart(string message, string start, string end)
        {
            string rest;
            return FindPart(message, start, end, out rest);
        }

        /// <summary>
        /// Find the interesting part of a message
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="startList">We will find each of these texts to work us up to the interesting part.
        /// The last of these strings will end where the interesting part starts.</param>
        /// <param name="end">The text starts where the interesting part of the message ends.</param>
        /// <returns></returns>
        public static string FindPart(string message, string[] startList, string end)
        {
            string rest;
            return FindPart(message, startList, end, out rest);
        }

        /// <summary>
        /// Find the interesting part of a message
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="startList">We will find each of these texts to work us up to the interesting part.
        /// The last of these strings will end where the interesting part starts.</param>
        /// <param name="end">The text starts where the interesting part of the message ends.</param>
        /// <param name="rest">Will return the rest of the message (from the start of <see cref="end"/>).</param>
        /// <returns></returns>
        public static string FindPart(string message, string[] startList, string end, out string rest)
        {
            var result = message;

            foreach (var start in startList)
            {
                result = FindPart(result, start, null);
                if (result == null)
                {
                    rest = message;
                    return null;
                }
            }

            result = FindPart(result, (string)null, end, out rest);
            return result;
        }

        /// <summary>
        /// Find the interesting part of a message
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="start">The text that ends where the interesting part of the message starts.</param>
        /// <param name="end">The text starts where the interesting part of the message ends.</param>
        /// <param name="rest">Will return the rest of the message (from the start of <see cref="end"/>).</param>
        /// <returns></returns>
        public static string FindPart(string message, string start, string end, out string rest)
        {
            int pos;

            var result = message;
            rest = null;

            if (!string.IsNullOrEmpty(start))
            {
                pos = result.IndexOf(start, StringComparison.Ordinal);
                if (pos < 0)
                {
                    rest = message;
                    return null;
                }

                result = result.Substring(pos + start.Length);
            }

            if (!string.IsNullOrEmpty(end))
            {
                pos = result.IndexOf(end, StringComparison.Ordinal);
                if (pos < 0)
                {
                    rest = message;
                    return null;
                }

                rest = result.Substring(pos);
                result = result.Substring(0, pos);
            }

            return result;
        }
    }
}
