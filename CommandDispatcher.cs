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
        internal static readonly Dictionary<string, CommandFunction> CommandList = new Dictionary<string, CommandFunction>();

        #endregion

        #region Delegates

        /// <summary>
        /// The command function.
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
        internal delegate void CommandFunction(string command, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel);

        #endregion

        #region Methods

        internal static void Dispatch(string command, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            if (CommandList.ContainsKey(command))
            {
                CommandFunction d = CommandList[command];
                d(command, parameters, sender, serverUser, channel);
            }
            else
            {
                Debug.WriteLine(
                    string.Format(
                        "Unknown command {0}({1}) (from {2})", 
                        command, 
                        string.Join(",", parameters), 
                        (sender != null) ? sender.Nickname.ToString() : "Anonymous"), 
                    null != channel ? channel.ToString() : serverUser.ToString());
            }
        }

        #endregion
    }
}