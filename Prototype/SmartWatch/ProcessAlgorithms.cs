using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    public interface IProcessAlgorithm
    {
        /// <summary>
        /// Method for processing large amount of data and returning smaller subset in JSON format.
        /// </summary>
        /// <param name="path">Path to a CSV file to process</param>
        /// <returns>JSON string with a timestamp and computed data values</returns>
        string ProcessDataFromFile(string path, Guid deviceId);

        event DeviceProgressUpdateHandler ProgressUpdate;
    }


    /// <summary>
    /// Algorithm that calculates average every 1000th record
    /// </summary>
    class SimpleAlgorithm : IProcessAlgorithm
    {
        public event DeviceProgressUpdateHandler ProgressUpdate;

        protected virtual void OnProgressUpdate(int progress, Guid deviceId)
        {
            DeviceProgressUpdateHandler handler = ProgressUpdate;
            if (handler != null) handler(progress, deviceId);
        }

        public string ProcessDataFromFile(string path, Guid deviceId)
        {
            int totalLines = this.CountLines(path);

            var reader = new StreamReader(File.OpenRead(path));
            List<DataClass> listData = new List<DataClass>();

            int lineNumber = 0;
            float avgActivity = 0;
            float avgTemperature = 0;
            float avgLight = 0;
            bool maxButton = false;
            DateTime? firstRecordTime = null;
            int number = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                lineNumber++;
                if (lineNumber < 101)
                    continue;
                if (lineNumber % 1000 != 1)
                {
                    try
                    {
                        number++;

                        var values = line.Split(',');

                        int year = Convert.ToInt32(values[0].Substring(0, 4));
                        int month = Convert.ToInt32(values[0].Substring(5, 2));
                        int day = Convert.ToInt32(values[0].Substring(8, 2));
                        int hour = Convert.ToInt32(values[0].Substring(11, 2));
                        int minute = Convert.ToInt32(values[0].Substring(14, 2));
                        int seconds = Convert.ToInt32(values[0].Substring(17, 2));
                        int millis = Convert.ToInt32(values[0].Substring(20, 3));

                        var row = new DataClass();
                        row.timestamp = new DateTime(year, month, day, hour, minute, seconds, millis);
                        var X = float.Parse(values[1], CultureInfo.InvariantCulture);
                        var Y = float.Parse(values[2], CultureInfo.InvariantCulture);
                        var Z = float.Parse(values[3], CultureInfo.InvariantCulture);
                        row.activity = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
                        row.lightIntensity = float.Parse(values[4], CultureInfo.InvariantCulture);
                        row.button = values[5] == "1";
                        row.temperature = float.Parse(values[6], CultureInfo.InvariantCulture);

                        if (firstRecordTime == null)
                            firstRecordTime = row.timestamp;
                        avgActivity += row.activity;
                        avgTemperature += row.temperature;
                        avgLight += row.lightIntensity;
                        maxButton = maxButton || row.button;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    continue;
                }

                var row2 = new DataClass();
                row2.timestamp = (DateTime)firstRecordTime;
                row2.activity = avgActivity / number;
                row2.lightIntensity = avgLight / number;
                row2.button = maxButton;
                row2.temperature = avgTemperature / number;

                listData.Add(row2);

                OnProgressUpdate((int)(((double)lineNumber) / totalLines * 100f), deviceId);

                avgActivity = 0;
                avgTemperature = 0;
                avgLight = 0;
                maxButton = false;
                firstRecordTime = null;
                number = 0;
            }

            /* foreach (DataClass d in listData)
            {
                Debug.WriteLine(d.ToString());
            } */

            string json = JsonConvert.SerializeObject(listData);
            Debug.WriteLine(json);

            Debug.WriteLine("Processed rows in total: " + listData.Count);

            return json;
        }

        private int CountLines(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                int i = 0;
                while (r.ReadLine() != null)
                    i++;
                return i;
            }
        }

        private class DataClass
        {
            public DateTime timestamp { get; set; }
            /*   public float X { get; set; }
               public float Y { get; set; }
               public float Z { get; set; }*/
            public float activity { get; set; }
            public float temperature { get; set; }
            public float lightIntensity { get; set; }
            public bool button { get; set; }

            public override string ToString()
            {
                return timestamp.ToString() + " " + activity + ", " + lightIntensity + ", " + button + ", " + temperature + " °C";
            }
        }
    }


    /// <summary>
    /// Algorithm whose output is always 100 points or less
    /// </summary>
    class HundredAlgorithm : IProcessAlgorithm
    {
        public event DeviceProgressUpdateHandler ProgressUpdate;

        protected virtual void OnProgressUpdate(int progress, Guid deviceId)
        {
            DeviceProgressUpdateHandler handler = ProgressUpdate;
            if (handler != null) handler(progress, deviceId);
        }

        public string ProcessDataFromFile(string path, Guid deviceId)
        {
            int totalLines = this.CountLines(path);

            int sectionLength = totalLines / 100;

            var reader = new StreamReader(File.OpenRead(path));
            List<DataClass> listData = new List<DataClass>();

            int lineNumber = 0;
            float avgActivity = 0;
            float avgTemperature = 0;
            float avgLight = 0;
            bool maxButton = false;
            DateTime? firstRecordTime = null;
            int number = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                lineNumber++;
                if (lineNumber < 101)
                    continue;
                if (lineNumber % sectionLength != 1)
                {
                    try
                    {
                        number++;

                        var values = line.Split(',');

                        int year = Convert.ToInt32(values[0].Substring(0, 4));
                        int month = Convert.ToInt32(values[0].Substring(5, 2));
                        int day = Convert.ToInt32(values[0].Substring(8, 2));
                        int hour = Convert.ToInt32(values[0].Substring(11, 2));
                        int minute = Convert.ToInt32(values[0].Substring(14, 2));
                        int seconds = Convert.ToInt32(values[0].Substring(17, 2));
                        int millis = Convert.ToInt32(values[0].Substring(20, 3));

                        var row = new DataClass();
                        row.timestamp = new DateTime(year, month, day, hour, minute, seconds, millis);
                        var X = float.Parse(values[1], CultureInfo.InvariantCulture);
                        var Y = float.Parse(values[2], CultureInfo.InvariantCulture);
                        var Z = float.Parse(values[3], CultureInfo.InvariantCulture);
                        row.activity = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
                        row.lightIntensity = float.Parse(values[4], CultureInfo.InvariantCulture);
                        row.button = values[5] == "1";
                        row.temperature = float.Parse(values[6], CultureInfo.InvariantCulture);

                        if (firstRecordTime == null)
                            firstRecordTime = row.timestamp;
                        avgActivity += row.activity;
                        avgTemperature += row.temperature;
                        avgLight += row.lightIntensity;
                        maxButton = maxButton || row.button;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    continue;
                }

                var row2 = new DataClass();
                row2.timestamp = (DateTime)firstRecordTime;
                row2.activity = avgActivity / number;
                row2.lightIntensity = avgLight / number;
                row2.button = maxButton;
                row2.temperature = avgTemperature / number;

                listData.Add(row2);

                OnProgressUpdate((int)(((double)lineNumber) / totalLines * 100f), deviceId);

                avgActivity = 0;
                avgTemperature = 0;
                avgLight = 0;
                maxButton = false;
                firstRecordTime = null;
                number = 0;
            }

            /* foreach (DataClass d in listData)
            {
                Debug.WriteLine(d.ToString());
            } */

            string json = JsonConvert.SerializeObject(listData);
            Debug.WriteLine(json);

            Debug.WriteLine("Processed rows in total: " + listData.Count);

            return json;
        }

        private int CountLines(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                int i = 0;
                while (r.ReadLine() != null)
                    i++;
                return i;
            }
        }

        private class DataClass
        {
            public DateTime timestamp { get; set; }
            /*   public float X { get; set; }
               public float Y { get; set; }
               public float Z { get; set; }*/
            public float activity { get; set; }
            public float temperature { get; set; }
            public float lightIntensity { get; set; }
            public bool button { get; set; }

            public override string ToString()
            {
                return timestamp.ToString() + " " + activity + ", " + lightIntensity + ", " + button + ", " + temperature + " °C";
            }
        }
    }

    /// <summary>
    /// Dummy algorithm that simulates calculations
    /// </summary>
    class DummyAlgorithm : IProcessAlgorithm
    {
        public event DeviceProgressUpdateHandler ProgressUpdate;

        protected virtual void OnProgressUpdate(int progress, Guid deviceId)
        {
            DeviceProgressUpdateHandler handler = ProgressUpdate;
            if (handler != null) handler(progress, deviceId);
        }

        public string ProcessDataFromFile(string path, Guid deviceId)
        {

            for (int i = 0; i <= 30; i++) // wait loop simulating reading data
            {
                System.Threading.Thread.Sleep(500);
                OnProgressUpdate((int)(i / 30f * 100), deviceId);
            }


            return "";
        }
    }
}
