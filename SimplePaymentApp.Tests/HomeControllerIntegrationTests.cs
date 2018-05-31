using System;
using HtmlAgilityPack;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using System.Linq;

namespace SimplePaymentApp.Tests
{
    public class HomeControllerIntegrationTests : IClassFixture<HomeTestFixture>
    {
        private readonly HomeTestFixture _fixture;
        public HomeControllerIntegrationTests(HomeTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task IndexUsingFixtureReturnsHtml()
        {
            // Act
            var response = await _fixture.Client.GetAsync("/");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Simple Payment App", content);
        }

        [Fact]
        public async Task IndexReturnsHtml()
        {
            var builder = new WebHostBuilder()
                .UseContentRoot(HomeTestFixture.ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                // Act
                var response = await client.GetAsync("/");

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                Assert.Contains("Simple Payment App", content);
            }
        }

        [Fact]
        public async Task IndexUsingCreateDefaultBuilderReturnsHtml()
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseContentRoot(HomeTestFixture.ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                // Act
                var response = await client.GetAsync("/");

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                Assert.Contains("Simple Payment App", content);
            }
        }

        [Fact]
        public async Task IndexUsingProgramReturnsHtml()
        {
            var builder = Program.CreateWebHostBuilder()
                .UseContentRoot(HomeTestFixture.ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            using (var server = new TestServer(builder))
            {

                var client = server.CreateClient();

                // Act
                var response = await client.GetAsync("/");

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                Assert.Contains("Simple Payment App", content);
            }
        }

        [Fact]
        public async Task NoSuccessWhenPostedUnauthorized()
        {
            var builder = Program.CreateWebHostBuilder()
                .UseContentRoot(HomeTestFixture.ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<Startup>();

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                // Act
                var response = await client.PostAsJsonAsync("/", new Models.PaymentModel());

                Assert.False(response.IsSuccessStatusCode);
            }
        }

        [Fact]
        public async Task SuccessWhenPostedAuthorized()
        {
            var builder = Program.CreateWebHostBuilder()
                .UseContentRoot(HomeTestFixture.ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                // Act
                var response = await client.PostAsJsonAsync("/", new Models.PaymentModel());

                Assert.True(response.IsSuccessStatusCode);
            }
        }

        [Fact]
        public async Task ErrorWhenNoNumber()
        {
            var builder = Program.CreateWebHostBuilder()
                .UseContentRoot(HomeTestFixture.ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                // Arrange
                var model = new Models.PaymentModel
                {
                    Code = "111",
                    Name = "Chris Hansen",
                    Number = null,
                    Valid = new DateTime(2020, 1, 1)
                };

                // Act
                var response = await client.PostAsJsonAsync("/", model);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();

                var doc = new HtmlDocument();

                doc.Load(stream);
                HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//input[@class='form-control input-validation-error']");

                // Assert
                Assert.Equal(links.Count, 1);
                Assert.Equal(links[0].Attributes.FirstOrDefault(f => f.Name == "name")?.Value, "Number");
            }
        }

        [Fact]
        public async Task ErrorWhenZeroQuantity()
        {
            var builder = Program.CreateWebHostBuilder()
                .UseContentRoot(HomeTestFixture.ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                // Arrange
                var model = new Models.PaymentModel
                {
                    Code = "111",
                    Name = "Chris Hansen",
                    Quantity = 0.0,
                    Valid = new DateTime(2020, 1, 1)
                };

                // Act
                var response = await client.PostAsJsonAsync("/", model);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();

                var doc = new HtmlDocument();

                doc.Load(stream);
                HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//input[@class='form-control input-validation-error']");

                // Assert
                Assert.Equal(links.Count, 1);
                Assert.Equal(links[0].Attributes.FirstOrDefault(f => f.Name == "name")?.Value, "Quantity");
            }
        }

        [Fact]
        public async Task ErrorWhenPastDate()
        {
            var builder = Program.CreateWebHostBuilder()
                .UseContentRoot(HomeTestFixture.ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                // Arrange
                var model = new Models.PaymentModel
                {
                    Code = "111",
                    Name = "Chris Hansen",
                    Valid = new DateTime(2010, 1, 1)
                };

                // Act
                var response = await client.PostAsJsonAsync("/", model);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();

                var doc = new HtmlDocument();

                doc.Load(stream);
                HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//input[@class='form-control input-validation-error']");

                // Assert
                Assert.Equal(links.Count, 1);
                Assert.Equal(links[0].Attributes.FirstOrDefault(f => f.Name == "name")?.Value, "Valid");
            }
        }

        [Fact]
        public async Task ErrorWhenNameIsNumeric()
        {
            var builder = Program.CreateWebHostBuilder()
                .UseContentRoot(HomeTestFixture.ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                // Arrange
                var model = new Models.PaymentModel
                {
                    Code = "111",
                    Name = "Chris 111",
                    Valid = new DateTime(2020, 1, 1)
                };

                // Act
                var response = await client.PostAsJsonAsync("/", model);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();

                var doc = new HtmlDocument();

                doc.Load(stream);
                HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//input[@class='form-control input-validation-error']");

                // Assert
                Assert.Equal(links.Count, 1);
                Assert.Equal(links[0].Attributes.FirstOrDefault(f => f.Name == "name")?.Value, "Name");
            }
        }

        [Fact]
        public async Task ErrorWhenCodeIsAlphabetic()
        {
            var builder = Program.CreateWebHostBuilder()
                .UseContentRoot(HomeTestFixture.ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                // Arrange
                var model = new Models.PaymentModel
                {
                    Code = "ooo",
                    Name = "Chris Hansen",
                    Valid = new DateTime(2020, 1, 1)
                };

                // Act
                var response = await client.PostAsJsonAsync("/", model);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();

                var doc = new HtmlDocument();

                doc.Load(stream);
                HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//input[@class='form-control input-validation-error']");

                // Assert
                Assert.Equal(links.Count, 1);
                Assert.Equal(links[0].Attributes.FirstOrDefault(f => f.Name == "name")?.Value, "Code");
            }
        }

        [Fact]
        public async Task SuccessfulPayment()
        {
            var builder = Program.CreateWebHostBuilder()
                .UseContentRoot(HomeTestFixture.ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                // Arrange
                var model = new Models.PaymentModel
                {
                    Name = "Chris Hansen",
                    Code = "111",
                    Valid = new DateTime(2020, 1, 1)
                };

                // Act
                var response = await client.PostAsJsonAsync("/", model);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                Assert.Contains("Payment OK", content);
            }
        }

        [Fact]
        public async Task UnsuccessfulPayment()
        {
            var builder = Program.CreateWebHostBuilder()
                .UseContentRoot(HomeTestFixture.ProjectRootPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                // Arrange
                var model = new Models.PaymentModel
                {
                    Name = "Chris Hansen",
                    Code = "111",
                    Number = "000",
                    Valid = new DateTime(2020, 1, 1)
                };

                // Act
                var response = await client.PostAsJsonAsync("/", model);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                Assert.Contains("Error in payment", content);
            }
        }
    }
}