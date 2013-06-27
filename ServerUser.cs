// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServerUser.cs" company="">
//   
// </copyright>
// <summary>
//   Handles all things for a specific login.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ZenioxBot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using NetIrc2;
    using NetIrc2.Events;

    /// <summary>
    ///     Handles all things for a specific login.
    /// </summary>
    public class ServerUser : IDisposable
    {
        #region Fields

        /// <summary>
        /// The channel list.
        /// </summary>
        private readonly Dictionary<string, Channel> channelList = new Dictionary<string, Channel>();

        /// <summary>
        /// The last command time.
        /// </summary>
        private DateTime lastCommandTime = DateTime.Now.AddDays(-1.0);

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerUser"/> class. 
        /// Connect and login to an IrcServer.
        /// </summary>
        /// <param name="hostName">
        /// </param>
        /// <param name="userName">
        /// </param>
        /// <param name="password">
        /// </param>
        /// <param name="nickName">
        /// </param>
        /// <param name="realName">
        /// </param>
        /// <param name="millisecondsBetweenCommands">
        /// </param>
        public ServerUser(string hostName, string userName, string password, string nickName = null, string realName = null, int millisecondsBetweenCommands = 1000)
        {
            this.HostName = hostName;
            this.UserName = userName;
            this.Password = password;
            this.NickName = nickName ?? userName;
            this.RealName = realName ?? this.NickName;
            this.SecondsBetweenCommands = millisecondsBetweenCommands / 1000.0;

            this.Client = new IrcClient();

            Dispatcher.Add(this);

            // Connect
            this.Wait();
            this.Client.Connect(this.HostName);
            this.CommandSent();
            var waiter = new Waiter(10.0, string.Format("Connecting {0}", this));
            waiter.WaitFor(() => this.IsConnected);

            // Login
            this.Wait();
            this.Client.LogIn(this.UserName, this.RealName, this.NickName, this.HostName, null, this.Password);
            this.CommandSent();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the command prefix
        /// </summary>
        public string CommandPrefix { get; set; }

        /// <summary>
        /// Gets the IRC client
        /// </summary>
        public IrcClient Client { get; private set; }

        /// <summary>
        /// Gets the name of the server we should connect to
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// Gets the connection status.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.Client.IsConnected;
            }
        }

        /// <summary>
        ///     The nickname for the user that we login as.
        /// </summary>
        public string NickName { get; private set; }

        /// <summary>
        ///     The real name for the user that we login as.
        /// </summary>
        public string RealName { get; private set; }

        /// <summary>
        /// Gets or set the minimum number of seconds between commands to the server to not get banned.
        /// </summary>
        public double SecondsBetweenCommands { get; set; }

        /// <summary>
        /// Gets the username to use for login
        /// </summary>
        public string UserName { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the password that we use for login
        /// </summary>
        private string Password { get; set; }

        /// <summary>
        /// Gets a value indicating whether too fast.
        /// </summary>
        private bool TooFast
        {
            get
            {
                var secondsSinceLast = DateTime.Now.Subtract(this.lastCommandTime).TotalSeconds;
                return secondsSinceLast < this.SecondsBetweenCommands;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.LogOut();
        }

        /// <summary>
        /// The log out.
        /// </summary>
        public void LogOut()
        {
            if (!this.IsConnected)
            {
                return;
            }

            // Logout
            this.Wait();
            this.Client.LogOut(string.Format("{0} logging out.", this.NickName));

            // Close
            this.Client.Close();
            var waiter = new Waiter(10.0, string.Format("Leave {0}", this));
            waiter.WaitFor(() => !this.IsConnected);
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}@{1}", this.UserName, this.HostName);
        }

        /// <summary>
        /// The wait.
        /// </summary>
        public void Wait()
        {
            while (this.TooFast)
            {
                Thread.Sleep(10);
            }
        }

        #endregion

        #region Methods
   
        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        internal void Add(Channel channel)
        {
            this.channelList.Add(channel.Name, channel);
            Debug.WriteLine(string.Format("Added channel {0}.", channel), this.ToString());
        }

        /// <summary>
        /// The command sent.
        /// </summary>
        internal void CommandSent()
        {
            this.lastCommandTime = DateTime.Now;
        }

        /// <summary>
        /// The on leave join channels.
        /// </summary>
        /// <param name="isJoin">
        /// The is join.
        /// </param>
        /// <param name="joinLeaveEventArgs">
        /// The join leave event args.
        /// </param>
        internal void OnLeaveJoinChannels(bool isJoin, JoinLeaveEventArgs joinLeaveEventArgs)
        {
            foreach (IrcString channelName in joinLeaveEventArgs.GetChannelList())
            {
                Channel channel = this.GetChannel(channelName);
                channel.OnLeaveJoin(isJoin, joinLeaveEventArgs.Identity);
            }
        }

        /// <summary>
        /// The on message.
        /// </summary>
        /// <param name="messageType">
        /// The message type.
        /// </param>
        /// <param name="chatMessageEventArgs">
        /// The chat message event args.
        /// </param>
        internal void OnMessage(Dispatcher.MessageType messageType, ChatMessageEventArgs chatMessageEventArgs)
        {
            string message = chatMessageEventArgs.Message;
            var channel = this.GetChannel(chatMessageEventArgs.Recipient, true);
            var command = this.GetCommand(chatMessageEventArgs.Message);

            if (null == channel)
            {
                // This message/command was a personal message to the bot.
                Debug.WriteLine(
                    string.Format(
                        "Message \"{0}\" (from {1} to {2})",
                        message,
                        (chatMessageEventArgs.Sender != null) ? chatMessageEventArgs.Sender.Nickname.ToString() : "Anonymous",
                        chatMessageEventArgs.Recipient),
                    this.ToString());
                return;
            }

            // A channel message/command
            if (null == command)
            {
                // A message
                channel.OnMessage(messageType, message, chatMessageEventArgs.Sender);
            }
            else
            {
                // A command
                channel.OnCommand(messageType, command, GetParameters(message), chatMessageEventArgs.Sender);
            }
        }

        /// <summary>
        /// The on simple message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        internal void OnSimpleMessage(string message)
        {
            Debug.WriteLine(string.Format("Simple message: \"{0}\"", message), this.ToString());
        }

        internal void SendMessage(string receiver, string message)
        {
            this.Client.Message(receiver, message);
        }

        private string GetCommand(string message)
        {
            var parts = message.Split(' ');
            var command = parts[0].ToLower();

            // Remove command prefix
            command = command.Substring(this.CommandPrefix.Length);

            return string.IsNullOrEmpty(command) ? null : command;
        }

        private static string[] GetParameters(string message)
        {
            var parts = message.Split(' ');
            return parts.Skip(1).ToArray();
        }

        /// <summary>
        /// The get channel.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="notFoundIsOk">
        /// The not found is ok.
        /// </param>
        /// <returns>
        /// The <see cref="Channel"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentException">
        /// </exception>
        private Channel GetChannel(string name, bool notFoundIsOk = false)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (!this.channelList.ContainsKey(name))
            {
                if (notFoundIsOk)
                {
                    return null;
                }

                throw new ArgumentException(string.Format("Channel {0} is unknown for server {1}.", name, this));
            }

            return this.channelList[name];
        }

        /// <summary>
        /// Check if a message is a command
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool IsCommand(string message)
        {
            if (string.IsNullOrEmpty(this.CommandPrefix))
            {
                // No command prefix has been specified, so no messages are considered as commands
                return false;
            }

            return message.StartsWith(this.CommandPrefix);
        }

        #endregion
    }
}