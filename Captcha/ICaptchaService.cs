using System.Threading.Tasks;

namespace Entropy.Captcha
{
    public interface ICaptchaService
    {
        Task<string> Solve(string sitekey = "6LdpuiYTAAAAAL6y9JNUZzJ7cF3F8MQGGKko1bCy", string url = "https://club.pokemon.com/us/pokemon-trainer-club/parents/sign-up");
    }
}
