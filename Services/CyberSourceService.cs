using CyberSourceIntegration.Configurations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace CyberSourceIntegration.Services
{
    public class CyberSourceService : ICyberSourceService
    {
        private readonly CyberSourceSettings _settings;
        private readonly IHttpClientFactory _clientFactory;

        public CyberSourceService(IOptions<CyberSourceSettings> settings, IHttpClientFactory clientFactory)
        {
            _settings = settings.Value;
            _clientFactory = clientFactory;
        }

        public async Task<string> CreatePaymentSessionAsync()
        {
            var client = new RestClient(_settings.ApiUrl);
            var request = new RestRequest("up/v1/sessions", Method.Post);

            var payload = new
            {
                targetOrigins = new[] { "https://www.example.com" },
                allowedCardNetworks = new[] { "VISA", "MASTERCARD", "AMEX" },
                clientVersion = "v2.0"
            };

            request.AddJsonBody(payload);
            var response = await client.ExecuteAsync<RestResponse>(request);

            if (!response.IsSuccessful)
            {
                throw new Exception("Error creating payment session: " + response.Content);
            }

            if (response.Content == null)
            {
                throw new Exception("Response content is null.");
            }

            dynamic jsonResponse = JsonConvert.DeserializeObject(response.Content);
            if (jsonResponse.captureContext == null)
            {
                throw new Exception("Capture context is null.");
            }

            return jsonResponse.captureContext;
        }

        public async Task<string> ProcessPaymentAsync(string transientToken)
        {
            var client = new RestClient(_settings.ApiUrl);
            var request = new RestRequest("payments/v1/authorizations", Method.Post);

            var payload = new
            {
                paymentInformation = new
                {
                    transientToken
                }
            };

            request.AddJsonBody(payload);
            var response = await client.ExecuteAsync<RestResponse>(request);

            if (!response.IsSuccessful)
            {
                throw new Exception("Error processing payment: " + response.Content);
            }

            if (response.Content == null)
            {
                throw new Exception("Response content is null.");
            }

            dynamic jsonResponse = JsonConvert.DeserializeObject(response.Content);
            if (jsonResponse.id == null)
            {
                throw new Exception("Transaction ID is null.");
            }

            return jsonResponse.id;
        }
    }
}
