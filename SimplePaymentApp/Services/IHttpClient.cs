using System.Net.Http;
using System.Threading.Tasks;

namespace SimplePaymentApp.Services
{
    public interface IHttpClient
    {
        void SetAuthHeader(string user, string password = null);
        Task<string> GetStringAsync(string url);
        Task<HttpResponseMessage> PostAsObjAsync(string url, object data);
    }
}