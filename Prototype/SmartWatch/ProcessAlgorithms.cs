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
        string ProcessDataFromFile(string path);

        event ProgressUpdateHandler ProgressUpdate;
    }


    /// <summary>
    /// Dummy algorithm that calculates average every 1000th record
    /// </summary>
    class DummyAlgorithm : IProcessAlgorithm
    {
        public event ProgressUpdateHandler ProgressUpdate;

        protected virtual void OnProgressUpdate(int progress)
        {
            ProgressUpdateHandler handler = ProgressUpdate;
            if (handler != null) handler(progress);
        }

        public string ProcessDataFromFile(string path)
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
                        row.G = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
                        row.L = float.Parse(values[4], CultureInfo.InvariantCulture);
                        row.B = values[5] == "1";
                        row.T = float.Parse(values[6], CultureInfo.InvariantCulture);

                        if (firstRecordTime == null)
                            firstRecordTime = row.timestamp;
                        avgActivity += row.G;
                        avgTemperature += row.T;
                        avgLight += row.L;
                        maxButton = maxButton || row.B;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    continue;
                }

                var row2 = new DataClass();
                row2.timestamp = (DateTime)firstRecordTime;
                row2.G = avgActivity / number;
                row2.L = avgLight / number;
                row2.B = maxButton;
                row2.T = avgTemperature / number;

                listData.Add(row2);

                OnProgressUpdate((int)(((double)lineNumber) / totalLines * 100f));

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
            public float G { get; set; }
            public float T { get; set; }
            public float L { get; set; }
            public bool B { get; set; }

            public override string ToString()
            {
                return timestamp.ToString() + " " + G + ", " + L + ", " + B + ", " + T + " °C";
            }
        }
    }
}
