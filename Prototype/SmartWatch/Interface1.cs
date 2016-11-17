using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDataReceiver
{
    interface IProcessAlgorithm
    {
        string ProcessDataFromFile(string path);
    }

    class DummyAlgorithm : IProcessAlgorithm
    {
        public string ProcessDataFromFile(string path)
        {
            return "[]"; 
        }
    }
}
