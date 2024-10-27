using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.IO;
using dotenv.net;
using System.Net;
using System.Threading.Tasks;

namespace Electricity_Web_Scraper {
    public class Scraper {
        public  async Task ScrapeWebPage() {
             // Load the env file to get the web page specifications
            DotEnv.Load();
            
            // get the url from a dotenv file
            String url = DotEnv.Read()["WEB_PAGE_URL"];

            // create a new http client in order to access the web page
            var httpClient = new HttpClient();

            try {
                // send a get request in order to fetch the HTML content
                // from the url of the web page

                // TODO: FIX THE URL XD
                var htmlContent = await httpClient.GetStringAsync(url);

                // throw an exception if the content could not be fetched
                if (htmlContent == null) {
                    Console.WriteLine("Unable to send the request to the web page");
                    return;
                }

                // create a new html document and load the html into it
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent.ToString());

                Console.WriteLine(htmlDocument);

                // extract the year from the html document to check if it mathces with the current year
                var pdfDocumentElement = htmlDocument.DocumentNode.SelectSingleNode("//a[@class='link-pdf']");
                
                // throw and error if the pdf file element could not be fetched
                if (pdfDocumentElement == null) {
                    Console.WriteLine("The pdf file element could not be fetched");
                    return;
                }

                // set a default download file name
                string downloadFileName = "electricity.pdf";

                // store the pdf link into a variable
                var pdfDocumentLink = pdfDocumentElement.GetAttributeValue("href", String.Empty);
                
                // throw an error if the pdf link could not be found
                if (string.IsNullOrEmpty(pdfDocumentLink)) {
                    Console.WriteLine("The pdf file link could not be found");
                    return;
                }

                // split the pdf document link in order to get the name of the pdf file
                string[] pdfUrlTokenized = pdfDocumentLink.Split('/');

                // if the array is not empty save the last element of the array
                // since it should always be the name of the pdf document
                if (pdfUrlTokenized.Length != 0) {
                    string lastToken = pdfUrlTokenized[^1];

                    // check if the string ends with the extention .pdf 
                    // replace the default download file name with the new fetched name
                    if (lastToken.EndsWith(".pdf")) {
                        downloadFileName = lastToken;
                    }
                }
                
                // add the web page name to the link in order to have access to the 
                // specific pdf document
                string pdfLinkUrl = DotEnv.Read()["WEB_PAGE_NAME"] + pdfDocumentLink;

                if (string.IsNullOrEmpty(pdfLinkUrl)) {
                    Console.WriteLine("The pdf URL could not be found");
                    return;
                }

                // set the file name for the downloaded pdf file
                bool downloadStatus = await DownloadFile(pdfLinkUrl, httpClient, downloadFileName);

                // if the file was not downloaded throw an error and stop the execution
                if (!downloadStatus) {
                    Console.WriteLine("The pdf file could not be downloaded");
                    return;
                }

                // create a new instance of the parser object with our downloaded file
                Parser pdfParser = new(downloadFileName);

                // parse the pdf file
                string receivedMessage = pdfParser.ReadPdfFile();

                // pass the message to the Messager in order to send it to the contacts
                Messager messager = new(receivedMessage);
                await messager.SendMessage();

            } catch (Exception exception) {
                Console.WriteLine("Failed to extract the html from the page: " + exception.ToString());
            } finally { 
                httpClient.Dispose();
            }
        }

        static async Task<bool> DownloadFile(string fileUrl, HttpClient httpClient, string downloadFileName) {
            try {
                using var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Specify the path where the PDF will be saved
                var currentPath = Path.Combine(Environment.CurrentDirectory, downloadFileName);

                // Download the file content and save it locally
                using var ms = await response.Content.ReadAsStreamAsync();
                using var fs = File.Create(currentPath);
                await ms.CopyToAsync(fs);

                // Flush the file stream to make sure everything is written
                await fs.FlushAsync();

                Console.WriteLine($"File downloaded and saved as {downloadFileName}");

                return true;
            } catch (Exception ex) {
                Console.WriteLine($"Error downloading the file: {ex.Message}");
                return false;
            }
        }
    }
}