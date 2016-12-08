namespace IoTDataReceiver
{
    public enum DataProcessStep
    {
        DeviceInserted = 0,
        DataDownloaded = 1,
        DataProcessed = 2,
        DataUploaded = 3,
        DeviceCleared = 4,
        Processing = -1     // = working 
    }
}
