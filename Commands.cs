using System;
using System.Linq;
using System.Net;

namespace ZenioxBot
{
    using System.Collections.Generic;

    using NetIrc2;

    internal static class Commands
    {
        /// <summary>
        /// Initializes static members of the <see cref="CommandDispatcher"/> class.
        /// </summary>
        public static void Register()
        {
            var command = new Command("time", TimeCommand);
            command = new Command("fuck", FuckCommand);
            command = new Command("hi", HelloCommand);
            command = new Command("hello", HelloCommand);
            command = new Command("help", HelpCommand);
            command = new Command("talk", AskBotCommand);
        }

        private static void TimeCommand(string command, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            string message;
            if (null == parameters || parameters.Length == 0)
            {
                message = DateTime.Now.ToLongTimeString();
            }
            else
            {
                try
                {
                    message = DateTime.Now.ToString(parameters[0]);
                }
                catch (Exception)
                {
                    message = @"Usage: time [<format>]\rSee http://www.geekzilla.co.uk/View00FF7904-B510-468C-A2C8-F859AA20581F.htm for examples of possible formats.";
                }
            }

            serverUser.SendMessage(Command.GetReceiver(sender, channel), message);
        }

        private static void FuckCommand(string command, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            string message;
            if (null == parameters || parameters.Length == 0)
            {
                message = "Fuck who?";
            }
            else
            {
                switch (parameters[0])
                {
                    case "you":
                        message = "Fuck you too!";
                        break;
                    case "me":
                        message = "You are too ugly!";
                        break;
                    default:
                        message = string.Format("Searching for {0}...", string.Join(" ", parameters));
                        break;
                }
            }

            serverUser.SendMessage(Command.GetReceiver(sender, channel), message);
        }

        private static void HelloCommand(string command, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            serverUser.SendMessage(Command.GetReceiver(sender, channel), "Hello!");
        }

        private static void HelpCommand(string c, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            string message = CommandDispatcher.CommandList.Values.OrderBy(p => p.Name).Aggregate("Known commands:", (current, command) => current + (" +" + command.Name));

            serverUser.SendMessage(Command.GetReceiver(sender, channel), message);
        }

        private static void AskBotCommand(string c, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            string message;
            bool okCommand = true;
            bool turnOn = false;
            if (null == parameters || parameters.Length != 1)
            {
                okCommand = false;
                turnOn = false;
            }
            else
            {
                switch (parameters[0])
                {
                    case "on":
                        turnOn = true;
                        break;
                    case "off":
                        turnOn = false;
                        break;
                    default:
                        turnOn = false;
                        okCommand = false;
                        break;
                }
            }

            if (okCommand)
            {
                channel.DoInterpreteMessages = turnOn;
            }
            else
            {
                message = @"Usage: talk on|off.";
                serverUser.SendMessage(Command.GetReceiver(sender, channel), message);
            }
        }

        internal static void Interprete(string message, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            try
            {
                var answer = AskRomulon(message, serverUser.NickName);
                serverUser.SendMessage(Command.GetReceiver(sender, channel), answer);
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to ask Dr. Romulon");
            }
        }

        private static string AskRomulon(string message, string name)
        {
            var answer = Rest.Post(
                    new Uri("http://www.pandorabots.com"),
                    "/pandora/talk?botid=823a1209ae36baf3",
                    new KeyValuePair<string, string>("botcust2", "9868c3a47e7aabb8"),
                    new KeyValuePair<string, string>("input", WebUtility.UrlEncode(message)));

            // Interprete answer
            const string SearchFor = @"Dr. Romulon:</b> ";
            var pos = answer.IndexOf(SearchFor, System.StringComparison.Ordinal);
            answer = answer.Substring(pos + SearchFor.Length);
            pos = answer.IndexOf("<br>", System.StringComparison.Ordinal);
            answer = answer.Substring(0, pos);
            answer = WebUtility.HtmlDecode(answer);

            // Replace the bot name with our own
            answer = answer.Replace("Dr. Romulon", name);
            answer = answer.Replace("Romulon", name);

            return answer;
        }
    }
}
