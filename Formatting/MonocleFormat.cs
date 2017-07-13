using Entropy;

namespace Entropy.Formatting
{
    public class MonocleFormat : IAccountFormat
    {
        public string Format(AccountCreationResult account)
        {
            return $"{account.Username},{account.Password},ptc,,,";
        }

        public string GetHeader()
        {
            return "username,password,provider,model,iOS,id";
        }

        public string GetFooter()
        {
            return string.Empty;
        }
    }
}