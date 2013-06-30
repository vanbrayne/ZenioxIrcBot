// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Command.cs" company="">
//   
// </copyright>
// <summary>
//   The command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ZenioxBot
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using NetIrc2;

    /// <summary>
    /// The command.
    /// </summary>
    internal static class CommandDispatcher
    {
        #region Static Fields

        /// <summary>
        /// The command list.
        /// </summary>
        internal static readonly Dictionary<string, Command> CommandList = new Dictionary<string, Command>();

        #endregion

        #region Methods

        internal static void Dispatch(string commandName, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            if (CommandList.ContainsKey(commandName))
            {
                var command = CommandList[commandName];

                var commandParameters = new CommandParameters
                    {
                        CommandName = commandName,
                        Parameters = parameters,
                        Sender = sender,
                        ServerUser = serverUser,
                        Channel = channel,
                        Command = command
                    };

                command.Function(commandParameters);
            }
            else
            {
                var commandCompressed = commandName;

                if (null != parameters && parameters.Length > 0)
                {
                    commandCompressed += string.Format(" ({0})", string.Join(",", parameters));
                }
                Debug.WriteLine(
                    string.Format(
                        "Unknown command {0} (from {1})", 
                        commandCompressed,
                        (sender != null) ? sender.Nickname.ToString() : "Anonymous"), 
                    null != channel ? channel.ToString() : serverUser.ToString());
                var message = string.Format(
                    "Unknown command {0}",
                    commandCompressed);
                serverUser.SendMessage(Command.GetReceiver(sender, channel), message);
            }
        }

        #endregion
    }
}