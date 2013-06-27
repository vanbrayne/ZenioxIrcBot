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
        internal static void Main(string[] args)
        {
            try
            {
                using (var serverUser = new ServerUser(
                    ConfigurationManager.AppSettings.Get("HostName"),
                    ConfigurationManager.AppSettings.Get("UserName"),
                    ConfigurationManager.AppSettings.Get("TheWord"),
                    ConfigurationManager.AppSettings.Get("NickName"),
                    ConfigurationManager.AppSettings.Get("RealName"), 
                    int.Parse(ConfigurationManager.AppSettings.Get("MillisecondsBetweenCommands"))))
                {
                    serverUser.CommandPrefix = ".";
                    using (var channel = new Channel(serverUser, ConfigurationManager.AppSettings.Get("ChannelName")))
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
