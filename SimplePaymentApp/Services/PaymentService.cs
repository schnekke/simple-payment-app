using System;
using System.Text;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SimplePaymentApp.Utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SimplePaymentApp.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymillSettings _settings;
        public PaymentService(IOptions<PaymillSettings> settings)
        {
            _settings = settings?.Value;
            if (null == _settings || string.IsNullOrEmpty(_settings.ApiKey))
            {
                throw new Exception("You need to set an API key");
            }
        }

        public string Key => _settings.PublicKey;
        public string Bridge => _settings.BridgeAP;

        public HttpClient Client
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string authInfo =  $"{_settings.ApiKey}:";
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                }

                return _httpClient;
            }
        }

        public async Task<T> CreateWithToken<T>(string token)
        {
            if (String.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token can not be blank");
            }

            var encoded = EncodeObject(new { Token = token });
            var content = new StringContent(encoded);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var requestUri = $"{_settings.ApiAP}/{_resource.ToString().ToLower()}";
            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, content);
            string data = await readReponseMessage(response);

            return JsonConvert.DeserializeObject<SingleResult<T>>(data, _customConverters).Data;
        }

        private HttpClient _httpClient;
        private Resource _resource => Resource.Payments;
        private JsonConverter[] _customConverters = {
            new UnixTimestampConverter(), new StringToNIntConverter()};

        private Task<String> readReponseMessage(HttpResponseMessage response)
        {
            try
            {
                var json = response.Content.ReadAsStringAsync().Result;
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
                    addKeyValuePair(sb, prop.Name.ToLower(), value);
                }
            }

            return sb.ToString();
        }

        private void addKeyValuePair(StringBuilder sb, string key, object value)
        {
            string reply = "";
            var charset = Encoding.UTF8;
            if (null == value) 
                return;
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

                if (!String.IsNullOrEmpty(reply))
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
