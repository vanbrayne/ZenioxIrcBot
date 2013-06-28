// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Commands.cs" company="">
//   
// </copyright>
// <summary>
//   The commands.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ZenioxBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using NetIrc2;

    /// <summary>
    ///     The commands.
    /// </summary>
    internal static class Commands
    {
        #region Static Fields

        /// <summary>
        /// The known users.
        /// </summary>
        private static readonly Dictionary<string, User> UserDictionary = new Dictionary<string, User>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Initializes static members of the <see cref="CommandDispatcher" /> class.
        /// </summary>
        public static void Register()
        {
            var command = new Command("time", TimeCommand);
            command = new Command("help", HelpCommand);
            command = new Command("hello", TalkCommand);
            command = new Command("bye", TalkCommand);
        }

        /// <summary>
        /// The ask romulon.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="botName">
        /// The bot name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string AskRomulon(string message, string userName, string botName)
        {
            try
            {
                var answer = Rest.Post(
                    new Uri("http://www.pandorabots.com"),
                    "/pandora/talk?botid=823a1209ae36baf3",
                    new KeyValuePair<string, string>("botcust2", userName),
                    new KeyValuePair<string, string>("input", message));

                // Interprete answer
                const string SearchFor = @"Dr. Romulon:</b> ";
                int pos = answer.IndexOf(SearchFor, StringComparison.Ordinal);
                answer = answer.Substring(pos + SearchFor.Length);
                pos = answer.IndexOf("<br>", StringComparison.Ordinal);
                answer = answer.Substring(0, pos);
                answer = WebUtility.HtmlDecode(answer);

                answer = answer.Replace("ALICE A.I.", "Zeniox Inc.");
                answer = answer.Replace("ALICE", "Zeniox");
                answer = answer.Replace("seeker", "sir");
                if (!string.IsNullOrWhiteSpace(botName))
                {
                    // Replace the bot name with our own
                    answer = answer.Replace("Dr. Romulon", botName);
                    answer = answer.Replace("Romulon", botName);
                }

                return answer;
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to ask Dr. Romulon");
                return null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The interprete.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="serverUser">
        /// The server user.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        internal static void Interprete(string message, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            if (null == sender)
            {
                return;
            }

            if (!UserDictionary.ContainsKey(sender.Username))
            {
                UserDictionary.Add(sender.Username, new User(sender));
            }

            var user = UserDictionary[sender.Username];

            if (!user.TalkTo)
            {
                return;
            }

            var answer = AskRomulon(
                message,
                sender.Username,
                serverUser.NickName);
            serverUser.SendMessage(Command.GetReceiver(sender, channel), answer);
        }

        /// <summary>
        /// The ask bot command.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="serverUser">
        /// The server user.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        private static void TalkCommand(string c, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            var commandIsOk = true;
            bool turnOn;

            switch (c)
            {
                case "hello":
                    turnOn = true;
                    break;
                case "bye":
                    turnOn = false;
                    break;
                default:
                    turnOn = false;
                    commandIsOk = false;
                    break;
            }

            if (commandIsOk)
            {
                if (!UserDictionary.ContainsKey(sender.Username))
                {
                    UserDictionary.Add(sender.Username, new User(sender));
                }

                var user = UserDictionary[sender.Username];

                user.TalkTo = turnOn;

                if (turnOn)
                {
                    if (!user.HasBeenPresented)
                    {
                        AskRomulon(string.Format("My name is {0}", sender.Nickname), sender.Username, serverUser.NickName);
                        var a = AskRomulon("Hello", sender.Username, serverUser.NickName);
                        serverUser.SendMessage(Command.GetReceiver(sender, channel), a);
                        user.HasBeenPresented = true;
                    }
                }
                else
                {
                    if (!user.HasBeenPresented)
                    {
                        serverUser.SendMessage(Command.GetReceiver(sender, channel), "I wasn't talking to you? Enter \"+hello\" if you would like to.");
                    }
                    else
                    {
                        serverUser.SendMessage(Command.GetReceiver(sender, channel), "OK, I will stop talking to you.");
                    }
                }

                channel.DoInterpreteMessages = turnOn;

                channel.DoInterpreteMessages = null != UserDictionary.Values.FirstOrDefault(u => u.TalkTo);
            }
        }

        /// <summary>
        /// The hello command.
        /// <summary>
        /// The help command.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="serverUser">
        /// The server user.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        private static void HelpCommand(string c, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            string message = CommandDispatcher.CommandList.Values.OrderBy(p => p.Name).Aggregate("Known commands:", (current, command) => current + (" +" + command.Name));

            serverUser.SendMessage(Command.GetReceiver(sender, channel), message);
        }

        /// <summary>
        /// The time command.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="serverUser">
        /// The server user.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        private static void TimeCommand(string command, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            string message;
            if (null == parameters || parameters.Length == 0)
            {
                message = DateTime.Now.ToString("u", serverUser.CultureInfo);
            }
            else
            {
                try
                {
                    message = DateTime.Now.ToString(parameters[0], serverUser.CultureInfo);
                }
                catch (Exception)
                {
                    message = @"Usage: time [<format>]\rSee http://www.geekzilla.co.uk/View00FF7904-B510-468C-A2C8-F859AA20581F.htm for examples of possible formats.";
                }
            }

            serverUser.SendMessage(Command.GetReceiver(sender, channel), message);
        }

        #endregion
    }
}