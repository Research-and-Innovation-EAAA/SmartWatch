using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllRegistrations
{
    class Program
    {
        private static RestClient client = new RestClient("https://www.leukemia.sundata.dk/api/v1");

        static void Main(string[] args)
        {
            const string PATIENTS_FILE = @"C:\watch\patients.csv";
            const string OUTPUT_FILE = @"C:\watch\data.json";



            string[][] patients = GetPatientCredentials(PATIENTS_FILE);
            JArray data = new JArray();

            int count = 0, countErr = 0;

            foreach (string[] p in patients)
            {
                //          Console.ReadLine();
                try
                {
                    count++;
                    data.Merge(ReadData(p[0], p[1]));

                }
                catch
                {
                    countErr++;
                    continue;
                }

            }

            Console.WriteLine("------DONE [" + countErr + "err out of " + count + "] ------");
            Console.ReadLine();

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            File.WriteAllText(OUTPUT_FILE, json);

            Console.WriteLine("------DONE------");
            Console.ReadLine();
            Console.ReadLine();
        }

        public static JArray ReadData(string username, string password)
        {
            String[] token = null;
            try
            {
                token = Login(username, password);
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERR] Cannot login " + username + " " + password);
                throw e;
            }

            try
            {
                JArray data = ReadRegistrations(token[0], token[1], username);
                //        Console.WriteLine("[OK] Data read " + username);
                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERR] " + e);
                throw e;
            }
            finally
            {
                Logout(token[0], token[1]);
            }

        }

        public static string[][] GetPatientCredentials(string path)
        {
            List<string[]> patients = new List<string[]>();

            using (StreamReader reader = File.OpenText(path))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.Delimiter = ";";
                while (csv.Read())
                {
                    string username = csv.GetField<string>(0);
                    string password = csv.GetField<string>(1);

                    patients.Add(new string[] { username, password });
                }

            }

            return patients.ToArray();
        }

        public static String[] Login(string username, string password)
        {
            // prepare the request
            var request = new RestRequest("login", Method.POST);
            request.AddParameter("username", username);
            request.AddParameter("password", password);

            // execute the request
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == 0)
            {
                throw new Exception();
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception();
            }
            var content = response.Content; // raw content as a string
            //Debug.WriteLine("API response: " + content);

            // parse the response
            JObject joResponse = JObject.Parse(content.ToString());
            JObject ojObject = (JObject)joResponse["data"];
            string authToken = ojObject["authToken"].ToString();
            string userId = ojObject["userId"].ToString();

            return new String[] { authToken, userId };
        }

        public static JArray ReadRegistrations(string authToken, string userId, string username)
        {
            // prepare the request
            var request = new RestRequest("registrations", Method.GET);
            request.AddHeader("X-User-id", userId);
            request.AddHeader("X-Auth-Token", authToken);

            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as a string
                                            //   Console.WriteLine("API response: " + content);

            JArray a = JArray.Parse(content);
            foreach (JObject item in a)
            {
                item["patient"] = username;
            }
            return a;
        }

        public static void Logout(String authToken, String userId)
        {
            // prepare the request
            var request = new RestRequest("logout", Method.POST);
            request.AddHeader("X-User-id", userId);
            request.AddHeader("X-Auth-Token", authToken);

            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as a string
                                            //  Console.WriteLine("API response: " + content);
        }


    }
}
