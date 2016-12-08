using System.Collections.Generic;

namespace IoTDataReceiver
{
    class SettingsService : ISettingsService
    {
        private static SettingsService instance = null;

        #region Singleton
        public static SettingsService Instance
        {
            get
            {
                if (instance == null) { instance = new SettingsService(); }
                return instance;
            }
        }
        #endregion

        private SettingsService() { }

        public Dictionary<string, string> Settings
        {
            get { return SettingsDao.Instance.Settings; }

            set { SettingsDao.Instance.Settings = value; }
        }
    }
}
