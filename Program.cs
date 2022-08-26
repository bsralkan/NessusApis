using NessusApis;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
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
            try {
                HttpResponseMessage data;
                HttpContent content;
                JObject json_object;
                string response;
                StringContent stringContent;
                string json;

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

                json = JsonSerializer.Serialize(userInfo);
                stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                httpClient.BaseAddress = new Uri(url);

                data = await httpClient.PostAsync("session", stringContent);
                content = data.Content;
                response = await content.ReadAsStringAsync();

                json_object = JObject.Parse(response);

                httpClient.DefaultRequestHeaders.Add("X-Cookie", "token=" + json_object["token"]);
                httpClient.DefaultRequestHeaders.Add("username", username);
                httpClient.DefaultRequestHeaders.Add("password", password);
                Console.WriteLine(httpClient.DefaultRequestHeaders);

                data = await httpClient.GetAsync("scans");
                content = data.Content;
                response = await content.ReadAsStringAsync();
                json_object = JObject.Parse(response);

                string scanId = (string)json_object["scans"][0]["id"];
                url = "scans/" + scanId + "/export";
                json = "{\"format\":\"nessus\"}";

                httpClient.DefaultRequestHeaders.Add("ScanID", scanId);
                stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                data = await httpClient.PostAsync(url, stringContent);
                content = data.Content;
                response = await content.ReadAsStringAsync();
                json_object = JObject.Parse(response);
                string fileId = (string)json_object["file"];

                System.Threading.Thread.Sleep(10000);

                httpClient.DefaultRequestHeaders.Add("FileID", fileId);
                url += "/" + fileId + "/download";
                data = await httpClient.GetAsync(url);
                content = data.Content;
                response = await content.ReadAsStringAsync();

                //string docPath = AppDomain.CurrentDomain.BaseDirectory;
                string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                Console.WriteLine(Path.Combine(projectDirectory, "WriteTextAsync.txt"));

                using (StreamWriter outputFile = new StreamWriter(Path.Combine(projectDirectory, "WriteTextAsync.txt")))
                {
                    await outputFile.WriteAsync(response);
                }
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
