namespace Entropy.Formatting
{
    public class RocketMapFormat : IAccountFormat
    {
        public string Format(AccountCreationResult account)
        {
            return $"ptc,{account.Username},{account.Password}";
        }

        public string GetHeader()
        {
            return string.Empty;
        }

        public string GetFooter()
        {
            return string.Empty;
        }
    }
}