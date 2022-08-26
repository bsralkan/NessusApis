using NessusApis;
using System;
using System.Net.Http;

namespace ConsoleApp4
{
    class Program
    {
        static void Main(string[] args)
        {
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
        }
    }
}
