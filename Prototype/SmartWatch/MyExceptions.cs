using System;

namespace IoTDataReceiver
{
    class MyExceptions
    {
        [Serializable]
        public class NoDataException : Exception
        {
            public NoDataException()
            { }

            public NoDataException(string message)
                : base(message)
            { }

            public NoDataException(string message, Exception innerException)
                : base(message, innerException)
            { }
        }

        [Serializable]
        public class CommunicationException : Exception
        {
            public CommunicationException()
            { }

            public CommunicationException(string message)
                : base(message)
            { }

            public CommunicationException(string message, Exception innerException)
                : base(message, innerException)
            { }
        }

        [Serializable]
        public class DeviceException : Exception
        {
            public DeviceException()
            { }

            public DeviceException(string message)
                : base(message)
            { }

            public DeviceException(string message, Exception innerException)
                : base(message, innerException)
            { }
        }
    }
}
