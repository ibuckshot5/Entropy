using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Entropy.Captcha;
using Entropy.Proxy.Pool;
using Newtonsoft.Json;
using Troschuetz.Random;

namespace Entropy
{
    internal class Program
    {
        public static string Status { get; set; }
        
        public static int Created { get; set; }
        
        public static int Failed { get; set; }
        
        public static int Remaining { get; set; }
        
        public static int CurrSolving { get; set; }
        
        // Proxy shit
        public static int ActiveProxies { get; set; }
        
        public static int BenchedProxies { get; set; }
        
        /* 
        // Verification
        public static int Verified { get; set; }
        
        public static int RemainingVerify { get; set; }
        
        // Tutorial
        
        public static int TosAccepted { get; set; }
        
        public static int Level2 { get; set; }
        
        public static int Level5 { get; set; }
        */

        public static void Main(string[] args)
        {
            Run(args);
        }

        public static async void Run(string[] args)
        {
            var random = new TRandom();
            Configuration config;
            try
            {
                config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(
                    Directory.GetCurrentDirectory() + "/" +
                    "config.json"));
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Please create a config.json file to continue.");
                throw;
            }
            Console.Write("Hello and welcome to ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Entropy!\n");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("How many accounts would you like to create today?");
            var acts = int.Parse(Console.ReadLine());
            
            Console.WriteLine($"This generation session will cost you about ${Math.Round(acts * 0.003, 2)} USD " +
                              $"(for {acts} accounts)");

            IProxyPool proxyPool;
            if (File.Exists($"{Directory.GetCurrentDirectory()}/{config.ProxyFile}"))
                proxyPool = new StandardProxyPool(ProxyReader.ReadProxies(config.ProxyFile));
            else
                proxyPool = new GimmeProxyPool();
            
            var captcha = new TwoCaptchaService(config.CaptchaKey);    
            Console.WriteLine("OK! Let's go!");
            var tasks = new List<Task>();
            Status = "OK";
            UpdateStats();
            for (var i = acts - 1; i >= 0; i--)
            {
                tasks.Add(new Task(async () =>
                {
                    var username = random.Choice(config.Usernames.Prefix) + random.Choice(config.Usernames.Root) +
                                   random.Choice(config.Usernames.Suffix);

                    var password = config.Password.UseStaticPassword ? config.Password.StaticPassword : String.Empty;

                    var proxy = proxyPool.NextProxy();
                    
                    var ui = new AccountCreationOptions
                    {
                        CaptchaService = captcha,
                        Dob = $"{random.Next(1970, 2000)}-{FormatPTCNumber(random.Next(1, 12))}-{FormatPTCNumber(random.Next(1, 27))}",
                        Proxy = proxy.ToWebProxy(),
                        Username = username,
                        Password = password
                    };

                    var res = await Creator.Create(ui);

                    if (res.Successful)
                    {
                        Created++;
                        UpdateStats();
                        await Creator.HandleAccountAfterCreation(config, res);
                    }
                    else
                    {
                        Failed++;
                        UpdateStats();
                    }
                }));
            }
            
            // Go.
            await Task.WhenAll(tasks);
        }

        private static string FormatPTCNumber(int num) => num < 10 ? $"0{num}" : num.ToString();

        public static void UpdateStats()
        {
            Console.Clear();
            Console.WriteLine($"Entropy status: {Status}\nAccounts created: {Created}, remaining: {Remaining} " +
                              $"({CurrSolving} solving captcha now)\n\nProxies\n======\nActive proxies: " +
                              $"{ActiveProxies}\nBenched proxies: {BenchedProxies}");
        }
    }
}