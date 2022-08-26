using NessusApis;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    class Program
    {
        static async Task Main(string[] args)
        {
            HttpResponseMessage data;
            HttpContent content;
            string response;

            //Blocked SSL errors
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            var httpClient = new HttpClient(clientHandler);

            Console.WriteLine("Please enter Nessus api URL: ");
            var url = Console.ReadLine();
            Console.WriteLine("Please enter Nessus username: ");
            var username = Console.ReadLine();
            Console.WriteLine("Please enter Nessus password: ");
            var password = Console.ReadLine();

            UserInfo userInfo = new UserInfo(username, password);

            var json = JsonSerializer.Serialize(userInfo);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.BaseAddress = new Uri(url);

            data = await httpClient.PostAsync("session", stringContent);
            content = data.Content;
            response = await content.ReadAsStringAsync();

            Console.WriteLine(response);
        }
    }
}
