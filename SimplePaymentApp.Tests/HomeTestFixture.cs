using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace SimplePaymentApp.Tests
{
    public class HomeTestFixture : IDisposable
    {
        public static string ProjectRootPath => Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "SimplePaymentApp");

        public HomeTestFixture()
        {
            var builder = Program.CreateWebHostBuilder()
                .UseContentRoot(ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            Server = new TestServer(builder);

            Client = Server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");
        }

        public HttpClient Client { get; }
        public TestServer Server { get; }

        public void Dispose()
        {
            Client?.Dispose();
            Server?.Dispose();
        }
    }
}