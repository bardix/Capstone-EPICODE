namespace Capstone_EPICODE.Services.Password
{
    public interface IPasswordEnconder
    {
        string Encode(string password);
        bool IsSame(string plainText, string codedText);
    }
}
