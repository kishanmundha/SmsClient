using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SmsClient
{
    /// <summary>
    /// Way 2 SMS Client Service
    /// </summary>
    public class SmsClientWay2sms : ISmsClient
    {
        private AuthenticationState _authenticationState = AuthenticationState.NotChecked;

        public AuthenticationState authenticationState
        {
            get
            {
                return this._authenticationState;
            }
            private set
            {
                this._authenticationState = value;
            }
        }

        private HttpClient httpClient;

        private string PrimaryHost = "http://way2sms.com/";
        private string FreeWay2SmsHost = String.Empty;

        private string Token = String.Empty;

        public SmsClientWay2sms()
        {
            httpClient = new HttpClient();
        }

        /// <summary>
        /// Set Authentication to use service
        /// </summary>
        /// <param name="UserId">Mobile Number</param>
        /// <param name="Password">Password</param>
        public void SetAuthentication(string UserId, string Password)
        {
            if (!Validation.IsValidMobileNumber(UserId))
                throw new Exception("UserId must be a mobile number");

            this.authenticationState = AuthenticationState.NotAuthenticated;

            string ResponseString = String.Empty;

            this.FreeWay2SmsHost = this.PrimaryHost;

            // get free server
            ResponseString = httpClient.GetString(this.PrimaryHost);
            if (httpClient.IsRedirect)
            {
                FreeWay2SmsHost = httpClient.RedirectLocation;
                if (!FreeWay2SmsHost.EndsWith("/"))
                    FreeWay2SmsHost += "/";
            }

            // get cookie and content from login page
            ResponseString = httpClient.GetString(FreeWay2SmsHost + "content/index.html");

            // post authentication keys
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("username", UserId);
            data.Add("password", Password);

            ResponseString = httpClient.GetStringPost(FreeWay2SmsHost + "content/Login1.action", data);

            // Identify status by redirect location
            if (!httpClient.IsRedirect)
                throw new Exception("Unknow Error on attempt login");

            /*
            -- input wrong (No User Id passed or username is not mobile number)
            http://site21.way2sms.com/entry.action?id=n3ps&ec=0001&username=

            -- invalid password
            http://site21.way2sms.com/wpwd.action?id=56tb&username=9530111056&ec=0004

            -- no user found
            http://site21.way2sms.com/nruser.action?id=s2tq&username=9530111057

            -- Success
            http://site21.way2sms.com/ebrdg.action;jsessionid=FC0243B583CA6B5F8A2B5ACF10925E88.w806?id=FC0243B583CA6B5F8A2B5ACF10925E88.w806
            http://site21.way2sms.com/ebrdg.action;jsessionid=C6D681BF5FB3D760630B11FE264162AA.w805?id=C6D681BF5FB3D760630B11FE264162AA.w805
            */

            if (httpClient.RedirectLocation.IndexOf("entry.action") != -1)
                throw new Exception("Authentication failed!\n\nMake sure mobile number and password was typed correctly");

            if (httpClient.RedirectLocation.IndexOf("nruser.action") != -1)
                throw new Exception("Authentication failed!\n\nNo such user found");

            if (httpClient.RedirectLocation.IndexOf("wpwd.action") != -1)
                throw new Exception("Authentication failed!\n\nInvalid password");

            if (httpClient.RedirectLocation.IndexOf("ebrdg.action") == -1)
                throw new Exception("Authentication failed!\n\nUnknow issue on authentication");

            Regex regex = new Regex(@"jsessionid=([a-zA-Z0-9\.]+)");

            Match m = regex.Match(httpClient.RedirectLocation);

            this.Token = m.Groups[1].Value;

            this.authenticationState = AuthenticationState.Authenticated;
        }

        /// <summary>
        /// Send Text Message
        /// </summary>
        /// <param name="Number">Mobile Number</param>
        /// <param name="Message">Message Content</param>
        public void SendMessage(string Number, string Message)
        {
            if (this.authenticationState != AuthenticationState.Authenticated)
                throw new Exception("Authentication required before send message");

            if (!Validation.IsValidMobileNumber(Number))
                throw new Exception("Mobile number was not in proper format");

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("ssaction", "ss");
            data.Add("Token", this.Token);
            data.Add("mobile", Number);
            data.Add("message", Message);
            data.Add("msgLen", "128");

            httpClient.GetStringPost(this.FreeWay2SmsHost + "smstoss.action", data);

            if(httpClient.IsRedirect)
            {
                string RedirectUrl = httpClient.RedirectLocation;
            }
        }

        public void LogOut()
        {
            if (this.authenticationState != AuthenticationState.Authenticated)
                throw new Exception("Not logged in");

            httpClient.GetString(this.FreeWay2SmsHost + "entry");

            this.authenticationState = AuthenticationState.LogOut;
        }

        public void Dispose()
        {
            if (this.authenticationState == AuthenticationState.Authenticated)
                this.LogOut();

            httpClient.Dispose();
        }
    }
}
