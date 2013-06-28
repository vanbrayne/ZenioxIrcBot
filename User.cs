using System;

namespace ZenioxBot
{
    using System.Globalization;

    using NetIrc2;

    internal class User
    {
        private static int counter;
        private readonly IrcIdentity user;
        private bool talkTo;

        public User(IrcIdentity user)
        {
            this.user = user;
            this.NewBotId();
            this.HasBeenPresented = false;
            this.TalkTo = false;
        }

        public string UserName
        {
            get
            {
                return user.Username;
            }
        }

        public string NickName
        {
            get
            {
                return this.user.Nickname;
            }
        }

        public bool HasBeenPresented { get; set; }

        public string BotId { get; private set; }

        public bool TalkTo
        {
            get
            {
                return talkTo;
            }

            set
            {
                talkTo = value;
                if (value)
                {
                    this.NewBotId();
                }
            }
        }

        private void NewBotId()
        {
            this.BotId = this.UserName + DateTime.Now.ToString("HHmm", new CultureInfo("en-US")) + (counter++).ToString(new CultureInfo("en-US"));
            this.HasBeenPresented = false;
        }
    }
}
