namespace ZenioxBot
{
    using NetIrc2;

    internal class Command
    {
        public Command(string name, CommandFunction function)
        {
            this.Name = name;
            this.Function = function;

            CommandDispatcher.CommandList.Add(this.Name, this);
        }

        /// <summary>
        /// The command function.
        /// </summary>
        /// <param name="commandName">
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
        internal delegate void CommandFunction(string commandName, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel);

        public string Name { get; private set; }

        public CommandFunction Function { get; private set; }

        internal static string GetReceiver(IrcIdentity sender, Channel channel)
        {
            if (null != channel)
            {
                return channel.Name;
            }

            return sender.Username;
        }
    }
}
