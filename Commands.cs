using System;
using System.Linq;
using System.Net;

namespace ZenioxBot
{
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
            var message = string.Join(" ", parameters);
            var input = WebUtility.UrlEncode(message);
            Rest.Post(new Uri("http://www.pandorabots.com/pandora/talk?botid=823a1209ae36baf3"), "botcust2=9868c3a47e7aabb8&input=" + input);
        }

        public static string GetBotAnswer(string message)
        {
            const string searchFor = @"Dr. Romulon:</b> ";
            var pos = message.IndexOf(searchFor, System.StringComparison.Ordinal);
            message = message.Substring(pos + searchFor.Length);
            pos = message.IndexOf("<br>", System.StringComparison.Ordinal);
            message = message.Substring(0, pos);

            return message;
        }
    }
}
