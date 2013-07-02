using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenioxBot
{
    using System.Diagnostics;
    using System.Globalization;

    public static class Services
    {
        public static string DetectLanguage(string message, bool onlyReliable = false, double minConfidence = 0.0)
        {
            try
            {
                var reply = Rest.Get(
                    new Uri("http://ws.detectlanguage.com"),
                    "/0.2/detect",
                    new KeyValuePair<string, string>("q", message),
                    new KeyValuePair<string, string>("key", "5b67314ed2a1dd0f5a1844d7d99a8add"));

                string rest;
                var language = Rest.FindPart(reply, "language\":\"", "\"", out rest);
                if (null == language)
                {
                    return null;
                }

                var isReliable = Rest.FindPart(rest, "isReliable\":", ",", out rest);
                if (onlyReliable && (isReliable != null) && !bool.Parse(isReliable))
                {
                    return null;
                }

                var confidence = Rest.FindPart(rest, "confidence\":", "}");
                if ((confidence != null) && double.Parse(confidence, new CultureInfo("en-US")) < minConfidence)
                {
                    return null;
                }

                return language;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Failed to detect language for {0}: {1}", message, ex.Message));
                return null;
            }
        }

        public static string Translate(string fromLanguage, string toLanguage, string message)
        {
            try
            {
                var reply = Rest.Get(
                    new Uri("http://api.mymemory.translated.net"),
                    "/get",
                    new KeyValuePair<string, string>("q", message),
                    new KeyValuePair<string, string>("langpair", string.Join("|", fromLanguage, toLanguage)));

                var result = Rest.FindPart(reply, "translatedText\":\"", "\"}");

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
