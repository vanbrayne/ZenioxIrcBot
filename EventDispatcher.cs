using System;

namespace ZenioxBot
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    using NetIrc2;
    using NetIrc2.Events;

    /// <summary>
    /// Dispatches all events to the right <see cref="ServerUser"/>.
    /// </summary>
    public static class EventDispatcher
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
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Trace.WriteLine(information, serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnMotdEnd(object sender, EventArgs eventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Trace.WriteLine(information, serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnChannelListBegin(object sender, EventArgs eventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Trace.WriteLine(information, serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnChannelListEnd(object sender, EventArgs eventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Trace.WriteLine(information, serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnQuit(object sender, QuitEventArgs quitEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Trace.WriteLine(information, serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnKick(object sender, KickEventArgs kickEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Trace.WriteLine(information, serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnPingReply(object sender, PingReplyEventArgs pingReplyEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Trace.WriteLine(information, serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnNameListReply(object sender, NameListReplyEventArgs nameListReplyEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                serverUser.OnNameList(nameListReplyEventArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnNameListEnd(object sender, NameListEndEventArgs nameListEndEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Trace.WriteLine(information, serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnNameChange(object sender, NameChangeEventArgs nameChangeEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Trace.WriteLine(information, serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnSimpleMessage(object sender, SimpleMessageEventArgs simpleMessageEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                serverUser.OnSimpleMessage(simpleMessageEventArgs.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnMode(object sender, ModeEventArgs modeEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Debug.WriteLine(string.Format("ON_MODE: {0}", modeEventArgs.Command), serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnMessage(object sender, ChatMessageEventArgs chatMessageEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                serverUser.OnMessage(MessageType.Message, chatMessageEventArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnChatAction(object sender, ChatMessageEventArgs chatMessageEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                serverUser.OnMessage(MessageType.ChatAction, chatMessageEventArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnNotice(object sender, ChatMessageEventArgs chatMessageEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                serverUser.OnMessage(MessageType.Notice, chatMessageEventArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnJoinChannel(object sender, JoinLeaveEventArgs joinLeaveEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                serverUser.OnLeaveJoinChannels(true, joinLeaveEventArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnLeaveChannel(object sender, JoinLeaveEventArgs joinLeaveEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                serverUser.OnLeaveJoinChannels(false, joinLeaveEventArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnIrcError(object sender, IrcErrorEventArgs ircErrorEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                var parameters = string.Join(" ", ircErrorEventArgs.Data.Parameters);
                Debug.WriteLine(string.Format("ON_IRC_ERROR, Parameters: {0}", parameters), serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnInvitation(object sender, InvitationEventArgs invitationEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Trace.WriteLine(information, serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnChannelTopicChange(object sender, ChannelTopicChangeEventArgs channelTopicChangeEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Trace.WriteLine(information, serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnChannelListEntry(object sender, ChannelListEntryEventArgs channelListEntryEventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Trace.WriteLine(information, serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }

        private static void OnConnected(object sender, EventArgs eventArgs)
        {
            var information = MethodBase.GetCurrentMethod().Name;
            try
            {
                var serverUser = GetServerUser(sender);
                Debug.WriteLine("ON_CONNECTED", serverUser.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} exception {1}", information, ex.Message);
            }
        }
    }
}
