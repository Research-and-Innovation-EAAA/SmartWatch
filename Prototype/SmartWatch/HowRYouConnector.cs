﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDataReceiver
{
    class HowRYouConnector
    {
        public HowRYouLoginToken Login(string username, string password) {
            return new HowRYouLoginToken();
        }

        public void Logout(string username, HowRYouLoginToken token)
        {

        }

        public void UploadFile(string filePath, string username, HowRYouLoginToken token)
        {

        }

        public void UploadViewData(string jsonData, string date, string username, HowRYouLoginToken token)
        {

        }

        public class HowRYouLoginToken
        {
            public string authToken { get; set; }
            public string userId { get; set; }
        }
    }
}
