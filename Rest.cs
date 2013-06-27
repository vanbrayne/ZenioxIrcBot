using System;
using System.Collections.Generic;

namespace ZenioxBot
{
    using System.Net;
    using System.Net.Http;

    public static class Rest
    {
        public static string Post2(Uri baseAddress, string path, FormUrlEncodedContent content)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = baseAddress;
                
                var result = client.PostAsync(path, content).Result;
                return result.Content.ReadAsStringAsync().Result;
            }
        }

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
    }
}
