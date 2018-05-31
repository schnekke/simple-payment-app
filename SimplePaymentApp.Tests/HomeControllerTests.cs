using System;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SimplePaymentApp.Models;

namespace SimplePaymentApp.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<Services.IPaymentService> _service = new Mock<Services.IPaymentService>();
        private readonly Mock<AutoMapper.IMapper> _mapper = new Mock<AutoMapper.IMapper>();

        private Controllers.HomeController _ctrl;

        public readonly static string ValidToken = "tok_d4958f01829d290224d369ff3d99";
        private readonly string _validAccount = "4111111111111111";

        private PaymentModel _model;

        public HomeControllerTests()
        {
            _service.Setup(x => x.GetToken(It.IsAny<TokenModel>()))
                .Returns((TokenModel m) => 
                    Task.FromResult<String>(m.Account == _validAccount ? ValidToken : null));

            _service.Setup(x => x.CreateWithToken<Payment>(It.IsAny<String>()))
                    .Returns((string t) => Task.FromResult<Payment>(new Payment {
                Id = t == ValidToken ? "OK" : String.Empty}));

            _model = new PaymentModel {
                Name = "Chris Hansen",
                Code = "111",
                Valid = new DateTime(2020, 1, 1)
            };

            _mapper.Setup(m => m.Map<TokenModel>(It.IsAny<PaymentModel>()))
                .Returns((PaymentModel p) => new TokenModel {
                    Account = p.Number,
                    Holder = p.Name,
                    Month = p.Valid.Value.Month,
                    Year = p.Valid.Value.Year,
                    Verification = Convert.ToInt32(p.Code),
                    Amount3D = p.Quantity
                });

            _ctrl = new Controllers.HomeController(_service.Object, _mapper.Object);
        }

        [Fact]
        public void IndexReturnsViewModel()
        {
            var result = _ctrl.Index();
            Assert.IsType<ViewResult>(result);

            var viewModel = (result as ViewResult).Model;
            Assert.IsType<PaymentModel>(viewModel);
        }

        [Fact]
        public async Task PostReturnsOk()
        {
            var response = await _ctrl.Index(_model);
            Assert.IsType<ViewResult>(response);

            var vr = response as ViewResult;
            Assert.NotNull(vr);
            Assert.Contains("OK", vr?.ViewData["Result"]?.ToString());
        }

        [Fact]
        public async Task PostReturnsError()
        {
            _model.Number = "000";

            var response = await _ctrl.Index(_model);
            Assert.IsType<ViewResult>(response);

            var vr = response as ViewResult;
            Assert.NotNull(vr);
            Assert.Contains("Error", vr?.ViewData["Result"]?.ToString());
        }
    }
}