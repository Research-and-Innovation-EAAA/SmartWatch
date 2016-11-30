namespace IoTDataReceiver
{
    public interface IDatabaseConnector
    {

        LoginToken Login(string username, string password);

        void Logout(LoginToken token);

        void UploadFile(string filePath, LoginToken token);

        void UploadViewData(string jsonData, string date, LoginToken token);

    }

    public class LoginToken
    {
        public string authToken { get; set; }
        public string userId { get; set; }
    }
}
