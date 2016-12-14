using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDataReceiver
{
    class HowRYouBirgitteConnector : HowRYouConnector
    {
        #region Singleton
        private static HowRYouBirgitteConnector instance;
        public static new HowRYouBirgitteConnector Instance
        {
            get
            {
                if (instance == null)
                    instance = new HowRYouBirgitteConnector();
                return instance;
            }
        }
        private HowRYouBirgitteConnector() { }
        #endregion

        const string PATH = @"U:\DATA\SmartWatch\";

        public override void UploadFile(string filePath, LoginToken token)
        {
            string targetPath = PATH + Path.GetFileName(filePath);

            // Ensure that the target does not exist
            if (File.Exists(targetPath))
                throw new Exception("The file already exists in the network drive! No data is being removed or overwritten.");

            try
            {
                File.Move(filePath, targetPath);
                Console.WriteLine("{0} was moved to {1}.", filePath, targetPath);
            }
            catch (Exception e)
            {
                throw new MyExceptions.CommunicationException("Error copying to network drive, please check network connection.\n" + e.Message, e);
            }
        }
    }
}
