using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Calibration
{
    class Program
    {
        static Dictionary<string, DateTime> patients;

        static void Main(string[] args)
        {
            const string targetDirectory = @"C:\Users\jkb\Downloads\_BIRGITTE";
            Console.WriteLine(targetDirectory);

            StreamReader fileReader = new StreamReader(targetDirectory + @"\files.csv");
            patients = new Dictionary<string, DateTime>();

            while (!fileReader.EndOfStream)
            {
                var line = fileReader.ReadLine();
                var parts = line.Split(',');

                string username = parts[0];
                DateTime date = DateTime.ParseExact(parts[1], "yyyy-MM-dd HH:mm:ss:fff", CultureInfo.InvariantCulture);
                patients.Add(username, date);
            }

            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                if (Path.GetExtension(fileName).Equals(".csv") && !fileName.Contains("_CALIB") && !fileName.Contains("files"))
                {
                    ProcessFile(fileName);
                }

            }


            Console.WriteLine("End of program------------");
            // Suspend the screen. 
            Console.ReadLine();

        }

        static DateTime FindFirstRegistration(string file)
        {
            file = Path.GetFileNameWithoutExtension(file);
            return DateTime.ParseExact(file.Substring(file.LastIndexOf('_') + 1), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }

        static DateTime FindLastRegistration(string file)
        {
            DateTime i = DateTime.MinValue;
            StreamReader fileReader = new StreamReader(file);
            string last = File.ReadLines(file).Last().Split(',')[0];

            return DateTime.ParseExact(last, "yyyy-MM-dd HH:mm:ss:fff", CultureInfo.InvariantCulture);
        }


        static void ProcessFile(string file)
        {
            Console.WriteLine("Processing " + file);

            int counter = 1;
            string line;
            TimeSpan span = new TimeSpan(TimeSpan.TicksPerSecond / 75L); // 75 Hz

            string outputFile = Path.GetDirectoryName(file) + @"\" + Path.GetFileNameWithoutExtension(file) + "_CALIB" + Path.GetExtension(file);

            // Read the file and display it line by line. 
            StreamReader fileReader = new StreamReader(file);
            using (StreamWriter output = new StreamWriter(outputFile))
            {
                string patient = Regex.Match(Path.GetFileNameWithoutExtension(file), @"^[a-zA-Z]*").Value;
                DateTime startRecording;
                DateTime configTime = DateTime.MinValue;
                DateTime extractTime;
                DateTime i = DateTime.MinValue;

                while ((line = fileReader.ReadLine()) != null)
                {
                    if (counter % 100000 == 0) Console.Write(".");
                    if (counter == 12)
                    {
                        startRecording = DateTime.ParseExact(line.Substring(line.LastIndexOf(',') + 1), "yyyy-MM-dd HH:mm:ss:fff", CultureInfo.InvariantCulture);
                        Console.WriteLine("Start recording: " + startRecording.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                        if (startRecording.Year != 2010)
                        {
                            Console.WriteLine("[ERR] Year is not 2010! skipping...");
                            return;
                        }
                    }
                    if (counter == 36)
                    {
                        configTime = DateTime.ParseExact(line.Substring(line.LastIndexOf(',') + 1), "yyyy-MM-dd HH:mm:ss:fff", CultureInfo.InvariantCulture);
                        Console.WriteLine("Config time: " + configTime.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                    }
                    if (counter == 39)
                    {
                        extractTime = DateTime.ParseExact(line.Substring(line.LastIndexOf(',') + 1), "yyyy-MM-dd HH:mm:ss:fff", CultureInfo.InvariantCulture);
                        Console.WriteLine("Extract time: " + extractTime.ToString("yyyy-MM-dd HH:mm:ss:fff"));

                        bool estimate = (extractTime - configTime).TotalDays > 10f;
                        if (estimate)
                        {
                            Console.WriteLine("[WAR] Estimate start timestamp necessary.");
                            try
                            {
                                i = patients[patient];
                                Console.WriteLine(i);
                            }
                            catch
                            {
                                Console.WriteLine("[ERR] Estimate start timestamp not found! skipping...");
                                return;
                            }

                        }
                        else
                        {
                            DateTime firstReg = FindFirstRegistration(file);
                            DateTime lastReg = FindLastRegistration(file);
                            var diff = lastReg - firstReg;

                            Console.WriteLine("First registration in file: " + firstReg.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                            Console.WriteLine("Last registration in file : " + lastReg.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                            Console.WriteLine("Difference                : " + diff);
                            Console.WriteLine();

                            i = extractTime - diff;
                            i = i.AddMilliseconds(-i.Millisecond);
                            Console.WriteLine("Extract timestamp         : " + extractTime.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                            Console.WriteLine("New start timestamp       : " + i.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                        }
                        Console.WriteLine();

                    }
                    if (counter == 40)
                    {
                        line += " + CALIBRATED " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                    }

                    if (counter > 100)
                    {
                        //Console.Write(i.ToString("yyyy-MM-dd HH:mm:ss:fff")[22]);

                        i += span; // add 1/75 second

                        line = i.ToString("yyyy-MM-dd HH:mm:ss:fff") + line.Substring(23);
                        //Console.WriteLine(line);

                        /* if (counter > 10000)
                             return;
                             */
                    }

                    output.WriteLine(line);
                    counter++;
                }
            }

            fileReader.Close();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}