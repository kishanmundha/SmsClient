using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmsClient
{
    /// <summary>
    /// SmsClient interface
    /// </summary>
    public interface ISmsClient : IDisposable
    {
        /// <summary>
        /// Authentication State
        /// </summary>
        AuthenticationState authenticationState { get; }

        /// <summary>
        /// Set Authentication to use Service
        /// </summary>
        /// <param name="UserId">User Id (Email, UserId, MobileNumber)</param>
        /// <param name="Password">Password</param>
        void SetAuthentication(string UserId, string Password);

        /// <summary>
        /// Send Text Message
        /// </summary>
        /// <param name="Number">Mobile Number</param>
        /// <param name="Message">Message Content</param>
        void SendMessage(string Number, string Message);

        /// <summary>
        /// Log Out
        /// </summary>
        void LogOut();
    }

    public enum AuthenticationState
    {
        /// <summary>
        /// Not checked yet
        /// </summary>
        NotChecked,
        
        /// <summary>
        /// User authenticated by sms service provider
        /// </summary>
        Authenticated,
        
        /// <summary>
        /// User not authenticated by sms service provider
        /// </summary>
        NotAuthenticated,

        /// <summary>
        /// Logged out service
        /// </summary>
        LogOut
    }
}
