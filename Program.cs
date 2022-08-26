using log4net;
using log4net.Config;
using NessusApis;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static async Task Main(string[] args)
        {
            try {
                JObject json_object;
                string response;
                StringContent stringContent;
                string json;

                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

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

                json_object = JObject.Parse(await post(httpClient, "session", stringContent));


                httpClient.DefaultRequestHeaders.Add("X-Cookie", "token=" + json_object["token"]);
                httpClient.DefaultRequestHeaders.Add("username", username);
                httpClient.DefaultRequestHeaders.Add("password", password);

                json_object = JObject.Parse(await get(httpClient, "scans"));

                string scanId = (string)json_object["scans"][0]["id"];
                url = "scans/" + scanId + "/export";
                json = "{\"format\":\"nessus\"}";

                httpClient.DefaultRequestHeaders.Add("ScanID", scanId);
                stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                
                json_object = JObject.Parse(await post(httpClient, url, stringContent));
                string fileId = (string)json_object["file"];

                System.Threading.Thread.Sleep(10000);

                httpClient.DefaultRequestHeaders.Add("FileID", fileId);
                url += "/" + fileId + "/download";
                response = await get(httpClient, url);

                string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

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

        static async Task<string> get(HttpClient httpClient, string url)
        {
            try {
                
                var data = await httpClient.GetAsync(url);
                var content = data.Content;
                var response = await content.ReadAsStringAsync();
                return response;            
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }
        static async Task<string> post(HttpClient httpClient, string url, StringContent strContent)
        {
            try {
                var data = await httpClient.PostAsync(url, strContent);
                var content = data.Content;
                var response = await content.ReadAsStringAsync();
                return response;
            }
            catch(Exception e)
            {
                
                return e.Message;
            }
}
    }
}
