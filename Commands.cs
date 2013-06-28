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

        internal enum ChatBot
        {
            None,

            Romulus,

            Chato
        }

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
        /// The ask bot command.
        /// </summary>
        /// <param name="chatBot">
        /// The chatBot to use.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="botName">
        /// The bot name.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        internal static string AskBot(ChatBot chatBot, string message, string userName, string botName)
        {
            try
            {
                string answer;
                string searchForStart;
                string searchForEnd;

                switch (chatBot)
                {
                    case ChatBot.Romulus:
                        answer = Rest.Post(
                            new Uri("http://www.pandorabots.com"),
                            "/pandora/talk?botid=823a1209ae36baf3",
                            new KeyValuePair<string, string>("botcust2", userName),
                            new KeyValuePair<string, string>("input", message));
                        searchForStart = @"Dr. Romulon:</b> ";
                        searchForEnd = "<br>";
                        break;

                    case ChatBot.Chato:
                        answer = Rest.Post(
                            new Uri("http://nlp-addiction.com"),
                            "/chatbot/chato/f.php",
                            new KeyValuePair<string, string>("chat", message),
                            new KeyValuePair<string, string>("response_Array[sessionid]", "19dm6ogm92mc4krked08138c40"),
                            new KeyValuePair<string, string>("response_Array[userid]", "24876"),
                            new KeyValuePair<string, string>("action", "checkresponse"),
                            new KeyValuePair<string, string>("response_Array[rname]", "Array#0"));
                        searchForStart = @"Bot: <b><font size='+1'>";
                        searchForEnd = "</font>";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("chatBot", chatBot, "Unknown");
                }

                // Interprete answer
                var pos = answer.IndexOf(searchForStart, StringComparison.Ordinal);
                answer = answer.Substring(pos + searchForStart.Length);
                pos = answer.IndexOf(searchForEnd, StringComparison.Ordinal);
                answer = answer.Substring(0, pos);
                answer = WebUtility.HtmlDecode(answer);

                answer = answer.Replace("ALICE A.I.", "Zeniox Inc.");
                answer = answer.Replace("ALICE", "Zeniox");
                answer = answer.Replace("seeker", "sir");

                switch (chatBot)
                {
                    case ChatBot.Romulus:
                        if (!string.IsNullOrWhiteSpace(botName))
                        {
                            // Replace the bot name with our own
                            answer = answer.Replace("Dr. Romulon", botName);
                            answer = answer.Replace("Romulon", botName);
                        }

                        break;
                    case ChatBot.Chato:
                        if (!string.IsNullOrWhiteSpace(botName))
                        {
                            // Replace the bot name with our own
                            answer = answer.Replace("Chato", botName);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException("chatBot", chatBot, "Unknown");
                }

                return answer;
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to ask external bot {0}", chatBot);
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

            var answer = AskBot(
                ChatBot.Romulus, 
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
                        AskBot(ChatBot.Romulus, string.Format("My name is {0}", sender.Nickname), sender.Username, serverUser.NickName);
                        var a = AskBot(ChatBot.Romulus, "Hello", sender.Username, serverUser.NickName);
                        a += " Give me the command \"+bye\" to end the conversation.";
                        serverUser.SendMessage(Command.GetReceiver(sender, channel), a);
                        user.HasBeenPresented = true;
                    }
                }
                else
                {
                    serverUser.SendMessage(
                        Command.GetReceiver(sender, channel),
                        !user.HasBeenPresented ? "I wasn't talking to you? Enter \"+hello\" if you would like to." : "OK, I will stop talking to you.");
                }

                channel.DoInterpreteMessages = turnOn;

                channel.DoInterpreteMessages = null != UserDictionary.Values.FirstOrDefault(u => u.TalkTo);
            }
        }

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
                message = "The time is " + DateTime.UtcNow.ToString("dddd, dd MMMM yyyy HH:mm:ss", serverUser.CultureInfo) + " GMT";
            }
            else
            {
                try
                {
                    message = "The time is " + DateTime.Now.ToString(parameters[0], serverUser.CultureInfo);
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