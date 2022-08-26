using log4net;
using log4net.Config;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace NessusApis
{
    class Program
    {
        string aa;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static HttpClient httpClient;
        private static JObject json_object;
        private static StringContent stringContent;
        private static UserInfo userInfo;
        private static string response, json, url, username, password, scanId, fileId;
        static async Task Main(string[] args)
        {
            try {                

                //log4net
                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                //Blocked SSL errors
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                httpClient = new HttpClient(clientHandler);

                Console.WriteLine("Please enter Nessus api URL: ");
                url = Console.ReadLine();
                Console.WriteLine("Please enter Nessus username: ");
                username = Console.ReadLine();
                Console.WriteLine("Please enter Nessus password: ");
                password = Console.ReadLine();

                userInfo = new UserInfo(username, password);

                var res = await addToken();
                res = await addScanId();
                res = await addFileId();

                // waiting for creating file
                System.Threading.Thread.Sleep(10000);

                res = await getFile();
                res = await writeFile();

            } 
            catch (Exception e)
            {
                log.Error("Program.cs Main function : " + e.Message);
                Environment.Exit(-1);
            }
        }

        static async Task<string> addToken()
        {
            try {
                json = JsonSerializer.Serialize(userInfo);
                stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                httpClient.BaseAddress = new Uri(url);

                json_object = JObject.Parse(await post("session"));
                httpClient.DefaultRequestHeaders.Add("X-Cookie", "token=" + json_object["token"]);
                httpClient.DefaultRequestHeaders.Add("username", username);
                httpClient.DefaultRequestHeaders.Add("password", password);
                return "success";
            }
            catch (Exception e)
            {
                log.Error("Program.cs addToken function : " + e.Message);
                throw new Exception();
            }
        }

        static async Task<string> addScanId()
        {
            try {
                json_object = JObject.Parse(await get("scans"));

                scanId = (string)json_object["scans"][0]["id"];
                httpClient.DefaultRequestHeaders.Add("ScanID", scanId);

                return "success";
            }
            catch (Exception e)
            {
                log.Error("Program.cs addScanId function : " + e.Message);
                throw new Exception();
            }
        }

        static async Task<string> addFileId()
        {
            try {
                json = "{\"format\":\"nessus\"}";
                url = "scans/" + scanId + "/export";
                stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                json_object = JObject.Parse(await post(url));
                fileId = (string)json_object["file"];
                httpClient.DefaultRequestHeaders.Add("FileID", fileId);

                return "success";
            }
            catch (Exception e)
            {
                log.Error("Program.cs addFileId function : " + e.Message);
                throw new Exception();
            }
        }

        static async Task<string> getFile()
        {
            try {

                url += "/" + fileId + "/download";
                response = await get(url);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);
                response = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);

                return "success";
            }
            catch (Exception e)
            {
                log.Error("Program.cs getFile function : " + e.Message);
                throw new Exception();
            }
        }

        static async Task<string> writeFile()
        {
            try {

                string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

                using (StreamWriter outputFile = new StreamWriter(Path.Combine(projectDirectory, "WriteTextAsync.json")))
                {
                    await outputFile.WriteAsync(response);
                }

                return "success";
            }
            catch (Exception e)
            {
                log.Error("Program.cs writeFile function : " + e.Message);
                throw new Exception();
            }
        }

        static async Task<string> get(string url)
        {
            try {                
                var data = await httpClient.GetAsync(url);
                if (data.StatusCode == System.Net.HttpStatusCode.NotFound || data.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new Exception();
                }
                var content = data.Content;
                return await content.ReadAsStringAsync();  
            }
            catch(Exception e)
            {
                log.Error("Program.cs get function : " + e.Message);
                throw new Exception();
            }
        }
        static async Task<string> post(string url)
        {
            try {
                var data = await httpClient.PostAsync(url, stringContent);
                if(data.StatusCode == System.Net.HttpStatusCode.NotFound || data.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new Exception();
                }
                var content = data.Content;
                return await content.ReadAsStringAsync();
            }
            catch(Exception e)
            {
                log.Error("Program.cs post function : " + e.Message);
                throw new Exception();
            }
        }

    }
}
