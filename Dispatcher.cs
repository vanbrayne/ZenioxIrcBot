using System;

namespace ZenioxBot
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using NetIrc2;
    using NetIrc2.Events;

    /// <summary>
    /// Dispatches all events to the right <see cref="ServerUser"/>.
    /// </summary>
    public static class Dispatcher
    {
        /// <summary>
        /// All known ServerUsers
        /// </summary>
        private static readonly Dictionary<IrcClient, ServerUser> ServerUserList = new Dictionary<IrcClient, ServerUser>();

        /// <summary>
        /// The different types of messages
        /// </summary>
        internal enum MessageType
        {
            /// <summary>
            /// Unknown
            /// </summary>
            None,

            /// <summary>
            /// A normal message
            /// </summary>
            Message,

            /// <summary>
            /// A chat action
            /// </summary>
            ChatAction,

            /// <summary>
            /// A notice
            /// </summary>
            Notice
        }

        internal static void Add(ServerUser serverUser)
        {
            var client = serverUser.Client;

            ServerUserList.Add(client, serverUser);
            Debug.WriteLine("Added serverUser {0} to dispatcher", serverUser);

            client.Connected += OnConnected;
            client.GotChannelListBegin += OnChannelListBegin;
            client.GotChannelListEnd += OnChannelListEnd;
            client.GotChannelListEntry += OnChannelListEntry;
            client.GotChannelTopicChange += OnChannelTopicChange;
            client.GotChatAction += OnChatAction;
            client.GotInvitation += OnInvitation;
            client.GotIrcError += OnIrcError;
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

        private static ServerUser GetServerUser(object sender)
        {
            var client = sender as IrcClient;

            if (client == null)
            {
                throw new ArgumentNullException("sender");
            }

            if (!ServerUserList.ContainsKey(client))
            {
                throw new ArgumentException(string.Format("Client {0} is unknown.", client));
            }

            return ServerUserList[client];
        }

        private static void OnMotdBegin(object sender, EventArgs eventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine("ON_MOTD_BEGIN", serverUser.ToString());
        }

        private static void OnMotdEnd(object sender, EventArgs eventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine("ON_MOTD_END", serverUser.ToString());
        }

        private static void OnChannelListBegin(object sender, EventArgs eventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine("ON_CHANNEL_LIST_BEGIN", serverUser.ToString());
        }

        private static void OnChannelListEnd(object sender, EventArgs eventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine("ON_CHANNEL_LIST_END", serverUser.ToString());
        }

        private static void OnQuit(object sender, QuitEventArgs quitEventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine("ON_QUIT", serverUser.ToString());
            throw new NotImplementedException();
        }

        private static void OnKick(object sender, KickEventArgs kickEventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine("ON_KICK", serverUser.ToString());
            throw new NotImplementedException();
        }

        private static void OnPingReply(object sender, PingReplyEventArgs pingReplyEventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine("ON_PING_REPLY", serverUser.ToString());
            throw new NotImplementedException();
        }

        private static void OnNameListReply(object sender, NameListReplyEventArgs nameListReplyEventArgs)
        {
            var serverUser = GetServerUser(sender);
            var names = string.Join<IrcString>(", ", nameListReplyEventArgs.GetNameList());
            Debug.WriteLine(string.Format("ON_NAME_LIST_REPLY: {0}", names), serverUser.ToString());
        }

        private static void OnNameListEnd(object sender, NameListEndEventArgs nameListEndEventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine(string.Format("ON_NAME_LIST_END: channel {0}", nameListEndEventArgs.Channel), serverUser.ToString());
        }

        private static void OnNameChange(object sender, NameChangeEventArgs nameChangeEventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine("ON_NAME_CHANGE", serverUser.ToString());
            throw new NotImplementedException();
        }

        private static void OnSimpleMessage(object sender, SimpleMessageEventArgs simpleMessageEventArgs)
        {
            var serverUser = GetServerUser(sender);
            serverUser.OnSimpleMessage(simpleMessageEventArgs.Message);
        }

        private static void OnMode(object sender, ModeEventArgs modeEventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine(string.Format("ON_MODE: {0}", modeEventArgs.Command), serverUser.ToString());
        }

        private static void OnMessage(object sender, ChatMessageEventArgs chatMessageEventArgs)
        {
            var serverUser = GetServerUser(sender);
            serverUser.OnMessage(MessageType.Message, chatMessageEventArgs);
        }

        private static void OnChatAction(object sender, ChatMessageEventArgs chatMessageEventArgs)
        {
            var serverUser = GetServerUser(sender);
            serverUser.OnMessage(MessageType.ChatAction, chatMessageEventArgs);
        }

        private static void OnNotice(object sender, ChatMessageEventArgs chatMessageEventArgs)
        {
            var serverUser = GetServerUser(sender);
            serverUser.OnMessage(MessageType.Notice, chatMessageEventArgs);
        }

        private static void OnJoinChannel(object sender, JoinLeaveEventArgs joinLeaveEventArgs)
        {
            var serverUser = GetServerUser(sender);
            serverUser.OnLeaveJoinChannels(true, joinLeaveEventArgs);
        }

        private static void OnLeaveChannel(object sender, JoinLeaveEventArgs joinLeaveEventArgs)
        {
            var serverUser = GetServerUser(sender);
            serverUser.OnLeaveJoinChannels(false, joinLeaveEventArgs);
        }

        private static void OnIrcError(object sender, IrcErrorEventArgs ircErrorEventArgs)
        {
            var serverUser = GetServerUser(sender);
            var parameters = string.Join(" ", ircErrorEventArgs.Data.Parameters);
            Debug.WriteLine(string.Format("ON_IRC_ERROR, Parameters: {0}", parameters), serverUser.ToString());
        }

        private static void OnInvitation(object sender, InvitationEventArgs invitationEventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine("ON_INVITATION", serverUser.ToString());
            throw new NotImplementedException();
        }

        private static void OnChannelTopicChange(object sender, ChannelTopicChangeEventArgs channelTopicChangeEventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine("ON_CHANNEL_TOPIC_CHANGE", serverUser.ToString());
            throw new NotImplementedException();
        }

        private static void OnChannelListEntry(object sender, ChannelListEntryEventArgs channelListEntryEventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine("ON_CHANNEL_LIST_ENTRY", serverUser.ToString());
            throw new NotImplementedException();
        }

        private static void OnConnected(object sender, EventArgs eventArgs)
        {
            var serverUser = GetServerUser(sender);
            Debug.WriteLine("ON_CONNECTED", serverUser.ToString());
        }
    }
}
