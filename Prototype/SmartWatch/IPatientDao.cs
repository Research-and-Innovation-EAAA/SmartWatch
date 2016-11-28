using System.Collections.Generic;

namespace IoTDataReceiver
{
    interface IPatientDao
    {
        List<string> GetPatients();

        string GetPassword(string username);
    }
}
