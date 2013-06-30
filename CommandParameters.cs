using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenioxBot
{
    using NetIrc2;

    public class CommandParameters
    {
        public string CommandName { get; set; }

        public string[] Parameters { get; set; }

        public IrcIdentity Sender { get; set; }

        public ServerUser ServerUser { get; set; }

        public Channel Channel { get; set; }

        internal Command Command { get; set; }

        public string Receiver
        {
            get
            {
                return Command.GetReceiver(Sender, Channel);
            }
        }
    }
}
