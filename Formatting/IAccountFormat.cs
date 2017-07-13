namespace Entropy.Formatting
{
    public interface IAccountFormat
    {
        string Format(AccountCreationResult account);

        string GetHeader();

        string GetFooter();
    }
}