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
            ServerUser serverUser = null;
            Channel channel = null;

            try
            {
                Commands.Register();

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
                        case "j":
                            serverUser = new ServerUser(
                                ConfigurationManager.AppSettings.Get("HostName"),
                                ConfigurationManager.AppSettings.Get("UserName"),
                                ConfigurationManager.AppSettings.Get("TheWord"),
                                ConfigurationManager.AppSettings.Get("NickName"),
                                ConfigurationManager.AppSettings.Get("RealName"),
                                int.Parse(ConfigurationManager.AppSettings.Get("MillisecondsBetweenCommands")))
                                             {
                                                 CommandPrefix = "+"
                                             };
                            var channelName = ConfigurationManager.AppSettings.Get("ChannelName");
                            if (parameters.Length > 0)
                            {
                                channelName = parameters[0];
                            }
                            channel = new Channel(serverUser, channelName)
                                          {
                                              KickBots = true
                                          };
                            break;
                        case "q":
                        case "l":
                            if (null != channel)
                            {
                                channel.Leave();
                                serverUser.LogOut();
                            }

                            if (command == "q")
                            {
                                isRunning = false;
                            }

                            break;
                        case "t":
                            if (channel == null)
                            {
                                break;
                            }

                            if (parameters.Length == 1)
                            {
                                channel.DoInterpreteMessages = parameters[0] == "on";
                            }
                            else
                            {
                                Console.WriteLine("Usage: t on|off");
                            }
                            break;
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
