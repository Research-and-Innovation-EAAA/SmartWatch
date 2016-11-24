using Newtonsoft.Json.Linq;
using RestSharp;
using System.Diagnostics;

namespace IoTDataReceiver
{
    class HowRYouConnector
    {
        private static HowRYouConnector instance;

        public static HowRYouConnector Instance
        {
            get
            {
                if (instance == null)
                    instance = new HowRYouConnector();
                return instance;
            }
        }
        private HowRYouConnector() { }


        private RestClient client = new RestClient("http://192.168.56.101:3000/api/v1");
        public HowRYouLoginToken Login(string username, string password)
        {
            // prepare the request
            var request = new RestRequest("login", Method.POST);
            request.AddParameter("username", username);
            request.AddParameter("password", password);

            // execute the request
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new MyExceptions.UnauthorizedException();
            }
            var content = response.Content; // raw content as a string
            Debug.WriteLine("API response: " + content);

            // parse the response
            JObject joResponse = JObject.Parse(content.ToString());
            JObject ojObject = (JObject)joResponse["data"];
            string authToken = ojObject["authToken"].ToString();
            string userId = ojObject["userId"].ToString();

            return new HowRYouLoginToken { authToken = authToken, userId = userId };
        }

        public void Logout(HowRYouLoginToken token)
        {
            // prepare the request
            var request = new RestRequest("logout", Method.POST);
            request.AddHeader("X-User-id", token.userId);
            request.AddHeader("X-Auth-Token", token.authToken);

            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as a string
            Debug.WriteLine("API response: " + content);
        }

        public void UploadFile(string filePath, HowRYouLoginToken token)
        {
            // prepare the request
            var request = new RestRequest("smartwatch", Method.POST);
            request.AddHeader("X-User-id", token.userId);
            request.AddHeader("X-Auth-Token", token.authToken);
            request.AddFile("payload", filePath);

            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string
            Debug.WriteLine(content);
        }

        public void UploadViewData(string jsonData, string date, HowRYouLoginToken token)
        {
            // prepare the request
            var request = new RestRequest("smartwatchview", Method.POST);
            request.AddHeader("X-User-id", token.userId);
            request.AddHeader("X-Auth-Token", token.authToken);
            request.AddParameter("device", "smartwatch");
            request.AddParameter("date", date);
            request.AddParameter("data", jsonData);

            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string
            Debug.WriteLine(content);
        }

        public class HowRYouLoginToken
        {
            public string authToken { get; set; }
            public string userId { get; set; }
        }
    }
}
