using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace IoTDataReceiver
{
    class PatientService
    {
        private static PatientService instance = null;
        public static PatientService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PatientService();
                }
                return instance;
            }
        }

        private PatientService() { }

        const string PATH = @"C:\SmartWatch\patients.csv";

        private Dictionary<string, string> patients = null;

        private void LoadPatients()
        {
            var reader = new StreamReader(File.OpenRead(PATH));
            this.patients = new Dictionary<string, string>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                // Match match = Regex.Match(line, @"^[^,]*");
                var parts = line.Split(',');

                string username = parts[0];
                string password = parts[1];
                this.patients.Add(username, password);
            }
        }

        public List<string> GetPatients()
        {
            if (this.patients == null)
                LoadPatients();
            return new List<string>(this.patients.Keys);
        }

        public string GetPassword(string username)
        {
            if (this.patients == null)
                LoadPatients();
            return this.patients[username];
        }

    }
}
