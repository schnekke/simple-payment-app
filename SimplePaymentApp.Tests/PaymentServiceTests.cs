using System;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using SimplePaymentApp.Services;
using System.Net.Http;
using Moq.Protected;

namespace SimplePaymentApp.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IOptions<PaymillSettings>> _settings = new Mock<IOptions<PaymillSettings>>();
        private readonly Mock<IHttpClient> _httpClient = new Mock<IHttpClient>();

        private PaymillSettings _paymill;

        public PaymentServiceTests()
        {
            _paymill = new PaymillSettings {
                ApiAP = "",
                BridgeAP = ""
            };

            _paymill.ApiKey = _paymill.PublicKey = "11921079858549680d11a1d09f9ab123";
        }

        [Fact]
        public void ThrowsExceptionIfNoPrivateKey()
        {
            _paymill.ApiKey = null;
            _settings.Setup(x => x.Value).Returns(_paymill);

            Assert.Throws<ArgumentException>(() => new PaymentService(_settings.Object, _httpClient.Object));
        }

        [Fact]
        public async Task ThrowsExceptionIfNoToken()
        {
            _settings.Setup(x => x.Value).Returns(_paymill);
            var service = new PaymentService(_settings.Object, _httpClient.Object);
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateWithToken<String>(" "));
        }

        [Fact]
        public async Task ThrowsExceptionIfBrokenResponse()
        {
            _settings.Setup(x => x.Value).Returns(_paymill);
            var content = new Mock<HttpContent>();
            content.Protected()
                .Setup<Task<System.IO.Stream>>("CreateContentReadStreamAsync")
                .Returns(Task.FromResult<System.IO.Stream>(null))
                .Verifiable();
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = content.Object
            };
            _httpClient.Setup(x => x.PostAsObjAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.FromResult<HttpResponseMessage>(response));
            var service = new PaymentService(_settings.Object, _httpClient.Object);
            await Assert.ThrowsAsync<Exception>(() =>
                service.CreateWithToken<Models.Payment>(HomeControllerTests.ValidToken));
        }

        [Fact]
        public async Task ThrowsExceptionIfJsonError()
        {
            _settings.Setup(x => x.Value).Returns(_paymill);
            var content = new Mock<HttpContent>();
            content.Protected()
                .Setup<Task<System.IO.Stream>>("CreateContentReadStreamAsync")
                .Returns(Task.FromResult(@"{""error"":{""id"":""""}}".GenerateStream()))
                .Verifiable();
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = content.Object
            };
            _httpClient.Setup(x => x.PostAsObjAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.FromResult<HttpResponseMessage>(response));
            var service = new PaymentService(_settings.Object, _httpClient.Object);
            await Assert.ThrowsAsync<Exception>(() => 
                service.CreateWithToken<Models.Payment>(HomeControllerTests.ValidToken));
        }

        [Fact]
        public async Task ReturnsPaymentOnValidToken()
        {
            _settings.Setup(x => x.Value).Returns(_paymill);
            var content = new Mock<HttpContent>();
            content.Protected()
                .Setup<Task<System.IO.Stream>>("CreateContentReadStreamAsync")
                .Returns(Task.FromResult(@"{""data"":{""id"":""OK""}}".GenerateStream()))
                .Verifiable();
            var response = new HttpResponseMessage {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = content.Object 
            };
            _httpClient.Setup(x => x.PostAsObjAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.FromResult<HttpResponseMessage>(response));
            var service = new PaymentService(_settings.Object, _httpClient.Object);
            var payment = await service.CreateWithToken<Models.Payment>(HomeControllerTests.ValidToken);
            Assert.NotNull(payment);
            Assert.Equal(payment.Id, "OK");
        }

        [Fact]
        public async Task ReturnsNullIfNoModel()
        {
            _settings.Setup(x => x.Value).Returns(_paymill);
            var service = new PaymentService(_settings.Object, _httpClient.Object);
            var token = await service.GetToken(null);
            Assert.Null(token);
        }

        [Fact]
        public async Task ReturnsNullIfNoToken()
        {
            _settings.Setup(x => x.Value).Returns(_paymill);
            _httpClient.Setup(x => x.GetStringAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<String>(""));
            
            var service = new PaymentService(_settings.Object, _httpClient.Object);
            var token = await service.GetToken(new Models.TokenModel());
            Assert.Null(token);
        }

        [Fact]
        public async Task ReturnsValidToken()
        {
            _settings.Setup(x => x.Value).Returns(_paymill);
            _httpClient.Setup(x => x.GetStringAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<String>(HomeControllerTests.ValidToken));
            
            var service = new PaymentService(_settings.Object, _httpClient.Object);
            var token = await service.GetToken(new Models.TokenModel());
            Assert.Equal(token, HomeControllerTests.ValidToken);
        }
    }
}
