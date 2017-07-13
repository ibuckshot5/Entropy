using System;
using System.Net;
using System.Reflection.Emit;
using System.Security;
using System.Threading.Tasks;
using RestSharp;

namespace Entropy.Captcha
{
    public class TwoCaptchaService : ICaptchaService
    {
        public string ApiKey;

        private readonly RestClient Client = new RestClient();
        
        public TwoCaptchaService(string apiKey)
        {
            ApiKey = apiKey;
        }

        public async Task<string> Solve(string sitekey = "6LdpuiYTAAAAAL6y9JNUZzJ7cF3F8MQGGKko1bCy", 
            string url = "https://club.pokemon.com/us/pokemon-trainer-club/parents/sign-up")
        {
            var inRes = Client.Execute(new RestRequest("http://2captcha.com/in.php", Method.GET)
                .AddQueryParameter("key", ApiKey)
                .AddQueryParameter("method", "userrecaptcha")
                .AddParameter("googlekey", sitekey)
                .AddQueryParameter("pageurl", url));
        
            var captchaId = string.Empty;
            if (inRes.Content.Contains("OK|"))
                captchaId = inRes.Content.Split('|')[1];
            else
                return inRes.Content;

            var token = string.Empty;
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                var capRequest = new RestRequest("http://2captcha.com/res.php", Method.GET)
                    .AddQueryParameter("key", ApiKey)
                    .AddQueryParameter("action", "get")
                    .AddQueryParameter("id", captchaId);
                var capRes = Client.Execute(capRequest);
                if (capRes.Content != "CAPTCHA_NOT_READY")
                {
                    // Yay, the captcha's been solved! Probably by some person in a third
                    // world country working for minimum wage but WHO FUCKING CARES!
                    token = capRes.Content;
                    break;
                }
            }

            return await Task.FromResult(token);
        }
    }
}