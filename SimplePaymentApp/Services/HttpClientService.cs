using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using System;
using System.Web;

namespace SimplePaymentApp.Services
{
    public class HttpClientService : IHttpClient
    {
        
        private HttpClient _httpClient;
        protected virtual HttpClient Client
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }

                return _httpClient;
            }
        }

        public void SetAuthHeader(string user, string password = null)
        {
            var authInfo = Convert.ToBase64String(Encoding.Default.GetBytes($"{user}:"));
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
        }

        public async Task<string> GetStringAsync(string url)
        {
            HttpResponseMessage response = await Client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

        public Task<HttpResponseMessage> PostAsObjAsync(string url, object data)
        {
            return Client.PostAsObjAsync(url, data);
        }
    }
}
