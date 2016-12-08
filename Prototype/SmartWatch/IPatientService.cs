using System.Collections.Generic;

namespace IoTDataReceiver
{
    interface IPatientService
    {

        List<string> GetPatients();
        string GetPassword(string username);

    }
}
