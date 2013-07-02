using System;

namespace ZenioxBot
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using NetIrc2;

    internal class User
    {
        /// <summary>
        /// The known users.
        /// </summary>
        private static readonly Dictionary<string, User> UserDictionary = new Dictionary<string, User>();

        private static int counter;
        private readonly IrcIdentity user;
        private bool talkTo;

        public User(IrcIdentity user)
        {
            this.user = user;
            this.NewBotId();
            this.HasBeenPresented = false;
            this.TalkTo = false;
            this.Translate = false;
            this.Language = null;
        }

        public static bool AnyUserTalks()
        {
            return User.UserDictionary.Values.FirstOrDefault(u => u.TalkTo) != null;
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

        public bool Translate { get; set; }

        public string Language { get; set; }

        public static User GetOrCreate(IrcIdentity user)
        {
            if (null == user)
            {
                return null;
            }

            if (!UserDictionary.ContainsKey(user.Username))
            {
                UserDictionary.Add(user.Username, new User(user));
            }

            return UserDictionary[user.Username];
        }

        private void NewBotId()
        {
            this.BotId = this.UserName + DateTime.Now.ToString("HHmm", new CultureInfo("en-US")) + (counter++).ToString(new CultureInfo("en-US"));
            this.HasBeenPresented = false;
        }
    }
}
