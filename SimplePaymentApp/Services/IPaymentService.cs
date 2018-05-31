using System;
using System.Threading.Tasks;

namespace SimplePaymentApp.Services
{
    public interface IPaymentService
    {
        Task<String> GetToken(Models.TokenModel model);
        Task<T> CreateWithToken<T>(string token);
    }
}
