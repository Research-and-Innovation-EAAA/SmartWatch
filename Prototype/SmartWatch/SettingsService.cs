using System.Collections.Generic;
using System.Configuration;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    class SettingsService
    {
        private static SettingsService instance = null;

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

        private ISettingsDao settingsDao = null;

        private SettingsService()
        {
            this.settingsDao = SettingsDao.Instance;
        }

        public Dictionary<string, string> Settings
        {
            get { return this.settingsDao.Settings; }

            set { this.settingsDao.Settings = value; }
        }
    }
}
