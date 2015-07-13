using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SmsClient
{
    class SmsClient160By2 : ISmsClient
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

        private string PrimaryHost = "http://www.160by2.com/";

        private string Token = String.Empty;

        public SmsClient160By2()
        {
            //throw new NotImplementedException();

            this.httpClient = new HttpClient();
        }

        public void SetAuthentication(string UserId, string Password)
        {
            if (!Validation.IsValidMobileNumber(UserId))
                throw new Exception("UserId must be a mobile number");

            this.authenticationState = AuthenticationState.NotAuthenticated;

            string ResponseString = String.Empty;

            // get cookie and content from login page
            ResponseString = httpClient.GetString(this.PrimaryHost + "Login");

            // post authentication keys
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("rssData", "");
            data.Add("username", UserId);
            data.Add("password", Password);
            data.Add("token", "");
            data.Add("hidDetails", "");
            data.Add("fbid", "");
            data.Add("userMobile1", "");
            data.Add("userPwd1", "");

            ResponseString = httpClient.GetStringPost(this.PrimaryHost + "re-login", data);

            // Identify status by redirect location
            if (!httpClient.IsRedirect)
                throw new Exception("Unknow Error on attempt login");

            /*
            -- Number not registered
            http://www.160by2.com/LoginReg

            -- invalid password (forget password)
            http://www.160by2.com/LoginForgot

            -- Success
             * Email Verification page (we can skip this)
            http://www.160by2.com/PostLoginEmailVerification.action?id=74EDB163CA495F47EB0A44718F4D1EB4.8514
            */

            if (httpClient.RedirectLocation.IndexOf("LoginReg") != -1)
                throw new Exception("Authentication failed!\n\nNo such user found");

            if (httpClient.RedirectLocation.IndexOf("LoginForgot") != -1)
                throw new Exception("Authentication failed!\n\nInvalid password");

            //if (httpClient.RedirectLocation.IndexOf("ebrdg.action") == -1)
            //    throw new Exception("Authentication failed!\n\nUnknow issue on authentication");

            Regex regex = new Regex(@"id=([a-zA-Z0-9\.]+)");

            Match m = regex.Match(httpClient.RedirectLocation);

            this.Token = m.Groups[1].Value;

            this.authenticationState = AuthenticationState.Authenticated;
        }

        public void SendMessage(string Number, string Message)
        {
            //if (this.authenticationState != AuthenticationState.Authenticated)
            //    throw new Exception("Authentication required before send message");

            //if (!Validation.IsValidMobileNumber(Number))
            //    throw new Exception("Mobile number was not in proper format");

            httpClient.GetString(this.PrimaryHost + "Ramping?id=" + this.Token);

            Dictionary<string, string> data = new Dictionary<string, string>();
            //data.Add("hidSessionId", this.Token);
            //data.Add("maxwellapps", this.Token);
            //data.Add("HAYMRQ", Number);
            //data.Add("sendSMSMsg", Message);
            //data.Add("msgLen", "140");

            // Cookie: __gads=ID=86f2934d5e665dc1:T=1431876705:S=ALNI_MaIBwpXjF2nL04_nKhg_yeVqnxAsQ; LastLoginCookie="17-05-2015-901-21:30-901-google chrome 42.0.2311.152-901-1.39.97.23-901-25-11-2010"; JSESSIONID=HH~1DBED7E9C40E4C9F144A345F1777FE1B.8508; _ga=GA1.2.998331835.1431876716; _gat=1; adCookie=5

            data.Add("hid_exists", "no");
            data.Add("fkapps", "SendSMSDec19");
            data.Add("newsUrl", "");
            data.Add("pageContext", "normal");
            data.Add("linkrs", "");
            data.Add("hidSessionId", this.Token);
            data.Add("msgLen", "140");
            data.Add("maxwellapps", this.Token);
            data.Add("feb2by2action", "sa65sdf656fdfd");
            data.Add("SSLVP", "");
            data.Add("RILRLQ", Number);
            data.Add("sendSMSMsg", Message);
            data.Add("newsExtnUrl", "");
            data.Add("ulCategories", "28");

            //data.Add("messid_0","Morning+is+Gods+way+of+saying0D%0AOne+more+time+Live+life.%0D%0AMake+a+difference.%0D%0ATouch+1+heart.%0D%0AEncourage+1+mind.%0D%0AInspire+1+soul%0D%0AGud+Morg");
            //data.Add("messid_1","Good+Morning%0D%0AGet+up+Get+up%0D%0AGOD+is+waiting+for+U%2Cto+give+a+gift+of+24+GOLDEN+HOURS.Whatever+U+want+do+it+successfully+GOOD+THINGS+FOLLOWS");
            //data.Add("messid_2","Live+4+d+person+who+can+die+4+U%2C%0D%0ASmile+4+d+person+who+cries+4+U%2C%0D%0ALove+d+person+who+love+u+more+then+U%0D%0AGood+Morning.");
            //data.Add("messid_3","On+Your+New+Good+morning+.+Have+a+great%0D%0Aday+ahead%2C+fight+your+challenges%2C+be%0D%0Apositive+and+enjoy+your+family+time.");
            //data.Add("messid_4","Our+blessing+starts+when+we%0D%0Awake+up+in+the+morning.%0D%0Awith+the+sun+shining+bright%0D%0Atelling+you+welcome+to+a+new%0D%0Abeautiful+day....");

            //data.Add("reminderDate","18-05-2015");
            //data.Add("sel_hour","HH");
            //data.Add("sel_minute", "MM");

            httpClient.GetStringPost(this.PrimaryHost + "SendSMSDec19", data);

            if (httpClient.IsRedirect)
            {
                string RedirectUrl = httpClient.RedirectLocation;
            }
        }

        public void LogOut()
        {
            if (this.authenticationState != AuthenticationState.Authenticated)
                throw new Exception("Not logged in");

            httpClient.GetString(this.PrimaryHost + "Logout");

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
