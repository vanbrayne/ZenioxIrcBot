namespace ZenioxBot
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    /// <summary>
    /// The main program for the IRC bot
    /// </summary>
    internal class Program
    {
        internal static void Main(string[] args)
        {
            try
            {
                Commands.Register();

                //using (var serverUser = new ServerUser(
                //    ConfigurationManager.AppSettings.Get("HostName"),
                //    ConfigurationManager.AppSettings.Get("UserName"),
                //    ConfigurationManager.AppSettings.Get("TheWord"),
                //    ConfigurationManager.AppSettings.Get("NickName"),
                //    ConfigurationManager.AppSettings.Get("RealName"),
                //    int.Parse(ConfigurationManager.AppSettings.Get("MillisecondsBetweenCommands"))))
                //{
                //    serverUser.CommandPrefix = "+";
                //    using (var channel = new Channel(serverUser, ConfigurationManager.AppSettings.Get("ChannelName")))
                //    {
                //        channel.KickBots = true;

                var isRunning = true;
                while (isRunning)
                {
                    Console.Write("> ");
                    var line = Console.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    if (line.Length == 0)
                    {
                        continue;
                    }

                    var parts = line.Split(' ');
                    var command = parts[0].ToLower();
                    var parameters = parts.Skip(1).ToArray();

                    switch (command)
                    {
                        case "q":
                            isRunning = false;
                            break;
                        case "l":
                            //channel.Leave();
                            break;
                        case "t":
                            var result = Rest.Post(
     new Uri("http://www.pandorabots.com"),
     "/pandora/talk?botid=823a1209ae36baf3",
     new KeyValuePair<string, string>("botcust2", "9868c3a47e7aabb8"),
     new KeyValuePair<string, string>("input", WebUtility.UrlEncode(string.Join(" ", parameters))));

                            Console.WriteLine(Commands.GetBotAnswer(result));
                            break;
                    }
                }
                //    }
                //}
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fatal error {0}\r{1}", e.Message, e.StackTrace);
                throw;
            }
        }
    }
}
