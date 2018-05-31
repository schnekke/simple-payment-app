using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using SimplePaymentApp.Models;
using Microsoft.AspNetCore.Mvc;
using SimplePaymentApp.Services;
using AutoMapper;

namespace SimplePaymentApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPaymentService _service;
        private readonly IMapper _mapper; 

        public HomeController(IPaymentService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
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
                var tokenModel = _mapper.Map<TokenModel>(viewModel);
                await Process(tokenModel);
            }
            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [NonAction]
        private async Task Process(TokenModel model)
        {
            ViewBag.Result = "Error in payment";

            var token = await _service.GetToken(model);
            if (!String.IsNullOrEmpty(token))
            {
                Payment payment = await _service.CreateWithToken<Payment>(token);
                if (!String.IsNullOrEmpty(payment.Id))
                {
                    ViewBag.Result = "Payment OK";
                }
            }
        }
    }
}
