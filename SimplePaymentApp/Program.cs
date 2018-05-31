using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SimplePaymentApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(params string[] args) =>
            WebHost.CreateDefaultBuilder(args);

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
