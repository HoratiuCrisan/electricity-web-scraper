using dotenv.net;

namespace Electricity_Web_Scraper {
    class Messager (string message) 
    {
        private readonly string _message = message;
        public async Task SendMessage() {
            string botToken = DotEnv.Read()["BOT_TOKEN"];
            string chatId = DotEnv.Read()["CHAT_ID"];

            if (string.IsNullOrEmpty(_message)) {
                Console.WriteLine("No message recived in the Messager");
                return;
            }
            
            using (var client = new HttpClient()) {
                var url = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={_message}";
                var response = await client.GetAsync(url);
                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine(result);
                return;
            }
        }
    }
}