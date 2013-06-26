using System;

namespace ZenioxBot
{
    using System.Diagnostics;
    using System.Threading;

    using NetIrc2;
    using NetIrc2.Events;

    internal class Program
    {
        private const double SecondsBetweenCommands = 1.0;

        private const string ServerName = "irc.twitch.tv";
        private const string UserName = "zenioxbot";
        private const string RealName = "Zeniox Bot";
        private const string NickName = "ZenioxBot";
        private const string Password = "bottenarnadd";
        private const string ChannelName = "#zeniox";

        private static bool isConnected; 
        private static bool hasJoined;
        private static IrcClient client;

        private static DateTime lastCommandTime = DateTime.Now;

        private static bool TooFast
        {
            get
            {
                var secondsSinceLast = DateTime.Now.Subtract(lastCommandTime).TotalSeconds;
                return secondsSinceLast < SecondsBetweenCommands;
            }
        }

        internal static void Main(string[] args)
        {
            client = new IrcClient();

            RegisterMethods();

            // Connect
            Connect();

            // Login
            client.LogIn(UserName, RealName, NickName, ServerName, null, Password);
            CommandSent(); 

            // Join
            Join();

            Thread.Sleep(30000);

            // Leave
            client.Leave(ChannelName);
            CommandSent();

            // Logout
            client.LogOut(string.Format("{0} logging out.", NickName));
            CommandSent();

            // Close
            client.Close();
            CommandSent(false);
        }

        private static void Join()
        {
            client.Join(ChannelName);
            CommandSent(false);
            while (!hasJoined)
            {
                Thread.Sleep(100);
            }

            Wait();
        }

        private static void Connect()
        {
            client.Connect(ServerName);
            CommandSent(false);
            while (!isConnected)
            {
                Thread.Sleep(100);
            }

            Wait();
        }

        private static void RegisterMethods()
        {
            client.Connected += ClientOnConnected;
            client.GotChannelListBegin += OnChannelListBegin;
            client.GotChannelListEnd += OnChannelListEnd;
            client.GotChannelListEntry += ClientOnGotChannelListEntry;
            client.GotChannelTopicChange += ClientOnGotChannelTopicChange;
            client.GotChatAction += OnChatAction;
            client.GotInvitation += ClientOnGotInvitation;
            client.GotIrcError += ClientOnGotIrcError;
            client.GotJoinChannel += OnJoinChannel;
            client.GotLeaveChannel += OnLeaveChannel;
            client.GotMessage += OnMessage;
            client.GotMode += OnMode;
            client.GotMotdBegin += OnMotdBegin;
            client.GotMotdEnd += OnMotdEnd;
            client.GotMotdText += OnSimpleMessage;
            client.GotNameChange += OnNameChange;
            client.GotNameListEnd += OnNameListEnd;
            client.GotNameListReply += OnNameListReply;
            client.GotNotice += OnNotice;
            client.GotPingReply += OnPingReply;
            client.GotUserKicked += OnKick;
            client.GotUserQuit += OnQuit;
            client.GotWelcomeMessage += OnSimpleMessage;
        }

        private static void Wait()
        {
            while (TooFast)
            {
                Thread.Sleep(100);
            }
        }

        private static void OnMotdEnd(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("ONMOTDEND");
        }

        private static void OnMotdBegin(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("ONMOTDBEGIN");
        }

        private static void OnChannelListEnd(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private static void OnChannelListBegin(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private static void OnQuit(object sender, QuitEventArgs quitEventArgs)
        {
            throw new NotImplementedException();
        }

        private static void OnKick(object sender, KickEventArgs kickEventArgs)
        {
            throw new NotImplementedException();
        }

        private static void OnPingReply(object sender, PingReplyEventArgs pingReplyEventArgs)
        {
            throw new NotImplementedException();
        }

        private static void OnNameListReply(object sender, NameListReplyEventArgs nameListReplyEventArgs)
        {
            var names = string.Join<IrcString>(", ", nameListReplyEventArgs.GetNameList());
            Debug.WriteLine(string.Format("ONNAMELIST: {0}", names));
        }

        private static void OnNameListEnd(object sender, NameListEndEventArgs nameListEndEventArgs)
        {
            Debug.WriteLine(string.Format("ONNAMELISTEND: {0}", nameListEndEventArgs.Channel));
        }

        private static void OnNameChange(object sender, NameChangeEventArgs nameChangeEventArgs)
        {
            throw new NotImplementedException();
        }

        private static void OnSimpleMessage(object sender, SimpleMessageEventArgs simpleMessageEventArgs)
        {
            Debug.WriteLine(string.Format("ONSIMPLE: {0}", simpleMessageEventArgs.Message));
        }

        private static void OnMode(object sender, ModeEventArgs modeEventArgs)
        {
            Debug.WriteLine(string.Format("ONMODE: {0}", modeEventArgs.Command));
        }

        private static void OnMessage(object sender, ChatMessageEventArgs chatMessageEventArgs)
        {
            Debug.WriteLine(string.Format("ONMESSAGE: {0}", chatMessageEventArgs.Message));
            if (chatMessageEventArgs.Sender == null)
            {
                return;
            }

            // Send message
            client.Message(ChannelName, string.Format("Test Message: Hello {0}", chatMessageEventArgs.Sender.Nickname));
            CommandSent();
        }

        private static void OnChatAction(object sender, ChatMessageEventArgs chatMessageEventArgs)
        {
            Debug.WriteLine(string.Format("ONCHATACTION: {0}", chatMessageEventArgs.Message));
        }

        private static void OnNotice(object sender, ChatMessageEventArgs chatMessageEventArgs)
        {
            Debug.WriteLine(string.Format("ONNOTICE: {0}", chatMessageEventArgs.Message));
        }

        private static void OnJoinChannel(object sender, JoinLeaveEventArgs joinLeaveEventArgs)
        {
            var channels = string.Join<IrcString>(", ", joinLeaveEventArgs.GetChannelList());
            Debug.WriteLine(string.Format("ONJOIN: {0}", channels));

            hasJoined = true;
        }

        private static void OnLeaveChannel(object sender, JoinLeaveEventArgs joinLeaveEventArgs)
        {
            var channels = string.Join<IrcString>(", ", joinLeaveEventArgs.GetChannelList());
            Debug.WriteLine(string.Format("ONLEAVE: {0}", channels));
        }

        private static void ClientOnGotIrcError(object sender, IrcErrorEventArgs ircErrorEventArgs)
        {
            var parameters = string.Join(" ", ircErrorEventArgs.Data.Parameters);
            Debug.WriteLine(string.Format("ONIRCERROR, Parameters: {0}", parameters));
        }

        private static void ClientOnGotInvitation(object sender, InvitationEventArgs invitationEventArgs)
        {
            throw new NotImplementedException();
        }

        private static void ClientOnGotChannelTopicChange(object sender, ChannelTopicChangeEventArgs channelTopicChangeEventArgs)
        {
            throw new NotImplementedException();
        }

        private static void ClientOnGotChannelListEntry(object sender, ChannelListEntryEventArgs channelListEntryEventArgs)
        {
            throw new NotImplementedException();
        }

        private static void ClientOnConnected(object sender, EventArgs eventArgs)
        {

            var c = sender as IrcClient;
            if (c == client)
            {
                isConnected = true;
            }
        }

        private static void CommandSent(bool doWait = true)
        {
            lastCommandTime = DateTime.Now;
            if (doWait)
            {
                Wait();
            }
        }
    }
}
