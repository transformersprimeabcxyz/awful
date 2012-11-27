using System;
using System.Net;
using System.Net.Browser;
using System.Collections.Generic;
using KollaSoft;

namespace Awful.Core.Web
{
    public static class AwfulWebRequest
    {
        private const string ACCEPT = "text/html, application/xhtml+xml, */*";
        private const string USERAGENT = "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0; Microsoft; XDeviceEmulator)";
        private const string POST_CONTENT_TYPE = "application/x-www-form-urlencoded";

        private static List<Cookie> _cookieJar = new List<Cookie>();
        public static IEnumerable<Cookie> CookieJar { get { return _cookieJar; } }

        public static void SetCookieJar(IEnumerable<Cookie> jar)
        {
            _cookieJar.Clear();
            _cookieJar.AddRange(jar);
        }

        public static bool CanAuthenticate { get { return !_cookieJar.IsNullOrEmpty(); } }

        private static CookieContainer GetCookiesForUri(Uri uri)
        {
            var container = new CookieContainer();
            var cookies = CookieJar;
            foreach (Cookie cookie in cookies) { container.Add(uri, cookie); }
            return container;
        }

        public static HttpWebRequest CreateGetRequest(string url)
        {
            var uri = new Uri(url);
            var request = (HttpWebRequest)WebRequestCreator.ClientHttp.Create(uri);
            request.Accept = ACCEPT;
            request.UserAgent = USERAGENT;
            request.CookieContainer = GetCookiesForUri(uri);
            request.Method = "GET";
            request.UseDefaultCredentials = false;
            return request;
        }

        public static HttpWebRequest CreatePostRequest(string url)
        {
            var uri = new Uri(url);
            var request = (HttpWebRequest)WebRequestCreator.ClientHttp.Create(uri);
            request.Accept = ACCEPT;
            request.UserAgent = USERAGENT;
            request.CookieContainer = GetCookiesForUri(uri);
            request.Method = "POST";
            request.ContentType = POST_CONTENT_TYPE;
            request.UseDefaultCredentials = false;
            return request;
        }

        public static HttpWebRequest CreateFormDataPostRequest(string url, string contentType)
        {
            var uri = new Uri(url);
            var request = (HttpWebRequest)WebRequestCreator.ClientHttp.Create(uri);
            request.Accept = ACCEPT;
            request.UserAgent = USERAGENT;
            request.CookieContainer = GetCookiesForUri(uri);
            request.Method = "POST";
            request.ContentType = contentType;
            request.UseDefaultCredentials = false;
            return request;
        }
    }
}
