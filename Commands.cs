using System;

namespace ZenioxBot
{
    using NetIrc2;

    internal static class Commands
    {      
        /// <summary>
        /// Initializes static members of the <see cref="CommandDispatcher"/> class.
        /// </summary>
        static Commands()
        {
            CommandDispatcher.CommandList.Add("time", TimeCommand);
        }

        private static string GetReceiver(IrcIdentity sender, Channel channel)
        {
            if (null != channel)
            {
                return channel.Name;
            }

            return sender.Username;
        }

        private static void TimeCommand(string command, string[] parameters, IrcIdentity sender, ServerUser serverUser, Channel channel)
        {
            string message;
            if (null == parameters || parameters.Length != 1)
            {
                message = DateTime.Now.ToLongTimeString();
            }
            else
            {
                try
                {
                    message = DateTime.Now.ToString(parameters[0]);
                }
                catch (Exception)
                {
                    message = @"Usage: time [<format>]\rSee http://www.geekzilla.co.uk/View00FF7904-B510-468C-A2C8-F859AA20581F.htm for examples of possible formats.";
                }
            }

            serverUser.SendMessage(GetReceiver(sender, channel), message);
        }
    }
}
