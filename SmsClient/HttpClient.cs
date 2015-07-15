using System;
using System.Collections.Generic;
using System.Net;
using System.Web;

namespace SmsClient
{
    /// <summary>
    /// HttpClient
    /// <para>Handle http connection, data, request</para>
    /// </summary>
    class HttpClient : IDisposable
    {
        private MyWebClient webClient;

        private List<Cookie> Cookies = new List<Cookie>();

        private string _RedirectLocation = String.Empty;

        /// <summary>
        /// Http response is redirect to another location
        /// </summary>
        public bool IsRedirect { get { return this._RedirectLocation.Length != 0; }
            private set {
                if (value) throw new Exception("Required Location");
                
                this._RedirectLocation = String.Empty;
            }
        }

        /// <summary>
        /// Redirection location in http response
        /// </summary>
        public string RedirectLocation { get { return this._RedirectLocation; } private set { this._RedirectLocation = value; } }

        /// <summary>
        /// HttpClient
        /// </summary>
        public HttpClient()
        {
            webClient = new MyWebClient();
        }

        /// <summary>
        /// Get data from url in string using GET method
        /// </summary>
        public string GetString(string url)
        {
            return RequestString(url, HttpMethod.GET, new Dictionary<string, string>());
        }

        /// <summary>
        /// Post data on url and retrive response string using POST method
        /// </summary>
        public string GetStringPost(string url, Dictionary<string, string> data)
        {
            return RequestString(url, HttpMethod.POST, data);
        }

        /// <summary>
        /// Request url to retrive string
        /// </summary>
        public string RequestString(string url, HttpMethod method, Dictionary<string, string> data)
        {
            this.IsRedirect = false;

            List<string> paramList = new List<string>();

            foreach (KeyValuePair<string, string> keydata in data)
            {
                paramList.Add(string.Format("{0}={1}", HttpUtility.UrlEncode(keydata.Key), HttpUtility.UrlEncode(keydata.Value)));
            }

            // add cookies in request header
            AddCookieRequestHeader(url);

            string ResponseData;

            switch (method)
            {
                case HttpMethod.GET:
                    ResponseData = webClient.DownloadString(url);
                    break;
                case HttpMethod.POST:
                    webClient.Headers.Add("Content-Type: application/x-www-form-urlencoded");
                    //webClient.Headers.Add("Cookie: __gads=ID=86f2934d5e665dc1:T=1431876705:S=ALNI_MaIBwpXjF2nL04_nKhg_yeVqnxAsQ; LastLoginCookie=\"17-05-2015-901-21:30-901-google chrome 42.0.2311.152-901-1.39.97.23-901-25-11-2010\"; JSESSIONID=HH~1DBED7E9C40E4C9F144A345F1777FE1B.8508; _ga=GA1.2.998331835.1431876716; _gat=1; adCookie=5");
                    ResponseData = webClient.UploadString(url, method.ToString(), string.Join("&", paramList.ToArray()));
                    break;
                default:
                    throw new Exception("Invalid method");
            }

            // handle response header
            HandleHeader(url, webClient.ResponseHeaders);

            return ResponseData;
        }

        /// <summary>
        /// Handle response headers
        /// </summary>
        private void HandleHeader(string url, WebHeaderCollection headers)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                string Key = headers.GetKey(i);
                string Value = headers.Get(i);

                switch (Key)
                {
                    case "Set-Cookie":
                        HandleResponseSetCookie(url, Value);
                        break;
                    case "Location":
                        this.RedirectLocation = Value;
                        break;
                }
            }
        }

        /// <summary>
        /// Handle response cookies
        /// </summary>
        private void HandleResponseSetCookie(string url, string CookieContent)
        {
            string[] actions = CookieContent.Split(';');

            // trim data
            for (int i = 0; i < actions.Length; i++)
                actions[i] = actions[i].Trim();

            Cookie cookie = new Cookie();
            for (int i = 0; i < actions.Length; i++)
            {
                string[] KeyPair = actions[i].Split('=');

                if (i == 0)
                {
                    cookie.Key = KeyPair[0];
                    cookie.Value = KeyPair[1];

                    Uri uri = new Uri(url);
                    cookie.Host = uri.Host;

                    continue;
                }

                switch (KeyPair[0])
                {
                    case "path":
                        cookie.Path = KeyPair[1];
                        break;

                    case "HttpOnly":
                        cookie.HttpOnly = true;
                        break;
                }
            }

            Cookie c = this.Cookies.Find(x => x.Host == cookie.Host && x.Key == cookie.Key);

            if (c != null)
                this.Cookies.Remove(c);

            this.Cookies.Add(cookie);
        }

        /// <summary>
        /// Add cookie in request header before request
        /// </summary>
        private void AddCookieRequestHeader(string url)
        {
            Uri uri = new Uri(url);

            List<Cookie> cookies = this.Cookies.FindAll(x => x.Host == uri.Host);

            if (cookies.Count > 0)
            {
                List<string> CookieRequest = new List<string>();

                foreach (Cookie c in cookies)
                {
                    CookieRequest.Add(string.Format("{0}={1}", c.Key, c.Value));
                }

                webClient.Headers.Add("Cookie", string.Join("; ", CookieRequest.ToArray()));
            }
        }

        public void Dispose()
        {
            webClient.Dispose();
        }

        /// <summary>
        /// Http methods
        /// </summary>
        [Flags]
        public enum HttpMethod
        {
            /// <summary>
            /// GET Method
            /// </summary>
            GET = 0,
            
            /// <summary>
            /// POST Method
            /// </summary>
            POST = 1
        }

        /// <summary>
        /// Cookie class
        /// </summary>
        private class Cookie
        {
            /// <summary>
            /// Key of cookie
            /// </summary>
            public string Key;

            /// <summary>
            /// Value of cookie
            /// </summary>
            public string Value;

            /// <summary>
            /// Cookie path
            /// </summary>
            public string Path;

            /// <summary>
            /// Host for cookie
            /// </summary>
            public string Host;

            /// <summary>
            /// Cookie only for http connection
            /// <para>Cookie request not send if HttpOnly true and connection is secure connection</para>
            /// </summary>
            public bool HttpOnly = false;

            public override string ToString()
            {
                return this.Key;
            }
        }

        /// <summary>
        /// Customize WebClient to retrive date throw http connection
        /// </summary>
        private class MyWebClient : WebClient
        {
            /// <summary>
            /// WebRequest method
            /// <para>Disalbed auto redirection</para>
            /// </summary>
            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = (HttpWebRequest)base.GetWebRequest(address);
                request.AllowAutoRedirect = false;
                return request;
            }
        }
    }
}
