using System;
using System.Collections.Generic;
using System.IO;

namespace IoTDataReceiver
{
    class PatientFileDao : IPatientDao
    {

        const string PATH = @"C:\SmartWatch\patients.csv";

        private Dictionary<string, string> patients = null;

        #region Singleton
        private static PatientFileDao instance = null;
        public static PatientFileDao Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PatientFileDao();
                }
                return instance;
            }
        }
        #endregion

        private PatientFileDao()
        {
            LoadPatients();
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

        private void LoadPatients()
        {
            var reader = new StreamReader(File.OpenRead(PATH));
            this.patients = new Dictionary<string, string>();
            int lineNr = 1;

            while (!reader.EndOfStream)
            {
                string line = "";
                try
                {
                    line = reader.ReadLine();

                    var parts = line.Split(',');

                    string username = parts[0].Trim();
                    string password = parts[1].Trim();
                    this.patients.Add(username, password);
                }
                catch (Exception e)
                {
                    var text = "";
                    if (e is ArgumentException)
                    {
                        text = "Patient " + line + " is twice in the file. Please ensure that a username is there only once.";
                    }
                    else
                    {
                        text = "Check line " + lineNr + " in the file patients.csv: " + line;
                    }
                    throw new MyExceptions.InputException(text + "\n" + e.Message, e);
                }
                lineNr++;
            }

            reader.Close();
        }
    }
}
