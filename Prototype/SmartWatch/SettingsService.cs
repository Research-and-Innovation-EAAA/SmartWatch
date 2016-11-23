using System.Collections.Generic;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    class SettingsService
    {
        public Dictionary<string, string> Settings { get; set; }

        private static SettingsService instance = null;

        private SettingsService()
        {
            string frequency = (string)Properties.Settings.Default["frequency"] ?? "75";
            string period = (string)Properties.Settings.Default["period"] ?? "240"; //24 hours * 10 days = 240
            string studyCenter = (string)Properties.Settings.Default["studyCenter"] ?? "AUH-EAAA"; // TODO change to AUH, GIGT
            string studyCode = (string)Properties.Settings.Default["studyCode"] ?? "TEST";
            Settings = new Dictionary<string, string>();
            Settings.Add("frequency", frequency);
            Settings.Add("period", period);
            Settings.Add("studyCenter", studyCenter);
            Settings.Add("studyCode", studyCode); 
            //testctest
        }

        public static SettingsService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SettingsService();
                }
                return instance;
            }
        }
    }
}
