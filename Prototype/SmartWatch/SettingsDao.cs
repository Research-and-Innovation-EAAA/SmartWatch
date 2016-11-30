using System.Collections.Generic;
using System.Configuration;

namespace IoTDataReceiver
{
    class SettingsDao : ISettingsDao
    {
        private static SettingsDao instance = null;

        public static SettingsDao Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SettingsDao();
                }
                return instance;
            }
        }

        private SettingsDao()
        {
            string frequency, period, studyCenter, studyCode;
            try
            {
                frequency = (string)Properties.Settings.Default["frequency"];
                period = (string)Properties.Settings.Default["period"];
                studyCenter = (string)Properties.Settings.Default["studyCenter"];
                studyCode = (string)Properties.Settings.Default["studyCode"];
            }
            catch (SettingsPropertyNotFoundException ex)
            {
                frequency = "75"; // Hz
                period = "240"; //24 hours * 10 days = 240
                studyCenter = "AUH-EAAA"; // TODO change to AUH
                studyCode = "TEST"; // TODO change to GIGT
            }
            this.settings = new Dictionary<string, string>();
            this.settings.Add("frequency", frequency);
            this.settings.Add("period", period);
            this.settings.Add("studyCenter", studyCenter);
            this.settings.Add("studyCode", studyCode);
        }

        private Dictionary<string, string> settings;

        public Dictionary<string, string> Settings
        {
            get
            {
                return new Dictionary<string, string>(this.settings);
            }

            set
            {
                this.settings = new Dictionary<string, string>(settings);
                Properties.Settings.Default.frequency = this.settings["frequency"];
                Properties.Settings.Default.period = this.settings["period"];
                Properties.Settings.Default.studyCenter = this.settings["studyCenter"];
                Properties.Settings.Default.studyCode = this.settings["studyCode"];
                Properties.Settings.Default.Save();
            }
        }
    }
}
