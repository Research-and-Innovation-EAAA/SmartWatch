namespace IoTDataReceiver
{
    public enum DataProcessStep
    {
        Processing = -1, // working with the watch
        DeviceInserted = 0,
        DataDownloaded = 1,
        DataProcessed = 2,
        DataUploaded = 3,
        DeviceCleared = 4
    }
}
