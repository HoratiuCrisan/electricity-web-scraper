using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using dotenv.net;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace Electricity_Web_Scraper {
    public class Parser(string filename)
    {
        private readonly string _filename = filename;

        public string ReadPdfFile() {
            // create a new pdf reader based on the filename
            using PdfReader pdfReader = new(_filename);

            string messageToSend = String.Empty;

            // create a new pdf document with our pdf reader
            using (PdfDocument pdfDocument = new(pdfReader)){

                // create a new string builder to store the possible text from the pdf document
                StringBuilder text = new();

                // loop over each page of the document
                for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++) {
                    // extract the text from each page and add it to the string build created above
                    var pageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page));
                    text.Append(pageText);
                }

                // load the place we need to check if mentioned from the .dotenv file
                string place = DotEnv.Read()["SEARCHED_PLACE"];

                messageToSend = FindPlaceAndTime(text, place); 
            }
            return messageToSend;
        }

        private static string FindPlaceAndTime(StringBuilder text, string location) {
            string dayPattern = DotEnv.Read()["DAY_PATTERN"];
            string locationPattern = DotEnv.Read()["LOCATION_PATTERN"];

            List<string> timesLocationFound = [];

            string[] lines = text.ToString().Split('\n');

            string currentDay = string.Empty;
            string currentDate = string.Empty;

            foreach (string line in lines) {
                // Match day and date

                Match dayMatch = Regex.Match(line, dayPattern);

                if (dayMatch.Success) {
                    currentDay = dayMatch.Groups[1].Value; 
                    string day = dayMatch.Groups[2].Value;
                    string month =  dayMatch.Groups[3].Value;
                    string year = dayMatch.Groups[4].Success ? dayMatch.Groups[4].Value : "2024";
                    currentDate = $"{day} {month} {year}";    
                }

                Match locationMatch = Regex.Match(line, locationPattern);
                if (locationMatch.Success) {
                    string foundLocation = locationMatch.Groups[1].Value.Trim();

                    if (foundLocation.Contains(location)) {
                        string foundDate = $"{currentDay}, {currentDate}."; 
                        Console.WriteLine(foundDate);
                        timesLocationFound.Add(foundDate);
                    }
                }
            }

            if (timesLocationFound.ToArray().Length == 0) {
                Console.WriteLine($"'{location}' not found in the schedule.");
                return "Nu se ia curentul in saptamana care urmeaza";
            }

            string messageToSend = $"Se ia curentul in {location}: \n";

            foreach (string line in timesLocationFound) {
                messageToSend += line + "\n";
            }

            return messageToSend;
        }
    }
}