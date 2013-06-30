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
        /// <param name="commandParameters">The parameters that describes the command and the command context.</param>
        internal delegate void CommandFunction(CommandParameters commandParameters);

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
