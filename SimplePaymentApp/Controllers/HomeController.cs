using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using SimplePaymentApp.Models;
using Microsoft.AspNetCore.Mvc;
using SimplePaymentApp.Services;

namespace SimplePaymentApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPaymentService _service;

        public HomeController(IPaymentService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            var model = new PaymentModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(PaymentModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var response = _service.Client.GetAsync($@"{
                    _service.Bridge }/?transaction.mode=CONNECTOR_TEST&channel.id={
                    _service.Key }&jsonPFunction=paymilljstests&account.number={
                    viewModel.Number }&account.expiry.month={
                    viewModel.Valid.Value.Month }&account.expiry.year={
                    viewModel.Valid.Value.Year }&account.verification={
                    viewModel.Code }&account.holder={
                    Uri.EscapeUriString(viewModel.Name) }&presentation.amount3D={
                    viewModel.Quantity }&presentation.currency3D=EUR").Result;

                var pattern = "(tok_)[a-z|0-9]+";
                var content = response.Content.ReadAsStringAsync().Result;
                if (Regex.Matches(content, pattern).Count > 0)
                {
                    var token = Regex.Matches(content, pattern)[0].Value;
                    Payment payment = await _service.CreateWithToken<Payment>(token);
                    ViewBag.Result = payment.Id == String.Empty ? "Error in payment" 
                        : "Payment OK";
                }
            }
            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
