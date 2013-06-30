// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Channel.cs" company="">
//   
// </copyright>
// <summary>
//   A specific channel on a server.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ZenioxBot
{
    using System;
    using System.Diagnostics;

    using NetIrc2;

    /// <summary>
    ///     A specific channel on a server.
    /// </summary>
    public class Channel : IDisposable
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class.
        /// </summary>
        /// <param name="serverUser">
        /// The server user.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public Channel(ServerUser serverUser, string name)
        {
            this.ServerUser = serverUser;
            this.Name = name;
            this.KickBots = false;

            if (null == serverUser)
            {
                return;
            }

            this.ServerUser.Add(this);

            // Join
            this.Join();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether do interprete messages.
        /// </summary>
        public bool DoInterpreteMessages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether kick bots.
        /// </summary>
        public bool KickBots { get; set; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Gets the server user.
        /// </summary>
        public ServerUser ServerUser { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a value indicating whether is active.
        /// </summary>
        internal bool IsActive { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Leave();
        }

        /// <summary>
        ///     The leave.
        /// </summary>
        public void Leave()
        {
            // Leave
            this.ServerUser.Wait();
            this.ServerUser.Client.Leave(this.Name);
            this.ServerUser.CommandSent();
            var waiter = new Waiter(10.0, string.Format("Leave {0}", this));
            waiter.WaitFor(() => !this.IsActive);
        }

        /// <summary>
        ///     The to string.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", this.ServerUser, this.Name);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on command.
        /// </summary>
        /// <param name="messageType">
        /// The message type.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        internal void OnCommand(EventDispatcher.MessageType messageType, string command, string[] parameters, IrcIdentity sender)
        {
            CommandDispatcher.Dispatch(command, parameters, sender, this.ServerUser, this);
        }

        /// <summary>
        /// The on leave join.
        /// </summary>
        /// <param name="isJoin">
        /// The is join.
        /// </param>
        /// <param name="identity">
        /// The identity.
        /// </param>
        internal void OnLeaveJoin(bool isJoin, IrcIdentity identity)
        {
            Debug.WriteLine(string.Format("{0} {1}", identity.Nickname, isJoin ? "joined" : "left"), this.ToString());

            if (identity.Username == this.ServerUser.UserName)
            {
                this.IsActive = isJoin;
            }
            else
            {
                this.EvaluateUserName(identity.Username);
            }
        }

        /// <summary>
        /// The on message.
        /// </summary>
        /// <param name="messageType">
        /// The message type.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        internal void OnMessage(EventDispatcher.MessageType messageType, string message, IrcIdentity sender)
        {
            if (this.ServerUser.IsMe(sender))
            {
                return;
            }

            if (this.DoInterpreteMessages &&
                (messageType == EventDispatcher.MessageType.Message))
            {
                Commands.Interprete(message, sender, this.ServerUser, this);
            }
            else
            {
                Debug.WriteLine(
                    string.Format(
                        "Message \"{0}\" (from {1})", 
                        message, 
                        (sender != null) ? sender.Nickname.ToString() : "Anonymous"), 
                    this.ToString());
            }
        }

        /// <summary>
        /// The on name list.
        /// </summary>
        /// <param name="ircString">
        /// The irc string.
        /// </param>
        internal void OnNameList(IrcString[] ircString)
        {
            foreach (IrcString name in ircString)
            {
                this.EvaluateUserName(name);
            }
        }

        /// <summary>
        /// The send message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        internal void SendMessage(string message)
        {
            if (null == this.ServerUser)
            {
                Trace.WriteLine(message);
                return;
            }

            this.ServerUser.SendMessage(this.Name, message);
        }

        /// <summary>
        /// The evaluate user name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        private void EvaluateUserName(string name)
        {
            if (this.ServerUser.IsMe(name))
            {
                return;
            }

            if (name.EndsWith("bot") && name != "nightbot" && this.KickBots)
            {
                this.Kick(name, "There can be only one!");
            }
        }

        /// <summary>
        ///     The join.
        /// </summary>
        private void Join()
        {
            this.ServerUser.Wait();
            this.ServerUser.Client.Join(this.Name);
            this.ServerUser.CommandSent();
            var waiter = new Waiter(10.0, string.Format("Join {0}", this));
            waiter.WaitFor(() => this.IsActive);
        }

        /// <summary>
        /// The kick.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="reason">
        /// The reason.
        /// </param>
        private void Kick(string name, string reason = null)
        {
            this.ServerUser.Wait();
            this.ServerUser.Client.Kick(name, this.Name, reason);
            this.ServerUser.CommandSent();
        }

        #endregion
    }
}