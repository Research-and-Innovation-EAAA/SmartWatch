using System.Collections.Generic;

namespace IoTDataReceiver
{
    class PatientService : IPatientService
    {
        private IPatientDao patientDao = null;

        #region Singleton
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
        #endregion

        private PatientService() {
            this.patientDao = PatientFileDao.Instance;
        }

        public List<string> GetPatients()
        {
            return new List<string>(patientDao.GetPatients());
        }

        public string GetPassword(string username)
        {
            return patientDao.GetPassword(username);
        }

    }
}
