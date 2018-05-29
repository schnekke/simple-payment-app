using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using SimplePaymentApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SimplePaymentApp.Extensions;

namespace SimplePaymentApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var model = new PaymentModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(PaymentModel viewModel)
        {
            var paymill = new PaymillContext(_configuration["PayMill:ApiKey"]);
            var response = paymill.Client.GetAsync($@"{
                _configuration["PayMill:BridgeAP"] }/?transaction.mode=CONNECTOR_TEST&channel.id={
                _configuration["PayMill:PublicKey"] }&jsonPFunction=paymilljstests&account.number={
                viewModel.Number }&account.expiry.month={
                viewModel.Valid.Value.Month }&account.expiry.year={
                viewModel.Valid.Value.Year }&account.verification={
                viewModel.Code }&account.holder={
                Uri.EscapeUriString(viewModel.Name) }&presentation.amount3D={ 
                viewModel.Quantity }&presentation.currency3D=EUR").Result;
            
            string token = null;
            var pattern = "(tok_)[a-z|0-9]+";
            var content = response.Content.ReadAsStringAsync().Result;
            if (Regex.Matches(content, pattern).Count > 0)
            {
                token = Regex.Matches(content, pattern)[0].Value;
            }
                                                   
            Payment payment = await paymill.CreateWithTokenAsync(token);
            viewModel.Result = payment.Id == String.Empty;

            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
