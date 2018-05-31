using System;
using System.Text;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using SimplePaymentApp.Utils;
using SimplePaymentApp.Models;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SimplePaymentApp.Services
{
    public class PaymentService : IPaymentService
    {
        private IHttpClient _client;
        private readonly PaymillSettings _settings;

        public PaymentService(IOptions<PaymillSettings> settings, IHttpClient client)
        {
            _client = client;
            _settings = settings?.Value;
            if (String.IsNullOrEmpty(_settings?.ApiKey))
            {
                throw new ArgumentException("You need to set an API key");
            }

            _client.SetAuthHeader(_settings.ApiKey);
        }

        public async Task<T> CreateWithToken<T>(string token)
        {
            if (String.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token can not be blank");
            }

            var requestUri = $"{_settings.ApiAP}/{Resource.Payments.ToString().ToLower()}";
            var response = await _client.PostAsObjAsync(requestUri, new { Token = token });

            string data = await ReadResponseMessage(response);
            return JsonConvert.DeserializeObject<SingleResult<T>>(data, _customConverters).Data;
        }

        public async Task<string> GetToken(TokenModel model)
        {
            string result = null;
            if (model != null)
            {
                var tokenUrl = $@"{_settings.BridgeAP}/?transaction.mode={
                    model.Mode}&channel.id={_settings.PublicKey}&jsonPFunction={
                    model.PFunction}&account.number={model.Account}&account.expiry.month={
                    model.Month}&account.expiry.year={model.Year}&account.verification={
                    model.Verification}&account.holder={model.Holder}&presentation.amount3D={
                    model.Amount3D}&presentation.currency3D={model.Currency3D}";
                var content = await _client.GetStringAsync(tokenUrl);

                var pattern = "(tok_)[a-z|0-9]+";
                if (Regex.Matches(content, pattern).Count > 0)
                {
                    result = Regex.Matches(content, pattern)[0].Value;
                }
            }

            return await Task.Run(() => {
                return result;
            });
        }

        private JsonConverter[] _customConverters = {
            new UnixTimestampConverter(), new StringToNIntConverter()};

        private async Task<string> ReadResponseMessage(HttpResponseMessage response)
        {
            string result = String.Empty;
            try
            {
                var stream = await response.Content.ReadAsStreamAsync();
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }

                var jsonArray = JObject.Parse(result);
                if (jsonArray["error"] != null)
                {
                    var error = jsonArray["error"].ToString();
                    throw new Exception(error);
                }
            }
            catch (Exception exc)
            {
                throw new Exception(exc.Message);
            }

            return await Task.Run(() => {
                return result;
            });
        }
    }
}