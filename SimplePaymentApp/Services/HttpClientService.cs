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
            var encoded = EncodeObject(data);
            var content = new StringContent(encoded);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return Client.PostAsync(url, content);
        }

        private string EncodeObject(object data)
        {
            if (data.GetType().Name.Contains("AnonymousType") == false)
            {
                throw new ArgumentException("Invalid object to encode");
            }

            var props = data.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var prop in props)
            {
                object value = prop.GetValue(data, null);
                if (value != null)
                {
                    AddKeyValuePair(sb, prop.Name.ToLower(), value);
                }
            }

            return sb.ToString();
        }

        private void AddKeyValuePair(StringBuilder sb, string key, object value)
        {
            if (null == value)
                return;

            string reply = String.Empty;
            var charset = Encoding.UTF8;

            try
            {
                key = HttpUtility.UrlEncode(key.ToLower(), charset);

                if (value.GetType().IsEnum)
                {
                    reply = value.ToString().ToLower();
                }
                else if (value.GetType().Equals(typeof(DateTime)))
                {
                    if (value.Equals(DateTime.MinValue))
                    {
                        reply = String.Empty;
                    }
                    else
                    {
                        reply = ((DateTime)value).ToUnixTimestamp().ToString();
                    }
                }
                else
                {
                    reply = HttpUtility.UrlEncode(value.ToString(), charset);
                }

                if (!String.IsNullOrEmpty(reply))
                {
                    if (sb.Length > 0)
                        sb.Append("&");

                    sb.Append($"{key}={reply}");
                }

            }
            catch
            {
                throw new Exception($"Unsupported or invalid character set encoding '{charset}'.");
            }
        }
    }
}
