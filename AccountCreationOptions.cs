using System.Net;
using Entropy.Captcha;
using RestSharp;

namespace Entropy
{
    public class AccountCreationOptions
    {
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public string Email { get; set; }
        
        public string Dob { get; set; }

        public IWebProxy Proxy { get; set; } = null;
        
        public ICaptchaService CaptchaService { get; set; }
    }
}