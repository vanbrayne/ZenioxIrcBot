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
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;

    using NetIrc2;

    /// <summary>
    ///     The commands.
    /// </summary>
    internal static class Commands
    {
        #region Static Fields

        private static string lastPollId;
        private static string lastPollQuestion;

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
            Command[] commands =
                {
                    new Command("time", TimeCommand),
                    new Command("help", HelpCommand),
                    new Command("hello", TalkCommand),
                    new Command("bye", TalkCommand),
                    new Command("ip", IpCommand),
                    new Command("calc", CalculatorCommand),
                    new Command("translate", TranslateCommand),
                    new Command("poll", StartPollCommand),
                    new Command("endpoll", EndPollCommand)
                };
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
                answer = Rest.FindPart(answer, searchForStart, searchForEnd);
                if (answer == null)
                {
                    throw new Exception("Could not find the chatbot answer.");
                }

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

            var user = User.GetOrCreate(sender);

            if (user.Translate)
            {
                var language = Services.DetectLanguage(message, false, user.Language == null ? 0.1 : 0.3);

                if ((language != null) && (language != "en"))
                {
                    user.Language = language;
                }

                if (user.Language != null)
                {
                    message = Services.Translate(user.Language, "en", message);
                    channel.SendMessage(string.Format("{0} ({1}): {2}", user.NickName, user.Language, message));
                }
            }

            if (!user.TalkTo)
            {
                return;
            }

            var answer = AskBot(
                ChatBot.Romulus,
                message,
                sender.Username,
                serverUser.NickName);

            if (channel != null)
            {
                answer = string.Format("-> {0}: {1}", sender.Nickname, answer);
            }

            serverUser.SendMessage(Command.GetReceiver(sender, channel), answer);
        }

        /// <summary>
        /// The ask bot command.
        /// </summary>
        /// <param name="commandParameters">The parameters that describes the command and the command context.</param>
        private static void TalkCommand(CommandParameters commandParameters)
        {
            var commandIsOk = true;
            bool turnOn;

            switch (commandParameters.CommandName)
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
                var user = User.GetOrCreate(commandParameters.Sender);

                user.TalkTo = turnOn;

                if (turnOn)
                {
                    if (!user.HasBeenPresented)
                    {
                        AskBot(
                            ChatBot.Romulus,
                            string.Format("My name is {0}", commandParameters.Sender.Nickname),
                            commandParameters.Sender.Username,
                            commandParameters.ServerUser.NickName);
                        var a = AskBot(ChatBot.Romulus, "Hello", commandParameters.Sender.Username, commandParameters.ServerUser.NickName);
                        a += " Give me the command \"+bye\" to end the conversation.";
                        commandParameters.ServerUser.SendMessage(commandParameters.Receiver, a);
                        user.HasBeenPresented = true;
                    }
                }
                else
                {
                    commandParameters.ServerUser.SendMessage(
                        commandParameters.Receiver,
                        !user.HasBeenPresented ? "I wasn't talking to you? Enter \"+hello\" if you would like to." : "OK, I will stop talking to you.");
                }

                commandParameters.Channel.DoInterpreteMessages = turnOn;

                commandParameters.Channel.DoInterpreteMessages = User.AnyUserTalks();
            }
        }

        /// <summary>
        /// The help command.
        /// </summary>
        /// <param name="commandParameters">The parameters that describes the command and the command context.</param>
        private static void HelpCommand(CommandParameters commandParameters)
        {
            var stringList = CommandDispatcher.CommandList.Values.Select(p => p.Name).OrderBy(p => p);
            var message = "Known commands: " + stringList.Aggregate((current, s) => current + ", " + s);
            commandParameters.ServerUser.SendMessage(commandParameters.Receiver, message);
        }

        /// <summary>
        /// The time command.
        /// </summary>
        /// <param name="commandParameters">The parameters that describes the command and the command context.</param>
        private static void TimeCommand(CommandParameters commandParameters)
        {
            string message;
            if (null == commandParameters.Parameters || commandParameters.Parameters.Length == 0)
            {
                message = "The time is " + DateTime.UtcNow.ToString("dddd, dd MMMM yyyy HH:mm:ss", commandParameters.ServerUser.CultureInfo) + " GMT";
            }
            else
            {
                try
                {
                    message = "The time is " + DateTime.Now.ToString(commandParameters.Parameters[0], commandParameters.ServerUser.CultureInfo);
                }
                catch (Exception)
                {
                    message = @"Usage: time [<format>]\rSee http://www.geekzilla.co.uk/View00FF7904-B510-468C-A2C8-F859AA20581F.htm for examples of possible formats.";
                }
            }

            commandParameters.ServerUser.SendMessage(commandParameters.Receiver, message);
        }

        private static void StartPollCommand(CommandParameters commandParameters)
        {
            const string Path1 = "http://www.anonvote.com/poll.php?id=";

            try
            {
                if (lastPollId == null)
                {
                    var message = string.Join(" ", commandParameters.Parameters);

                    // Find the question
                    var question = Rest.FindPart(message, string.Empty, "?");
                    if (question == null)
                    {
                        throw new Exception("No question found. A question is one or more words, ending with a '?'");
                    }

                    question = question.Trim() + "?";

                    // Find the answers
                    var answers = Rest.FindPart(message, question, null);
                    if (answers == null)
                    {
                        throw new Exception("No answers found. The answers are supposed to come after the '?', either as a number of single words or as a comma separated answers.");
                    }

                    answers = answers.Trim();

                    // Split the answers into individual answers
                    var splitChar = ' ';
                    if (answers.Contains(","))
                    {
                        splitChar = ',';
                    }

                    var answerList = answers.Split(splitChar).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
                    if (answers.Length < 2)
                    {
                        throw new Exception("A poll must have at least two answers.");
                    }

                    var values = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("answer0", question) };
                    var i = 1;
                    values.AddRange(answerList.Select(answer => new KeyValuePair<string, string>(string.Format("answer{0}", i++), answer)));
                    values.Add(new KeyValuePair<string, string>("addimages", "Make Poll"));

                    var reply = Rest.Post(
                        new Uri("http://www.anonvote.com"),
                        "/",
                        values.ToArray());

                    lastPollId = Rest.FindPart(reply, string.Format("<a href=\"{0}", Path1), "\" target=\"_blank");
                    if (lastPollId == null)
                    {
                        throw new Exception("www.anonvote.com did not react as expected.");
                    }

                    lastPollQuestion = question;
                    commandParameters.Channel.SendMessage(string.Format("New poll: {0} Please answer poll at {1}", question, Path1 + lastPollId));
                }
                else
                {
                    if (commandParameters.Parameters != null && commandParameters.Parameters.Length > 0)
                    {
                        throw new Exception(string.Format("You must end the last poll ({0}) before you can create a new poll.", lastPollQuestion));
                    }

                    commandParameters.Channel.SendMessage(string.Format("Current poll: {0} Please answer poll at {1}", lastPollQuestion, Path1 + lastPollId));
                }
            }
            catch (Exception ex)
            {
                commandParameters.Channel.SendMessage(string.Format("Failed to create poll. {0}", ex.Message));
            }
        }

        private static void EndPollCommand(CommandParameters commandParameters)
        {
            try
            {
                if (lastPollId == null)
                {
                    throw new Exception("There is no current poll.");
                }

                var path = string.Format("/poll.php?id={0}&results=true", lastPollId);
                var reply = Rest.Get(
                    new Uri("http://www.anonvote.com"), path);

                // Jump to answer table
                string answers;
                Rest.FindPart(reply, "<h1>", "class=\"tables\"", out answers);

                if (null == answers)
                {
                    throw new Exception("Could not parse poll results on www.anonvote.com");
                }

                commandParameters.Channel.SendMessage(string.Format("Results for \"{0}\"", lastPollQuestion));

                do
                {
                    // Find the answer
                    var answer = Rest.FindPart(answers, "<span class=\"boldNormal2\">", "</span>", out answers);
                    if (answer == null)
                    {
                        break;
                    }

                    // Find the result
                    var result = Rest.FindPart(answers, "<span class=\"boldNormal\">", "</span>", out answers);

                    commandParameters.Channel.SendMessage(string.Format("{0}: {1}", answer, result));
                }
                while (true);

                commandParameters.Channel.SendMessage(string.Format("Graph of results: {0}{1}", "http://www.anonvote.com", path));
                lastPollId = null;
                lastPollQuestion = null;
            }
            catch (Exception ex)
            {
                commandParameters.Channel.SendMessage(string.Format("Failed to end poll. {0}", ex.Message));
            }
        }

        private static void CalculatorCommand(CommandParameters commandParameters)
        {
            try
            {
                var expression = string.Join(" ", commandParameters.Parameters);
                var reply = Rest.Get(
                    new Uri("http://www.webmath.com/"),
                    "/cgi-bin/gopoly.cgi",
                    new KeyValuePair<string, string>("cgiCall", "gopoly"),
                    new KeyValuePair<string, string>("getPost", "post"),
                    new KeyValuePair<string, string>("s", expression),
                    new KeyValuePair<string, string>("back", "anything.html"));

                var answer = Rest.FindPart(reply, new[] { "The final answer is", "<b>" }, "</b>");

                commandParameters.Channel.SendMessage(string.Format("Answer: {0}", answer));
            }
            catch (Exception ex)
            {
                commandParameters.Channel.SendMessage(string.Format("Failed to calculate. {0}", ex.Message));
            }
        }

        private static void IpCommand(CommandParameters commandParameters)
        {
            commandParameters.Channel.SendMessage(string.Format("Server ip: {0}", ConfigurationManager.AppSettings.Get("Ip")));
        }

        /// <summary>
        /// The ask bot command.
        /// </summary>
        /// <param name="commandParameters">The parameters that describes the command and the command context.</param>
        private static void TranslateCommand(CommandParameters commandParameters)
        {
            bool turnOn;

            if (commandParameters.Parameters == null || commandParameters.Parameters.Length < 1)
            {
                commandParameters.Channel.SendMessage("Usage: +translate on|off");
                return;
            }

            turnOn = commandParameters.Parameters[0] == "on";

            var user = User.GetOrCreate(commandParameters.Sender);

            user.Translate = turnOn;

            commandParameters.Channel.SendMessage(string.Format("Translate {0} for user {1}.", turnOn ? "on" : "off", commandParameters.Sender.Nickname));
        }

        #endregion
    }
}