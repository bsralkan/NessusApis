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
        }
    }
}
