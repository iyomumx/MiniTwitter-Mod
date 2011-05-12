using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MiniTwitter.Input
{
    public static class Commands
    {
        private static readonly RoutedCommand update = new RoutedCommand("Update", typeof(Commands));

        public static RoutedCommand Update
        {
            get { return Commands.update; }
        }

        private static readonly RoutedCommand refresh = new RoutedCommand("Refresh", typeof(Commands));

        public static RoutedCommand Refresh
        {
            get { return Commands.refresh; }
        }

        private static readonly RoutedCommand reply = new RoutedCommand("Reply", typeof(Commands));

        public static RoutedCommand Reply
        {
            get { return Commands.reply; }
        }
        //Reply All增加代码：
        private static readonly RoutedCommand replyAll = new RoutedCommand("ReplyAll", typeof(Commands));

        public static RoutedCommand ReplyAll
        {
            get { return Commands.replyAll; }
        }
        //End of Reply All增加代码
        private static readonly RoutedCommand reTweet = new RoutedCommand("ReTweet", typeof(Commands));

        public static RoutedCommand ReTweet
        {
            get { return Commands.reTweet; }
        }

        private static readonly RoutedCommand reTweetApi = new RoutedCommand("ReTweetApi", typeof(Commands));

        public static RoutedCommand ReTweetApi
        {
            get { return Commands.reTweetApi; }
        }

        private static readonly RoutedCommand replyMessage = new RoutedCommand("ReplyMessage", typeof(Commands));

        public static RoutedCommand ReplyMessage
        {
            get { return Commands.replyMessage; }
        }

        private static readonly RoutedCommand delete = new RoutedCommand("Delete", typeof(Commands));

        public static RoutedCommand Delete
        {
            get { return Commands.delete; }
        }

        private static readonly RoutedCommand favorite = new RoutedCommand("Favorite", typeof(Commands));

        public static RoutedCommand Favorite
        {
            get { return Commands.favorite; }
        }

        private static readonly RoutedCommand timelineStyle = new RoutedCommand("TimelineStyle", typeof(Commands));

        public static RoutedCommand TimelineStyle
        {
            get { return Commands.timelineStyle; }
        }

        private static readonly RoutedCommand copy = new RoutedCommand("Copy", typeof(Commands));

        public static RoutedCommand Copy
        {
            get { return Commands.copy; }
        }

        private static readonly RoutedCommand copyUrl = new RoutedCommand("CopyUrl", typeof(Commands));

        public static RoutedCommand CopyUrl
        {
            get { return Commands.copyUrl; }
        }

        private static readonly RoutedCommand moveToUserPage = new RoutedCommand("MoveToUserPage", typeof(Commands));

        public static RoutedCommand MoveToUserPage
        {
            get { return Commands.moveToUserPage; }
        }

        private static readonly RoutedCommand moveToStatusPage = new RoutedCommand("MoveToStatusPage", typeof(Commands));

        public static RoutedCommand MoveToStatusPage
        {
            get { return Commands.moveToStatusPage; }
        }

        private static readonly RoutedCommand moveToReplyPage = new RoutedCommand("MoveToReplyPage", typeof(Commands));

        public static RoutedCommand MoveToReplyPage
        {
            get { return Commands.moveToReplyPage; }
        }

        private static readonly RoutedCommand moveToSourcePage = new RoutedCommand("MoveToSourcePage", typeof(Commands));

        public static RoutedCommand MoveToSourcePage
        {
            get { return Commands.moveToSourcePage; }
        }

        private static readonly RoutedCommand readAll = new RoutedCommand("ReadAll", typeof(Commands));

        public static RoutedCommand ReadAll
        {
            get { return Commands.readAll; }
        }

        private static readonly RoutedCommand sortCategory = new RoutedCommand("SortCategory", typeof(Commands));

        public static RoutedCommand SortCategory
        {
            get { return Commands.sortCategory; }
        }

        private static readonly RoutedCommand sortDirection = new RoutedCommand("SortDirection", typeof(Commands));

        public static RoutedCommand SortDirection
        {
            get { return Commands.sortDirection; }
        }

        private static readonly RoutedCommand addTimeline = new RoutedCommand("AddTimeline", typeof(Commands));

        public static RoutedCommand AddTimeline
        {
            get { return Commands.addTimeline; }
        }

        private static readonly RoutedCommand editTimeline = new RoutedCommand("EditTimeline", typeof(Commands));

        public static RoutedCommand EditTimeline
        {
            get { return Commands.editTimeline; }
        }

        private static readonly RoutedCommand deleteTimeline = new RoutedCommand("DeleteTimeline", typeof(Commands));

        public static RoutedCommand DeleteTimeline
        {
            get { return Commands.deleteTimeline; }
        }

        private static readonly RoutedCommand clearTimeline = new RoutedCommand("ClearTimeline", typeof(Commands));

        public static RoutedCommand ClearTimeline
        {
            get { return Commands.clearTimeline; }
        }

        private static readonly RoutedCommand moveTop = new RoutedCommand("MoveTop", typeof(Commands));

        public static RoutedCommand MoveTop
        {
            get { return Commands.moveTop; }
        }

        private static readonly RoutedCommand moveBottom = new RoutedCommand("MoveBottom", typeof(Commands));

        public static RoutedCommand MoveBottom
        {
            get { return Commands.moveBottom; }
        }

        private static readonly RoutedCommand search = new RoutedCommand("Search", typeof(Commands));

        public static RoutedCommand Search
        {
            get { return Commands.search; }
        }

        private static readonly RoutedCommand scrollUp = new RoutedCommand("ScrollUp", typeof(Commands));

        public static RoutedCommand ScrollUp
        {
            get { return Commands.scrollUp; }
        }

        private static readonly RoutedCommand scrollDown = new RoutedCommand("ScrollDown", typeof(Commands));

        public static RoutedCommand ScrollDown
        {
            get { return Commands.scrollDown; }
        }

        private static readonly RoutedCommand apportion = new RoutedCommand("Apportion", typeof(Commands));

        public static RoutedCommand Apportion
        {
            get { return Commands.apportion; }
        }

        private static readonly RoutedCommand footer = new RoutedCommand("Footer", typeof(Commands));

        public static RoutedCommand Footer
        {
            get { return Commands.footer; }
        }

        private static readonly RoutedCommand twitpic = new RoutedCommand("Twitpic", typeof(Commands));

        public static RoutedCommand Twitpic
        {
            get { return Commands.twitpic; }
        }
        //img.ly增加代码
        private static readonly RoutedCommand imgly = new RoutedCommand("Imgly", typeof(Commands));

        public static RoutedCommand Imgly
        {
            get { return Commands.imgly; }
        }
        //img.ly增加代码 结束
        private static readonly RoutedCommand playTitle = new RoutedCommand("PlayTitle", typeof(Commands));

        public static RoutedCommand PlayTitle
        {
            get { return Commands.playTitle; }
        }

        private static readonly RoutedCommand inReplyTo = new RoutedCommand("InReplyTo", typeof(Commands));

        public static RoutedCommand InReplyTo
        {
            get { return Commands.inReplyTo; }
        }

        private static readonly RoutedCommand beReplied = new RoutedCommand("BeReplied", typeof(Commands));

        public static RoutedCommand BeReplied
        {
            get { return Commands.beReplied; }
        }

        private static readonly RoutedCommand _gpsLocation = new RoutedCommand("GpsLocation", typeof(Commands));

        public static RoutedCommand GpsLocation
        {
            get { return Commands._gpsLocation; }
        }

        private static readonly RoutedCommand _follow = new RoutedCommand("Follow", typeof(Commands));

        public static RoutedCommand Follow
        {
            get { return Commands._follow; }
        }

        private static readonly RoutedCommand _unfollow = new RoutedCommand("Unfollow", typeof(Commands));

        public static RoutedCommand Unfollow
        {
            get { return Commands._unfollow; }
        }

        private static readonly RoutedCommand _block = new RoutedCommand("Block", typeof(Commands));

        public static RoutedCommand Block
        {
            get { return Commands._block; }
        }
        //ReportSpam 添加代码

        private static readonly RoutedCommand _reportSpam = new RoutedCommand("ReportSpam", typeof(Commands));

        public static RoutedCommand ReportSpam
        {
            get { return Commands._reportSpam; }
        }

        //添加代码结束
        private static readonly RoutedCommand _hashtag = new RoutedCommand("Hashtag", typeof(Commands));

        public static RoutedCommand Hashtag
        {
            get { return Commands._hashtag; }
        }
    }
}
