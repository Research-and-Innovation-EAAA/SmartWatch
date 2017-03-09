using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calibration
{
    class Program
    {
        static void Main(string[] args)
        {
            const string targetDirectory = @"C:\Users\jakub\Downloads\_BIRGITTE";

            Console.WriteLine(targetDirectory);

            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                if (Path.GetExtension(fileName).Equals(".csv") && !fileName.Contains("_CALIB"))
                    ProcessFile(fileName);
            }


            Console.WriteLine("End of program------------");
            // Suspend the screen. 
            Console.ReadLine();

        }

        static void ProcessFile(string file)
        {
            Console.WriteLine("Processing " + file);

            int counter = 1;
            string line;

            string outputFile = Path.GetDirectoryName(file) + @"\" + Path.GetFileNameWithoutExtension(file) + "_CALIB" + Path.GetExtension(file);

            // Read the file and display it line by line. 
            StreamReader fileReader = new StreamReader(file);
            using (StreamWriter output = new StreamWriter(outputFile))
            {
                DateTime startRecording;
                DateTime configTime = DateTime.MinValue;
                DateTime extractTime;

                while ((line = fileReader.ReadLine()) != null)
                {
                    if (counter % 10000 == 0) Console.Write(".");
                    if (counter == 12)
                    {
                        startRecording = DateTime.ParseExact(line.Substring(line.LastIndexOf(',') + 1), "yyyy-MM-dd HH:mm:ss:fff", CultureInfo.InvariantCulture);
                        Console.WriteLine("Start recording: " + startRecording.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                        if (startRecording.Year == 2017)
                        {
                            Console.WriteLine("[ERR] Year is 2017! skipping...");
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

                        bool estimate = (extractTime - configTime).TotalDays < 10f;
                        Console.WriteLine((extractTime - configTime).TotalDays);
                        return;
                    }

                    if (counter > 100)
                    {
                        //TODO calculate time and rewrite the line variable 

                    }

                    output.WriteLine(line);
                    counter++;
                }
            }

            fileReader.Close();
        }
    }
}