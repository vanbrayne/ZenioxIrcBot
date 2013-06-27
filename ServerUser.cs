using System;

namespace ZenioxBot
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using NetIrc2;
    using NetIrc2.Events;

    /// <summary>
    /// Handles all things for a specific login.
    /// </summary>
    public class ServerUser : IDisposable
    {
        private readonly Dictionary<string, Channel> channelList = new Dictionary<string, Channel>();

        private DateTime lastCommandTime = DateTime.Now.AddDays(-1.0);

        /// <summary>
        /// Connect and login to an IrcServer.
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="nickName"></param>
        /// <param name="realName"></param>
        /// <param name="millisecondsBetweenCommands"></param>
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

        /// <summary>
        /// Get or set the minimum number of seconds between commands to the server to not get banned.
        /// </summary>
        public double SecondsBetweenCommands { get; set; }

        /// <summary>
        /// Get the name of the server we should connect to
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// Get the username to use for login
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// The nickname for the user that we login as.
        /// </summary>
        public string NickName { get; private set; }

        /// <summary>
        /// The real name for the user that we login as.
        /// </summary>
        public string RealName { get; private set; }

        /// <summary>
        /// Get the connection status.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.Client.IsConnected;
            }
        }

        /// <summary>
        /// Get the IRC client
        /// </summary>
        public IrcClient Client { get; private set; }

        /// <summary>
        /// The password that we use for login
        /// </summary>
        private string Password { get; set; }

        private bool TooFast
        {
            get
            {
                var secondsSinceLast = DateTime.Now.Subtract(this.lastCommandTime).TotalSeconds;
                return secondsSinceLast < this.SecondsBetweenCommands;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}@{1}", this.UserName, this.HostName);
        }

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

        public void Wait()
        {
            while (this.TooFast)
            {
                Thread.Sleep(10);
            }
        }

        public void Dispose()
        {
            this.LogOut();
        }

        internal void Add(Channel channel)
        {
            this.channelList.Add(channel.Name, channel);
            Debug.WriteLine(string.Format("Added channel {0}.", channel), this.ToString());
        }

        internal void CommandSent()
        {
            this.lastCommandTime = DateTime.Now;
        }

        internal void OnLeaveJoinChannels(bool isJoin, JoinLeaveEventArgs joinLeaveEventArgs)
        {
            foreach (var channelName in joinLeaveEventArgs.GetChannelList())
            {
                var channel = this.GetChannel(channelName);
                channel.OnLeaveJoin(isJoin, joinLeaveEventArgs.Identity);
            }
        }

        internal void OnMessage(Dispatcher.MessageType messageType, ChatMessageEventArgs chatMessageEventArgs)
        {
            var channel = this.GetChannel(chatMessageEventArgs.Recipient, true);

            if (null == channel)
            {
                Debug.WriteLine(
                    string.Format(
                    "Message \"{0}\" (from {1} to {2})",
                    chatMessageEventArgs.Message,
                    (chatMessageEventArgs.Sender != null) ? chatMessageEventArgs.Sender.Nickname.ToString() : "Anonymous",
                    chatMessageEventArgs.Recipient),
                    this.ToString());
                return;
            }

            channel.OnMessage(messageType, chatMessageEventArgs.Message, chatMessageEventArgs.Sender);
        }

        internal void OnSimpleMessage(string message)
        {
            Debug.WriteLine(string.Format("Simple message: \"{0}\"", message), this.ToString());
        }

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
    }
}
