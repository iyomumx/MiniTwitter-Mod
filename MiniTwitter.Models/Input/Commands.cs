/*
 * 此文件由Apache License 2.0授权的文件修改而来
 * 根据协议要求在此声明此文件已被修改
 * 
 * 未被修改的原始文件可以在
 * https://github.com/iyomumx/MiniTwitter-Mod/tree/minitwitter
 * 找到
*/

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

        private static readonly RoutedCommand updateWithMedia = new RoutedCommand("UpdateWithMedia", typeof(Commands));

        public static RoutedCommand UpdateWithMedia
        {
            get { return Commands.updateWithMedia; }
        }

        private static readonly RoutedCommand updateWithClipBoardMedia = new RoutedCommand("UpdateWithClipBoardMedia", typeof(Commands));

        public static RoutedCommand UpdateWithClipBoardMedia
        {
            get { return Commands.updateWithClipBoardMedia; }
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

        private static readonly RoutedCommand unRetweetApi = new RoutedCommand("UnReTweetApi", typeof(Commands));

        public static RoutedCommand UnReTweetApi
        {
            get { return Commands.unRetweetApi; }
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

        private static readonly RoutedCommand moveToUserPageByName = new RoutedCommand("MoveToUserPageByName", typeof(Commands));

        public static RoutedCommand MoveToUserPageByName
        {
            get { return Commands.moveToUserPageByName; }
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

        private static readonly RoutedCommand viewConversation = new RoutedCommand("ViewConversation", typeof(Commands));

        public static RoutedCommand ViewConversation
        {
            get { return Commands.viewConversation; }
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

        private static readonly RoutedCommand _followbyname = new RoutedCommand("FollowByName", typeof(Commands));

        public static RoutedCommand FollowByName
        {
            get { return Commands._followbyname; }
        }

        private static readonly RoutedCommand _unfollow = new RoutedCommand("Unfollow", typeof(Commands));

        public static RoutedCommand Unfollow
        {
            get { return Commands._unfollow; }
        }

        private static readonly RoutedCommand _unfollowbyname = new RoutedCommand("UnfollowByName", typeof(Commands));

        public static RoutedCommand UnfollowByName
        {
            get { return Commands._unfollowbyname; }
        }

        private static readonly RoutedCommand _block = new RoutedCommand("Block", typeof(Commands));

        public static RoutedCommand Block
        {
            get { return Commands._block; }
        }

        private static readonly RoutedCommand _blockbyname = new RoutedCommand("BlockByName", typeof(Commands));

        public static RoutedCommand BlockByName
        {
            get { return Commands._blockbyname; }
        }
        //ReportSpam 添加代码

        private static readonly RoutedCommand _reportSpam = new RoutedCommand("ReportSpam", typeof(Commands));

        public static RoutedCommand ReportSpam
        {
            get { return Commands._reportSpam; }
        }

        private static readonly RoutedCommand _reportSpamByName = new RoutedCommand("ReportSpamByName", typeof(Commands));

        public static RoutedCommand ReportSpamByName
        {
            get { return Commands._reportSpamByName; }
        }

        //添加代码结束
        private static readonly RoutedCommand _hashtag = new RoutedCommand("Hashtag", typeof(Commands));

        public static RoutedCommand Hashtag
        {
            get { return Commands._hashtag; }
        }

        private static readonly RoutedCommand _viewUser = new RoutedCommand("ViewUser", typeof(Commands));

        public static RoutedCommand ViewUser
        {
            get { return Commands._viewUser; }
        }

        private static readonly RoutedCommand _viewUserByName = new RoutedCommand("ViewUserByName", typeof(Commands));

        public static RoutedCommand ViewUserByName
        {
            get { return Commands._viewUserByName; }
        }

        private static readonly RoutedCommand _setTweetText = new RoutedCommand("SetTweetText", typeof(Commands));

        public static RoutedCommand SetTweetText
        {
            get { return Commands._setTweetText; }
        }

        private static readonly RoutedCommand _filterUser = new RoutedCommand("FilterUser", typeof(Commands));

        public static RoutedCommand FilterUser
        {
            get { return Commands._filterUser; }
        }

        private static readonly RoutedCommand _filterUserByName = new RoutedCommand("FilterUserByName", typeof(Commands));

        public static RoutedCommand FilterUserByName
        {
            get { return Commands._filterUserByName; }
        }

        private static readonly RoutedCommand _filterTag = new RoutedCommand("FilterTag", typeof(Commands));

        public static RoutedCommand FilterTag
        {
            get { return Commands._filterTag; }
        }

        private static readonly RoutedCommand _globalFilterUser = new RoutedCommand("GlobalFilterUser", typeof(Commands));

        public static RoutedCommand GlobalFilterUser
        {
            get { return Commands._globalFilterUser; }
        }

        private static readonly RoutedCommand _globalFilterUserByName = new RoutedCommand("GlobalFilterUserByName", typeof(Commands));

        public static RoutedCommand GlobalFilterUserByName
        {
            get { return Commands._globalFilterUserByName; }
        }

        private static readonly RoutedCommand _globalFilterTag = new RoutedCommand("GlobalFilterTag", typeof(Commands));

        public static RoutedCommand GlobalFilterTag
        {
            get { return Commands._globalFilterTag; }
        }

        private static readonly RoutedCommand _globalFilterAndBlockUser = new RoutedCommand("GlobalFilterAndBlockUser", typeof(Commands));

        public static RoutedCommand GlobalFilterAndBlockUser
        {
            get { return Commands._globalFilterAndBlockUser; }
        }

        private static readonly RoutedCommand _globalFilterAndBlockUserByName = new RoutedCommand("GlobalFilterAndBlockUserByName", typeof(Commands));

        public static RoutedCommand GlobalFilterAndBlockUserByName
        {
            get { return Commands._globalFilterAndBlockUserByName; }
        }

        private static readonly RoutedCommand _navigateTo = new RoutedCommand("NavigateTo", typeof(Commands));

        public static RoutedCommand NavigateTo
        {
            get { return Commands._navigateTo; }
        }

        private static readonly RoutedCommand _viewImage = new RoutedCommand("ViewImage", typeof(Commands));

        public static RoutedCommand ViewImage
        {
            get { return Commands._viewImage; }
        }

        private static readonly RoutedCommand _copyImage = new RoutedCommand("CopyImage", typeof(Commands));

        public static RoutedCommand CopyImage
        {
            get { return Commands._copyImage; }
        }
    }
}
