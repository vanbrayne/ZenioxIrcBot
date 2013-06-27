namespace ZenioxBot
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// The main program for the IRC bot
    /// </summary>
    internal class Program
    {
        private static readonly int MillisecondsBetweenCommands = int.Parse(ConfigurationManager.AppSettings.Get("MillisecondsBetweenCommands") ?? @"1000");

        private static readonly string HostName = ConfigurationManager.AppSettings.Get("HostName") ?? @"irc.twitch.tv";

        private static readonly string UserName = ConfigurationManager.AppSettings.Get("UserName") ?? @"zenioxbot";

        private static readonly string RealName = ConfigurationManager.AppSettings.Get("RealName") ?? @"Zeniox Bot";

        private static readonly string NickName = ConfigurationManager.AppSettings.Get("NickName") ?? @"ZenioxBot";

        private static readonly string TheWord = ConfigurationManager.AppSettings.Get("TheWord");

        private static readonly string ChannelName = ConfigurationManager.AppSettings.Get("ChannelName") ?? @"#zenioxbot";

        internal static void Main(string[] args)
        {
            try
            {
                using (var serverUser = new ServerUser(HostName, UserName, TheWord, NickName, RealName, MillisecondsBetweenCommands))
                {
                    using (var channel = new Channel(serverUser, ChannelName))
                    {
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
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fatal error {0}\r{1}", e.Message, e.StackTrace);
                throw;
            }
        }
    }
}
