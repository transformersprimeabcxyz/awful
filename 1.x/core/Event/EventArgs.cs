﻿using System;
using Awful.Core.Models;
using System.Collections;
using System.Collections.Generic;

namespace Awful.Core.Event
{
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public T Value { get; private set; }
        public ValueChangedEventArgs(T value) { this.Value = value; }
    }

    public sealed class ProfileChangedEventArgs : ValueChangedEventArgs<AwfulProfile>
    {
        public IList<System.Net.Cookie> Cookies { get; private set; }
        public ProfileChangedEventArgs(AwfulProfile profile, IList<System.Net.Cookie> cookies)
            : base(profile) { this.Cookies = cookies; }
    }

    public sealed class CollectionUpdatedEventArgs<T> : EventArgs
    {
        public ICollection<T> Collection { get; private set; }
        public CollectionUpdatedEventArgs(ICollection<T> collection) { this.Collection = collection; }
    }

    public sealed class HtmlGeneratedEventArgs : EventArgs
    {
        public string Html { get; private set; }
        public HtmlGeneratedEventArgs(string html)
        {
            this.Html = html;
        }
    }

    public sealed class ForumsRefreshedEventArgs<T> : EventArgs
        where T : ForumData
    {
        public IEnumerable<T> Forums { get; private set; }
        public ForumsRefreshedEventArgs(IEnumerable<T> forums)
        {
            this.Forums = forums;
        }
    }

    public sealed class ForumFavoriteChangedEventArgs : EventArgs
    {
        public ForumData Forum { get; private set; }
        public bool OldValue { get; private set; }
        public bool NewValue { get; private set; }
        public bool Handled { get; set; }

        public ForumFavoriteChangedEventArgs(ForumData forum, bool oldValue, bool newValue)
        {
            this.Forum = forum;
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }
    }

    public sealed class ContentLoadedEventArgs : EventArgs
    {
        public string Content { get; private set; }
        public ContentLoadedEventArgs(string content)
        {
            Content = content;
        }
    }

    public sealed class PostContentLoadingEventArgs : EventArgs
    {
        public int OldIndex { get; private set; }
        public int NewIndex { get; private set; }
      
        public PostContentLoadingEventArgs(int oldIndex, int newIndex)
        {
            this.OldIndex = oldIndex;
            this.NewIndex = newIndex;
        }
    }

    public sealed class PostContentLoadedEventArgs : EventArgs
    {
        public int OldIndex { get; private set; }
        public int NewIndex { get; private set; }
        public string Content { get; private set; }
        public bool IsEditable { get; private set; }
        public PostContentLoadedEventArgs(int oldIndex, int newIndex, string content, bool isEditable)
        {
            this.OldIndex = oldIndex;
            this.NewIndex = newIndex;
            this.Content = content;
            this.IsEditable = isEditable;
        }
    }

    public sealed class ActionResultEventArgs : EventArgs
    {
        public ActionResult Result { get; private set; }
        public ActionResultEventArgs(ActionResult result)
        {
            Result = result;
        }
    }

    public enum RequestType { Quote, Edit }
    public sealed class WebContentLoadedEventArgs : EventArgs
    {
        public RequestType Type { get; private set; }
        public string Content { get; private set; }
        public WebContentLoadedEventArgs(RequestType type, string content)
        {
            Type = type;
            Content = content;
        }
    }

    public sealed class PageLoadedEventArgs : EventArgs
    {
        public ThreadData Thread { get; private set; }
        public string Content { get; private set; }
        public int PageNumber { get; private set; }
        public PageLoadedEventArgs(ThreadData thread, int page, string content)
        {
            Thread = thread;
            PageNumber = page;
            Content = content;
        }
    }

    public enum MediaType { Image, Video, Other }
    public sealed class MediaSelectedEventArgs : EventArgs
    {
        public MediaType Type { get; private set; }
        public string Uri { get; private set; }

        public MediaSelectedEventArgs(MediaType type, string uri)
        {
            Type = type;
            Uri = uri;
        }
    }

    public sealed class UrlEventArgs : EventArgs
    {
        public string Url { get; private set; }
        public bool IsHttpOrHttps { get; private set; }
        public UrlEventArgs(string url)
        {
            Url = url;

            if (url.Contains("://") && (!url.Contains("http")))
                IsHttpOrHttps = false;
            else
                IsHttpOrHttps = true;
        }
    }
}
