using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimplePaymentApp.Services
{
    public interface IPaymentService
    {
        string Key { get; }
        string Bridge { get; }
        HttpClient Client { get; }
        Task<T> CreateWithToken<T>(string token);
    }
}
