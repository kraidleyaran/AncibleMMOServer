namespace AncibleCoreServer.Services.ClientManager
{
    public class AuthenticationSession
    {
        public byte[] PublicKey;
        public byte[] AuthKey;
        public bool Authenticated { get; private set; }

        public void Reset()
        {
            PublicKey = null;
            AuthKey = null;
            Authenticated = false;
        }

        public void SetAuthenticationState(bool authenticated)
        {
            Authenticated = authenticated;
        }
    }
}