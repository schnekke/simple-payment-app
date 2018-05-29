using System;
using System.Text;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SimplePaymentApp.Models;
using SimplePaymentApp.Utils;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SimplePaymentApp.Extensions
{
    public class PaymillContext
    {
        public PaymillContext(String apiKey)
        {
            ApiUrl = @"https://api.paymill.com/v2.1";
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("You need to set an API key", "apiKey");
            }
            ApiKey = apiKey;
            _resource = Resource.Payments;
        }

        public static string ApiKey { get; private set; }
        public static string ApiUrl { get; private set; }
        private static JsonConverter[] customConverters = { 
            new UnixTimestampConverter(), new StringToNIntConverter()};
        private HttpClient _httpClient;
        public HttpClient Client
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var authInfo =  $"{ApiKey}:";
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                }

                return _httpClient;
            }
        }

        private Resource _resource;
        public async Task<Payment> CreateWithTokenAsync(string token)
        {
            if (String.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token can not be blank");
            }

            var encoded = EncodeObject(new { Token = token });
            var content = new StringContent(encoded);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var requestUri = $"{ApiUrl}/{_resource.ToString().ToLower()}";
            HttpResponseMessage response = _httpClient.PostAsync(requestUri, content).Result;
            String data = await readReponseMessage(response);

            return JsonConvert.DeserializeObject<SingleResult<Payment>>(data, customConverters).Data;
        }

        private Task<String> readReponseMessage(HttpResponseMessage response)
        {
            try
            {
                String json = response.Content.ReadAsStringAsync().Result;
                var jsonArray = JObject.Parse(json);
                if (jsonArray["data"] != null)
                {
                    return response.Content.ReadAsStringAsync();
                }
                else if (jsonArray["error"] != null)
                {
                    string error = jsonArray["error"].ToString();
                    throw new Exception(error);
                }
            }
            catch (System.IO.IOException exc)
            {
                throw new Exception(exc.Message);
            }

            return Task.Run(() => {
                return String.Empty;
            });
        }

        private string EncodeObject(Object data)
        {
            if (data.GetType().Name.Contains("AnonymousType") == false)
            {
                throw new ArgumentException("Invalid object to encode");
            }

            var props = data.GetType().GetProperties();
            StringBuilder sb = new StringBuilder();
            foreach (var prop in props)
            {
                object value = prop.GetValue(data, null);
                if (value != null)
                {
                    addKeyValuePair(sb, prop.Name.ToLower(), value);
                }
            }

            return sb.ToString();
        }

        private void addKeyValuePair(StringBuilder sb, string key, object value)
        {
            string reply = "";
            var charset = Encoding.UTF8;
            if (value == null) return;
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
                        reply = "";
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

                if (!string.IsNullOrEmpty(reply))
                {
                    if (sb.Length > 0)
                        sb.Append("&");

                    sb.Append(String.Format("{0}={1}", key, reply));
                }

            }
            catch
            {
                throw new Exception(
                    String.Format("Unsupported or invalid character set encoding '{0}'.", charset));
            }
        }
    }
}
