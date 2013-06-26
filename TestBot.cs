using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IrcDotNet;
using IrcDotNet.Samples.Common;

namespace ZenioxBot
{
    public class TestBot : BasicIrcBot
    {
        private const string quitMessage = "ZenioxBot ends";

        public TestBot()
            : base()
        {
            //this.ConnectCommand("irc.twitch.tv");
            this.ConnectCommand("zenioxbot.jtvirc.com");
        }

        public override IrcRegistrationInfo RegistrationInfo
        {
            get
            {
                return new IrcUserRegistrationInfo()
                                    {
                                        NickName = "ZenioxBot",
                                        UserName = "zenioxbot",
                                        Password = "snille(bot)",
                                        RealName = "Zeniox Bot"
                                    };
                                    //{
                                    //    NickName = "Zeniox",
                                    //    UserName = "zeniox",
                                    //    Password = "dra3gon"
                                    //};

            }
        }

        private void ConnectCommand(string server)
        {
            Connect(server, this.RegistrationInfo);
        }

        public override string QuitMessage
        {
            get { return quitMessage; }
        }

        protected override void OnClientConnect(IrcClient client)
        {
            Console.Out.WriteLine("OnClientConnect");
        }

        protected override void OnClientDisconnect(IrcClient client)
        {
            Console.Out.WriteLine("OnClientDisconnect");
        }

        protected override void OnClientRegistered(IrcClient client)
        {
            Console.Out.WriteLine("OnClientRegistered");
        }

        protected override void OnLocalUserJoinedChannel(IrcLocalUser localUser, IrcChannelEventArgs e)
        {
            Console.Out.WriteLine("OnLocalUserJoinedChannel");
        }

        protected override void OnLocalUserLeftChannel(IrcLocalUser localUser, IrcChannelEventArgs e)
        {
            Console.Out.WriteLine("OnLocalUserLeftChannel");
        }

        protected override void OnLocalUserNoticeReceived(IrcLocalUser localUser, IrcMessageEventArgs e)
        {
            Console.Out.WriteLine("OnLocalUserNoticeReceived");
            Console.Out.WriteLine(localUser.RealName);
            Console.Out.WriteLine(e.Text);
        }

        protected override void OnLocalUserMessageReceived(IrcLocalUser localUser, IrcMessageEventArgs e)
        {
            Console.Out.WriteLine("OnLocalUserMessageReceived");
            Console.Out.WriteLine(localUser.RealName);
            Console.Out.WriteLine(e.Text);
        }

        protected override void OnChannelUserJoined(IrcChannel channel, IrcChannelUserEventArgs e)
        {
            Console.Out.WriteLine("OnChannelUserJoined");
        }

        protected override void OnChannelUserLeft(IrcChannel channel, IrcChannelUserEventArgs e)
        {
            Console.Out.WriteLine("OnChannelUserLeft");
        }

        protected override void OnChannelNoticeReceived(IrcChannel channel, IrcMessageEventArgs e)
        {
            Console.Out.WriteLine("OnChannelNoticeReceived");
            Console.Out.WriteLine(channel.Name);
            Console.Out.WriteLine(e.Text);
        }

        protected override void OnChannelMessageReceived(IrcChannel channel, IrcMessageEventArgs e)
        {
            var client = channel.Client;

            if (e.Source is IrcUser)
            {
                Console.Out.WriteLine(e.Text);
                var said = e.Text.Substring(e.Text.Length - 5, 5);
                if (said == "hello")
                {
                    var replyTargets = GetDefaultReplyTarget(client, e.Source, e.Targets);
                    client.LocalUser.SendMessage(replyTargets, "Hello to you, sir.");
                    Console.Out.WriteLine("Answered");
                }
            }
        }

        protected override void InitializeChatCommandProcessors()
        {
            base.InitializeChatCommandProcessors();

            this.ChatCommandProcessors.Add("talk", this.ProcessChatCommandTalk);
        }

        #region Chat Command Processors

        private void ProcessChatCommandTalk(IrcClient client, IIrcMessageSource source,
            IList<IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var replyTargets = GetDefaultReplyTarget(client, source, targets);
            client.LocalUser.SendMessage(replyTargets, "I can't talk yet. Under construction...");
        }
        #endregion

        protected override void InitializeCommandProcessors()
        {
            base.InitializeCommandProcessors();
        }

        #region Command Processors

        //

        #endregion
    }
}
